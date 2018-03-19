using System;

namespace Tyrell.Data
{
    public class Reminder
    {
        public string Id { get; set; }
        public int TopicId { get; set; }
        public int AuthorID { get; set; }
        public int PostId { get; set; }
        public string AuthorUserName { get; set; }
        public string ReminderMessage { get; set; }
        public DateTime ReminderRequestedOn { get; set; }
        public DateTime RemindUserOn { get; set; }

        //forUs
        public DateTime AddedToTyrell { get; set; }
    }
}
