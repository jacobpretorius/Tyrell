using System;
using System.Threading.Tasks;
using Tyrell.Business;
using Tyrell.DisplayConsole;

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
                    Runner().Wait();
                }
                catch (Exception e)
                {
                    Display.WriteErrorBottomLine(e.ToString());
                }
            }
        }

        private static async Task Runner()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Clear();

            Display.FlickerPrint("STARTING");

            await Login();

            await Functions.AutomaticMode();

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
                        Display.FlickerPrint("READING LATEST FORUM POSTS");
                        await Crawler.ReadLatestForumPostsSmart();
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