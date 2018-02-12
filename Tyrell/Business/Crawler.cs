using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nest;
using Tyrell.Data;
using Tyrell.DisplayConsole;

namespace Tyrell.Business
{
    public static class Crawler
    {
        private static readonly ElasticClient ElasticSearch;
        private static readonly ElasticClient ElasticSearchThreads;
        private static int HighestActualTopicId { get; set; }
        private static int HighestActualPostId { get; set; }
        private static int HighestKnownPostId { get; set; }
        private static DateTime LastExtraRangeScan { get; set; }

        static Crawler()
        {
            LastExtraRangeScan = DateTime.Now.ToLocalTime().AddDays(-1);

            var settings = new ConnectionSettings(new Uri(Constants.ElasticUrl))
                .DefaultIndex(Constants.ElasticPostIndex);

            ElasticSearch = new ElasticClient(settings);

            settings = new ConnectionSettings(new Uri(Constants.ElasticUrl))
                .DefaultIndex(Constants.ElasticThreadIndex);

            ElasticSearchThreads = new ElasticClient(settings);
        }
        
        //lets literally read every post on the site we know of
        public static async Task ReadAllForumPosts()
        {
            //get the highest indexed post ID
            var esResponse = ElasticSearch.Search<ForumPost>(
                s => s
                    .Aggregations(a => a
                        .Max("max_id", m => m
                            .Field(p => p.Id)
                        )
                    )
                );

            var highestId = esResponse?.Aggs?.Max("max_id")?.Value;
            if (highestId != null)
            {
                //and read from there back to start of user acc access
                var i = (int) highestId;
                var lowerLimit = Constants.IndexPostsBottomRange;
                while (i-- >= lowerLimit)
                {
                    try
                    {
                        await ReadForumPost(i);
                    }
                    catch (Exception w)
                    {
                        Console.WriteLine(w);
                    }

                    //give things some time to sanity check
                    Thread.Sleep(Constants.FullReindexSleepMilliseconds);
                }
            }
        }

        //reads all the threads on the site we know of
        public static async Task ReadAllThreads()
        {
            //get the highest indexed post ID
            var esResponse = ElasticSearchThreads.Search<ForumThread>(
                s => s
                    .Aggregations(a => a
                        .Max("max_id", m => m
                            .Field(p => p.Id)
                        )
                    )
            );

            var highestId = esResponse?.Aggs?.Max("max_id")?.Value;
            if (highestId != null)
            {
                var i = (int) highestId;
                while (i-- >= Constants.IndexThreadsBottomRange)
                {
                    try
                    {
                        await ReadThread(i);
                    }
                    catch (Exception w)
                    {
                        Console.WriteLine(w);
                    }

                    //give things some time to sanity check
                    Thread.Sleep(Constants.FullReindexSleepMilliseconds);
                }
            }
        }

        //Read all posts between given ranges
        public static async Task ReadPostsBetweenRanges(int startRange, int endRange)
        {
            while (startRange-- >= endRange)
            {
                try
                {
                    await ReadForumPost(startRange);
                }
                catch (Exception w)
                {
                    Console.WriteLine(w);
                }

                //give things some time to sanity check
                Thread.Sleep(Constants.FullReindexSleepMilliseconds);
            }
        }

        //read the forum post into ES. Called on a loop
        private static async Task ReadForumPost(int postId)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new System.Uri(Constants.ForumBaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("cookie", Session.Cookie);

                HttpResponseMessage response = await client.GetAsync($"{Constants.ForumBaseUrl}/posts/{postId}");
                if (response.IsSuccessStatusCode)
                {
                    if (response?.RequestMessage?.RequestUri?.AbsolutePath == "/login")
                    {
                        //this one req died, we will get it on the next go
                        await Session.Login();
                        return;
                    }

                    dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());

                    if (json != null)
                    {
                        var forumPost = new ForumPost
                        {
                            LastCrawl = DateTime.Now,
                            Id = (int)json.id,
                            TopicId = (int)json?.topic_id,
                            PostRaw = (string)json?.raw,
                            AuthorID = (int)json?.user_id,
                            AuthorUserName = (string)json?.username,
                            Version = (int)json?.version,
                            PostNumber = (int)json?.post_number,
                            ReplyCount = (int)json?.reply_count,
                            Reads = (int)json?.reads,
                            TrustLevel = (int)json?.trust_level,
                            UpdatedAt = json.updated_at,
                            CreatedAt = json.created_at
                        };

                        var _index = ElasticSearch.Index(forumPost);

                        Display.ReadPostUpdate(ref forumPost);
                    }
                }
                else
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Display.WriteOnBottomLine($"COULD NOT READ POST [{postId}]");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                }
            }
        }

        //read the thread into ES, called on loops
        private static async Task ReadThread(int threadId)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new System.Uri(Constants.ForumBaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("cookie", Session.Cookie);

                HttpResponseMessage response = await client.GetAsync($"{Constants.ForumBaseUrl}/t/{threadId}");
                if (response.IsSuccessStatusCode)
                {
                    if (response?.RequestMessage?.RequestUri?.AbsolutePath == "/login")
                    {
                        await Session.Login();
                        return;
                    }

                    dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
                    if (json != null)
                    {
                        var forumThread = new ForumThread
                        {
                            LastCrawl = DateTime.Now,
                            Id = (int)json.id,
                            Title = (string)json?.title,
                            CategoryId = (int)json?.category_id,
                            HighestPostNumber = (int)json?.highest_post_number,
                            PostsCount = (int)json?.posts_count,
                            ReplyCount = (int)json?.reply_count,
                            Views = (int)json?.views,
                            Archived = (bool)json?.archived,
                            Bumped = false,
                            BumpedAt = json.created_at,
                            OriginalPosterID = (int)json?.details?.created_by.id,
                            CreatedAt = json.created_at
                        };

                        var _index = ElasticSearchThreads.Index(forumThread);

                        Display.ReadThreadUpdate(ref forumThread);
                    }
                }
                else
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Display.WriteOnBottomLine($"COULD NOT READ THREAD [{threadId}]");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                }
            }
        }
        
        //new smart indexer with optional extra rescan range
        public static async Task ReadLatestForumPostsSmart(int rescanRange = 5)
        {
            // if it's 1AM, do a 1k posts rescan for completeness
            if (DateTime.Now.Hour == 1 && LastExtraRangeScan.Date < DateTime.Today)
            {
                rescanRange = 1000;
                LastExtraRangeScan = DateTime.Now.ToLocalTime();
            }

            //get the latest threads, index them, and set highest ACTUAL thread ID
            await GetLatestForumPostsAndSetNewestThreadId();

            await GetLatestThreadAndSetLatestActualPostId();

            //get the highest indexed forum post ID
            SetHighestKnownPostId();

            //add our default rescan range and let it run
            if (HighestActualPostId > HighestKnownPostId)
            {
                var lowerLimit = HighestKnownPostId - rescanRange;
                var startRange = HighestActualPostId + 1;
                while (startRange-- >= lowerLimit)
                {
                    try
                    {
                        await ReadForumPost(startRange);
                    }
                    catch (Exception w)
                    {
                        Console.WriteLine(w);
                    }

                    //give things some time to sanity check
                    Thread.Sleep(100);
                }
            }
        }

        //used to get the latest forum posts
        private static async Task GetLatestForumPostsAndSetNewestThreadId()
        {
            try
            {
                //read latest forums to get their ID's mostly
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new System.Uri($"{Constants.ForumBaseUrl}/latest.json?order=default");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("cookie", Session.Cookie);

                    HttpResponseMessage response = await client.GetAsync($"{Constants.ForumBaseUrl}/latest.json?order=default");
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(data);

                        if (json.topic_list != null && json.topic_list.topics != null)
                        {
                            foreach (var jsonTopic in json.topic_list.topics)
                            {
                                var forumThread = new ForumThread
                                {
                                    LastCrawl = DateTime.Now,
                                    Id = (int)jsonTopic.id,
                                    Title = (string)jsonTopic?.title,
                                    CategoryId = (int)jsonTopic?.category_id,
                                    HighestPostNumber = (int)jsonTopic?.highest_post_number,
                                    PostsCount = (int)jsonTopic?.posts_count,
                                    ReplyCount = (int)jsonTopic?.reply_count,
                                    Views = (int)jsonTopic?.views,
                                    Archived = (bool)jsonTopic?.archived,
                                    Bumped = (bool)jsonTopic?.bumped,
                                    BumpedAt = jsonTopic.bumped_at,
                                    OriginalPosterID = (int)jsonTopic?.posters[0]?.user_id,
                                    CreatedAt = jsonTopic.created_at
                                };

                                //index them while we have them
                                var _index = ElasticSearchThreads.Index(forumThread);

                                Display.ReadThreadUpdate(ref forumThread);
                            }

                            HighestActualTopicId = (int)json.topic_list.topics[0].id;
                        }
                    }
                }
            }
            catch (Exception w)
            {
                Console.WriteLine(w);
            }
        }

        //get the latest/highest actual post ID from the latest active thread
        private static async Task GetLatestThreadAndSetLatestActualPostId()
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new System.Uri(Constants.ForumBaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("cookie", Session.Cookie);

                HttpResponseMessage response = await client.GetAsync($"{Constants.ForumBaseUrl}/t/{HighestActualTopicId}");
                if (response.IsSuccessStatusCode)
                {
                    if (response?.RequestMessage?.RequestUri?.AbsolutePath == "/login")
                    {
                        await Session.Login();
                        return;
                    }
                    
                    dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
                    if (json != null)
                    {
                        if (json.post_stream != null && json.post_stream.stream != null)
                        {
                            var last = 0;
                            foreach (var post in json.post_stream.stream)
                            {
                                if (post > last)
                                {
                                    last = post;
                                }
                            }

                            HighestActualPostId = last;
                        }
                    }
                }
                else
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Display.WriteOnBottomLine($"COULD NOT READ LATEST THREAD [{HighestActualTopicId}]");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                }
            }
        }

        //used to set the highest known (indexed) forum post variable
        private static void SetHighestKnownPostId()
        {
            var esResponse = ElasticSearch.Search<ForumPost>(
                s => s
                    .Aggregations(a => a
                        .Max("max_id", m => m
                            .Field(p => p.Id)
                        )
                    )
            );

            var value = esResponse?.Aggs?.Max("max_id")?.Value;
            if (value != null)
            {
                HighestKnownPostId = (int) value;
            }
        }
    }
}