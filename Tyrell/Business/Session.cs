using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tyrell.Business
{
    public static class Session
    {
        private static readonly string[] _csrfPath = { "csrf" };

        private static string _cookie = "";
        private static string _token = "";
        private static DateTime _tokenValidTo;

        public static string Cookie => _cookie;

        //logs in with username and password, setting the cookie in the process
        public static async Task Login()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                    parameters.Add(new KeyValuePair<string, string>("login", Constants.Username));
                    parameters.Add(new KeyValuePair<string, string>("password", Constants.Password));

                    var uri = new Uri($"{Constants.ForumBaseUrl}/session");
                    var request = new HttpRequestMessage(HttpMethod.Post, uri);
                    request.Headers.Add("X-CSRF-Token", await GetCsrfToken());
                    request.Headers.Add("x-requested-with", "XMLHttpRequest");
                    request.Headers.Add("cookie", _cookie);
                    request.Method = HttpMethod.Post;
                    request.Content = new FormUrlEncodedContent(parameters);

                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        //we got the real cookie, parse
                        var tCookie = response.Headers.FirstOrDefault(w => w.Key == "Set-Cookie").Value.FirstOrDefault(w => w.Contains("_t"));
                        var forumSessionCookie = response.Headers.FirstOrDefault(w => w.Key == "Set-Cookie").Value.FirstOrDefault(w => w.Contains("_forum"));
                        var trimForumCookie = forumSessionCookie.Substring(0, forumSessionCookie.IndexOf("path=/") - 2);

                        //and set it
                        _cookie = tCookie.Substring(0, tCookie.IndexOf(';') + 1) + " " +trimForumCookie;
                    }
                }
            }
            catch (Exception w)
            {
                Console.WriteLine(w);
                Thread.Sleep(2000);
            }
        }
        
        //get the csrf token, also generates a new one if current one is expired
        public static async Task<string> GetCsrfToken()
        {
            if (DateTime.Now >= _tokenValidTo)
            {
                //get the token (valid for ~15 mins)
                try
                {
                    var csrfSource = new Uri($"{Constants.ForumBaseUrl}/session/csrf.json");
                    using (HttpClient client = new HttpClient())
                    {
                        var csrfResult = await client.GetAsync(csrfSource);
                        var csrfToken = (string) await Deserialize(csrfResult, _csrfPath);

                        //and get a new cookie if not set
                        if (string.IsNullOrWhiteSpace(_cookie))
                        {
                            _cookie = csrfResult.Headers.FirstOrDefault(w => w.Key == "Set-Cookie").Value.FirstOrDefault();
                        }

                        _tokenValidTo = DateTime.Now.AddMinutes(5);
                        _token = csrfToken;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Thread.Sleep(2000);
                }
            }

            return _token;
        }

        //get the csrf token from the response by github magic.
        //thanks to: https://github.com/bgorven/Discouser for the magic
        private static async Task<JToken> Deserialize(HttpResponseMessage message, string[] path)
        {
            using (var inputStream = await message.Content.ReadAsStreamAsync())
            using (var streamReader = new StreamReader(inputStream))
            using (var reader = new JsonTextReader(streamReader))
            {
                try
                {
                    if (!reader.Read()) throw new ArgumentException("Response missing any JSON data", "result");
                    for (var index = 0; index < path.Length; index++)
                    {
                        //when this loop ends, the start of the desired object will be the current token in the reader.
                        while (true)
                        {
                            if (!reader.Read()) throw new InvalidDataException("Malformed JSON data.");
                            if (reader.TokenType != JsonToken.PropertyName) throw new InvalidDataException(
                                "Property “" + string.Join(".", path, 0, index + 1) + "” not found.");

                            if (path[index].Equals(reader.Value))
                            {
                                reader.Read();
                                break;
                            }

                            reader.Skip();
                        }
                    }

                    return JToken.ReadFrom(reader);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Thread.Sleep(2000);
                }
            }

            //something went wrong
            return null;
        }
    }
}