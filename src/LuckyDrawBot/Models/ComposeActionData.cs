using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LuckyDrawBot.Models
{
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public enum ComposeActionType
    {
        Unknown = 0,
        Preview = 10
    }

    public class ComposeActionData
    {
        public ComposeActionType UserAction { get; set; }
    }
}