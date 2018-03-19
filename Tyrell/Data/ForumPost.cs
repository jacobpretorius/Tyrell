using System;

namespace Tyrell.Data
{
    public class ForumPost
    {
        public int Id { get; set; }
        public int TopicId { get; set; }

        public string PostRaw { get; set; }

        public int AuthorID { get; set; }
        public string AuthorUserName { get; set; }

        public int Version { get; set; }
        public int PostNumber { get; set; }
        public int ReplyCount { get; set; }
        public int Reads { get; set; }
        public int TrustLevel { get; set; }

        public double? Sentiment { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        //forUs
        public DateTime LastCrawl { get; set; }
    }
}
