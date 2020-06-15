using Microsoft.Bot.Schema;

namespace LuckyDrawBot.Models
{
    public class TaskModuleTaskInfo2
    {
        public class TaskInfoValue2
        {
            public string Title { get; set; }
            public object Height { get; set; }
            public object Width { get; set; }
            public string Url { get; set; }
            public Attachment Card { get; set; }
            public string FallbackUrl { get; set; }
            public string CompletionBotId { get; set; }
        }

        public string Type { get; set; }
        public TaskInfoValue2 Value { get; set; }
    }

    public class TaskModuleTaskInfoResponse2
    {
        public TaskModuleTaskInfo2 Task { get; set; }
    }
}