using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LuckyDrawBot.Infrastructure.Database;
using LuckyDrawBot.Models;

namespace LuckyDrawBot.Services
{
    public interface ICompetitionRepositoryService
    {
        Task<Competition> GetOpenCompetition(Guid competitionId);
        Task<List<Guid>> GetOpenCompetitionIds(DateTimeOffset maxPlannedDrawTime);
        Task UpsertOpenCompetition(Competition competition);
        Task DeleteOpenCompetition(Guid competitionId);
        Task<Competition> GetCompletedCompetition(Guid competitionId);
        Task UpsertCompletedCompetition(Competition competition);
    }

    public partial class CompetitionRepositoryService : ICompetitionRepositoryService
    {
        public IDataTable<LuckyDrawDataTablesSettings, OpenCompetitionEntity> OpenCompetitions { get; }
        public IDataTable<LuckyDrawDataTablesSettings, CompletedCompetitionEntity> CompletedCompetitions { get; }

        public CompetitionRepositoryService(
            IDataTable<LuckyDrawDataTablesSettings, OpenCompetitionEntity> openCompetitions,
            IDataTable<LuckyDrawDataTablesSettings, CompletedCompetitionEntity> completedCompetitions)
        {
            OpenCompetitions = openCompetitions;
            CompletedCompetitions = completedCompetitions;
        }

        public async Task<Competition> GetOpenCompetition(Guid competitionId)
        {
            var entity = await OpenCompetitions.Retrieve(new OpenCompetitionEntity(competitionId));
            if (entity == null)
            {
                return null;
            }
            return new Competition
            {
                Id = entity.Id,
                ServiceUrl = entity.ServiceUrl,
                TenantId = entity.TenantId,
                TeamId = entity.TeamId,
                ChannelId = entity.ChannelId,
                MainActivityId = entity.MainActivityId,
                ResultActivityId = entity.ResultActivityId,
                CreatedTime = entity.CreatedTime,
                PlannedDrawTime = entity.PlannedDrawTime,
                ActualDrawTime = entity.ActualDrawTime,
                Locale = entity.Locale,
                Gift = entity.Gift,
                GiftImageUrl = entity.GiftImageUrl,
                Description = entity.Description,
                WinnerCount = entity.WinnerCount,
                IsCompleted = entity.IsCompleted,
                CreatorName = entity.CreatorName,
                CreatorAadObject = entity.CreatorAadObject,
                WinnerAadObjectIds = entity.WinnerAadObjectIds,
                Competitors = entity.Competitors
            };
        }

        public async Task<List<Guid>> GetOpenCompetitionIds(DateTimeOffset maxPlannedDrawTime)
        {
            var openCompetitions = await OpenCompetitions.Query(
                $"PlannedDrawTime lt datetime'{maxPlannedDrawTime.ToString("yyyy-MM-ddTHH:mm:ss")}Z'",
                selectColumns: new List<string> { "Id" });
            return openCompetitions.Select(oc => oc.Id).ToList();
        }

        public async Task UpsertOpenCompetition(Competition competition)
        {
            var entity = new OpenCompetitionEntity(competition.Id)
            {
                ServiceUrl = competition.ServiceUrl,
                TenantId = competition.TenantId,
                TeamId = competition.TeamId,
                ChannelId = competition.ChannelId,
                MainActivityId = competition.MainActivityId,
                ResultActivityId = competition.ResultActivityId,
                CreatedTime = competition.CreatedTime,
                PlannedDrawTime = competition.PlannedDrawTime,
                ActualDrawTime = competition.ActualDrawTime,
                Locale = competition.Locale,
                Gift = competition.Gift,
                GiftImageUrl = competition.GiftImageUrl,
                Description = competition.Description,
                WinnerCount = competition.WinnerCount,
                IsCompleted = competition.IsCompleted,
                CreatorName = competition.CreatorName,
                CreatorAadObject = competition.CreatorAadObject,
                WinnerAadObjectIds = competition.WinnerAadObjectIds,
                Competitors = competition.Competitors
            };
            await OpenCompetitions.InsertOrReplace(entity);
        }

        public async Task DeleteOpenCompetition(Guid competitionId)
        {
            await OpenCompetitions.Delete(new OpenCompetitionEntity(competitionId));
        }

        public async Task<Competition> GetCompletedCompetition(Guid competitionId)
        {
            var entity = await CompletedCompetitions.Retrieve(new CompletedCompetitionEntity(competitionId));
            if (entity == null)
            {
                return null;
            }
            return new Competition
            {
                Id = entity.Id,
                ServiceUrl = entity.ServiceUrl,
                TenantId = entity.TenantId,
                TeamId = entity.TeamId,
                ChannelId = entity.ChannelId,
                MainActivityId = entity.MainActivityId,
                ResultActivityId = entity.ResultActivityId,
                CreatedTime = entity.CreatedTime,
                PlannedDrawTime = entity.PlannedDrawTime,
                ActualDrawTime = entity.ActualDrawTime,
                Locale = entity.Locale,
                Gift = entity.Gift,
                GiftImageUrl = entity.GiftImageUrl,
                Description = entity.Description,
                WinnerCount = entity.WinnerCount,
                IsCompleted = entity.IsCompleted,
                CreatorName = entity.CreatorName,
                CreatorAadObject = entity.CreatorAadObject,
                WinnerAadObjectIds = entity.WinnerAadObjectIds,
                Competitors = entity.Competitors
            };
        }

        public async Task UpsertCompletedCompetition(Competition competition)
        {
            var entity = new CompletedCompetitionEntity(competition.Id)
            {
                ServiceUrl = competition.ServiceUrl,
                TenantId = competition.TenantId,
                TeamId = competition.TeamId,
                ChannelId = competition.ChannelId,
                MainActivityId = competition.MainActivityId,
                ResultActivityId = competition.ResultActivityId,
                CreatedTime = competition.CreatedTime,
                PlannedDrawTime = competition.PlannedDrawTime,
                ActualDrawTime = competition.ActualDrawTime,
                Locale = competition.Locale,
                Gift = competition.Gift,
                GiftImageUrl = competition.GiftImageUrl,
                Description = competition.Description,
                WinnerCount = competition.WinnerCount,
                IsCompleted = competition.IsCompleted,
                CreatorName = competition.CreatorName,
                CreatorAadObject = competition.CreatorAadObject,
                WinnerAadObjectIds = competition.WinnerAadObjectIds,
                Competitors = competition.Competitors
            };
            await CompletedCompetitions.InsertOrReplace(entity);
        }
    }
}