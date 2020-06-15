using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;

namespace LuckyDrawBot.Tests.Models
{
    // Microsoft.Bot.Schema.Teams.TaskModuleResponse declare 'Task' property as 'TaskModuleResponseBase'.
    // It is hard/impossible to be deserialized to subclass 'TaskModuleContinueResponse'.
    // Use this model for easy testing
    public class TaskModuleResponseForContinue
    {
        [JsonProperty(PropertyName = "task")]
        public TaskModuleContinueResponse Task { get; set; }
    }
}