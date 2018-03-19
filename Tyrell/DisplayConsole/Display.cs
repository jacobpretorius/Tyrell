using System;
using System.Threading;
using Tyrell.Data;

namespace Tyrell.DisplayConsole
{
    public static class Display
    {
        public static void MainMenu()
        {
            Console.Clear();
            WriteOnBottomLine("##### TYRELL #####");
            WriteOnBottomLine("");
            WriteOnBottomLine("[KEY]\tOPTION");
            WriteOnBottomLine("");
            WriteOnBottomLine("[1]\tRELOG");
            WriteOnBottomLine("[2]\tPROCESS ALL SENTIMENT");
            WriteOnBottomLine("[3]\tCRAWL LATEST THREADS AND X POSTS");
            WriteOnBottomLine("[4]\tCRAWL FROM X TO X POSTS");
            WriteOnBottomLine("[5]\tCRAWL ALL POSTS");
            WriteOnBottomLine("[6]\tCRAWL ALL THREADS");
            WriteOnBottomLine("[7]\tBACK PROCESS REMINDME");
            WriteOnBottomLine("[8]\tPOST TO THREAD");
            WriteOnBottomLine("[9]\tAUTOMATIC MODE");
            WriteOnBottomLine("");
            WriteOnBottomLine("[ESC]\tEXIT");
            WriteOnBottomLine("");
        }
        
        public static void ReadThreadUpdate(ref ForumThread forumThread)
        {
            Console.Clear();
            WriteOnBottomLine($"READ [{forumThread.Id}] BY [{forumThread.OriginalPosterID}] HAS {forumThread.PostsCount} POSTS");
        }
        

        public static void ReadPostUpdate(ref ForumPost forumPost)
        {
            Console.Clear();
            WriteOnBottomLine($"READ [{forumPost.Id}] BY [{forumPost.AuthorUserName}] SAYING: {forumPost.PostRaw}");
        }
        
        public static void PromptForLatestCount()
        {
            Console.Clear();
            WriteOnBottomLine("HOW MANY DO YOU WANT TO INDEX?");
            WriteOnBottomLine("");
        }

        public static void PromptForStartRange()
        {
            Console.Clear();
            WriteOnBottomLine("START FROM WHERE?");
            WriteOnBottomLine("");
        }

        public static void PromptForEndRange()
        {
            Console.Clear();
            WriteOnBottomLine("STOP WHERE? [LOWEST POSSIBLE 5500]");
            WriteOnBottomLine("");
        }

        public static void PromptForThreadId()
        {
            Console.Clear();
            WriteOnBottomLine("THREAD ID?");
            WriteOnBottomLine("");
        }

        public static void PromptForMessage()
        {
            Console.Clear();
            WriteOnBottomLine("SAY?");
            WriteOnBottomLine("");
        }

        //really dumb for spending time on this, but, ayy
        public static void FlickerPrint(string text)
        {
            for (int i = 0; i < 4; i++)
            {
                var outtext = "";
                switch (i)
                {
                    case 0:
                        outtext = text + ".";
                        break;
                    case 1:
                        outtext = text + "..";
                        break;
                    case 2:
                        outtext = text + "...";
                        break;
                    case 3:
                        outtext = text + ".../";
                        break;
                }

                Console.Clear();
                WriteOnBottomLine("##### TYRELL V1.1 #####");
                WriteOnBottomLine("");
                WriteOnBottomLine(outtext);
                Thread.Sleep(300);
            }
        }

        public static void WriteOnBottomLine(string text)
        {
            Console.CursorTop = Console.WindowTop + Console.WindowHeight - 1;
            Console.WriteLine(text);
        }

        public static void WriteErrorBottomLine(string text)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.CursorTop = Console.WindowTop + Console.WindowHeight - 1;
            Console.WriteLine($"[ERROR] {DateTime.Now.ToString("T")}: {text}");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
        }
    }
}
