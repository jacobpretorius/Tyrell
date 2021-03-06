﻿using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Tyrell.Business;
using Tyrell.DisplayConsole;
using Tyrell.Server;

namespace Tyrell
{
    internal class Program
    {
        private static void Main()
        {
            while (true)
            {
                try
                {
                    //start the webhost for uptime
                    Task.Run(StartWebHost);

                    //RUNS IN UNMANNED MODE, NO OPTIONS IT JUST GOES
                    UnmannedRunner().Wait();

                    //RUNS IN SEMI MANNED MODE, GIVES YOU SOME OPTIONS AND BREAKS TO THE MENU
                    //Runner().Wait();
                }
                catch (Exception e)
                {
                    Display.WriteErrorBottomLine(e.ToString());
                }
            }
        }

        private static async Task StartWebHost()
        {
            var webhost = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, 1982);
                })
                .UseStartup<ServerMain>()
                .Build();
            await webhost.RunAsync();
        }

        private static async Task UnmannedRunner()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Clear();

            Display.FlickerPrint("STARTING UNMANNED MODE");
            
            try
            {
                await Login();

                await Functions.AutomaticModeUnmanned();
            }
            catch
            {
                Thread.Sleep(10000);
            }
        }

        private static async Task Runner()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Clear();

            Display.FlickerPrint("STARTING");

            await Login();

            ConsoleKeyInfo keyDown;
            do
            {
                Display.MainMenu();

                keyDown = Console.ReadKey(false);

                switch (keyDown.KeyChar.ToString())
                {
                    case "1":
                        await Login();
                        break;

                    case "2":
                        Display.FlickerPrint("BACK PROCESSING SENTIMENT");
                        await Functions.ProcessAllSentiment();
                        break;

                    case "3":
                        Display.PromptForLatestCount();
                        var choice = Console.ReadLine();
                        await Crawler.ReadLatestForumPostsSmart(Convert.ToInt32(choice));
                        break;

                    case "4":
                        Display.PromptForStartRange();
                        var startRange = Console.ReadLine();

                        Display.PromptForEndRange();
                        var endRange = Console.ReadLine();
                        await Crawler.ReadPostsBetweenRanges(Convert.ToInt32(startRange), Convert.ToInt32(endRange));
                        break;

                    case "5":
                        Display.FlickerPrint("READING ALL POSTS");
                        await Crawler.ReadAllForumPosts();
                        break;

                    case "6":
                        Display.FlickerPrint("READING ALL THREADS");
                        await Crawler.ReadAllThreads();
                        break;

                    case "7":
                        await Functions.CheckForRemindMePosts(7);
                        break;

                    case "8":
                        Display.PromptForThreadId();
                        var threadID = Console.ReadLine();

                        Display.PromptForMessage();
                        var message = Console.ReadLine();
                        await Functions.PostToThread(Convert.ToInt32(threadID), message);
                        break;

                    case "9":
                        await Functions.AutomaticMode();
                        break;
                }
            }
            while (keyDown.Key != ConsoleKey.Escape);

            Display.FlickerPrint("BYE");
        }

        public static async Task Login()
        {
            Display.FlickerPrint("LOGGING IN");
            await Session.Login();
        }
    }
}