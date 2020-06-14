using Microsoft.Bot.Schema;

namespace LuckyDrawBot.Models
{
    public class TaskModuleTaskInfo
    {
        public class TaskInfoValue
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
        public TaskInfoValue Value { get; set; }
    }

    public class TaskModuleTaskInfoResponse
    {
        public TaskModuleTaskInfo Task { get; set; }
    }
}