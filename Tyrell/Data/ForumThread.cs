using System;

namespace Tyrell.Data
{
    public class ForumThread
    {
        public string Title { get; set; }

        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int HighestPostNumber { get; set; }
        public int PostsCount { get; set; }
        public int ReplyCount { get; set; }
        public int Views { get; set; }

        public bool Archived { get; set; }
        public bool Bumped { get; set; }
        public DateTime BumpedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public int OriginalPosterID { get; set; }

        //forUs
        public DateTime LastCrawl { get; set; }
    }
}
