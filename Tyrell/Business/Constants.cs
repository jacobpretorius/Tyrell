namespace Tyrell.Business
{
    public static class Constants
    {
        public static readonly string ForumBaseUrl = "https://someforum.com";

        public static readonly string ElasticUrl = "http://localhost:9200";

        //the way I understand the use of indexes in ES > 6 and going forward
        //they want you to use an index for each document "type" instead of how people
        //were (mis)using it before. So i've done so.
        public static readonly string ElasticPostIndex = "someforumposts";

        public static readonly string ElasticThreadIndex = "someforumthreads";

        public static readonly string ElasticRemindersIndex = "someforumreminders";

        //all sites start somewhere, looks like our one started around here
        public static readonly int IndexPostsBottomRange = 0;

        public static readonly int IndexThreadsBottomRange = 0;

        //increase this number to be easier on the site hosts
        public static readonly int FullReindexSleepMilliseconds = 500;

        //bot settings
        public static readonly string BotName = "tyrell";

        public static readonly string BotIgnoreFlag = "tyrellignore";

        //ya ya I know dont store this here, pull req with a fix
        public static readonly string Username = "Tyrell";

        public static readonly string Password = "password";

        //cleverbot
        public static readonly string CleverBotApiKey = "";
    }
}
