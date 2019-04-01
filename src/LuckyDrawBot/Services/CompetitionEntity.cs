using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using LuckyDrawBot.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace LuckyDrawBot.Services
{
    public partial class CompetitionRepositoryService
    {
        [Table("OpenCompetitions")]
        public class OpenCompetitionEntity : CompetitionEntity
        {
            public OpenCompetitionEntity()
            {
            }

            public OpenCompetitionEntity(Guid id) : base(id)
            {
            }
        }

        [Table("CompletedCompetitions")]
        public class CompletedCompetitionEntity : CompetitionEntity
        {
            public CompletedCompetitionEntity()
            {
            }

            public CompletedCompetitionEntity(Guid id) : base(id)
            {
            }
        }

        public class CompetitionEntity : TableEntity
        {
            public Guid Id { get; set; }
            public string ServiceUrl { get; set; }
            public Guid TenantId { get; set; }
            public string TeamId { get; set; }
            public string ChannelId { get; set; }
            public string MainActivityId { get; set; }
            public string ResultActivityId { get; set; }
            public DateTimeOffset CreatedTime { get; set; }
            public DateTimeOffset PlannedDrawTime { get; set; }
            public DateTimeOffset? ActualDrawTime { get; set; }
            public string Locale { get; set; }
            public string Gift { get; set; }
            public string GiftImageUrl { get; set; }
            public string Description { get; set; }
            public int WinnerCount { get; set; }
            public bool IsCompleted { get; set; }
            public string CreatorName { get; set; }
            public string CreatorAadObject { get; set; }
            public List<string> WinnerAadObjectIds { get; set; }
            public List<Competitor> Competitors { get; set; }

            public CompetitionEntity()
            {
            }

            public CompetitionEntity(Guid id)
            {
                var idString = id.ToString();
                PartitionKey = idString;
                RowKey = idString;
                Id = id;
            }

            public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
            {
                base.ReadEntity(properties, operationContext);
                EntityProperty property;
                if (properties.TryGetValue(nameof(CompetitionEntity.WinnerAadObjectIds), out property))
                {
                    WinnerAadObjectIds = JsonConvert.DeserializeObject<List<string>>(property.StringValue);
                }
                if (properties.TryGetValue(nameof(CompetitionEntity.Competitors), out property))
                {
                    Competitors = JsonConvert.DeserializeObject<List<Competitor>>(property.StringValue);
                }
            }

            public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
            {
                var properties = base.WriteEntity(operationContext);
                properties[nameof(CompetitionEntity.WinnerAadObjectIds)] = EntityProperty.GeneratePropertyForString(JsonConvert.SerializeObject(WinnerAadObjectIds));
                properties[nameof(CompetitionEntity.Competitors)] = EntityProperty.GeneratePropertyForString(JsonConvert.SerializeObject(Competitors));
                return properties;
            }
        }
    }
}
