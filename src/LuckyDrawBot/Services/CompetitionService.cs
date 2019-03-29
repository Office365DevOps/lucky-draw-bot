using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LuckyDrawBot.Models;

namespace LuckyDrawBot.Services
{
    public interface ICompetitionService
    {
        Task<Competition> Create(Guid tenantId, string teamId, string channelId,
                                 DateTimeOffset drawTime, string locale, string gift, string description,
                                 string creatorName, Guid creatorAadObject);
        Task<Competition> UpdateMainActivity(Guid competitionId, string mainActivityId);
        Task<Competition> AddCompetitor(Guid competitionId, Guid competitorAadObjectId, string competitorName);
        Task<List<Guid>> GetToBeDrawnCompetitionIds();
        Task<Competition> Draw(Guid competitionId);
        Task<Competition> UpdateResultActivity(Guid competitionId, string resultActivityId);
    }

    public class CompetitionService : ICompetitionService
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IRandomService _randomService;
        private readonly ICompetitionRepositoryService _repositoryService;

        public CompetitionService(IDateTimeService dateTimeService, IRandomService randomService, ICompetitionRepositoryService repositoryService)
        {
            _dateTimeService = dateTimeService;
            _randomService = randomService;
            _repositoryService = repositoryService;
        }

        public async Task<Competition> AddCompetitor(Guid competitionId, Guid competitorAadObjectId, string competitorName)
        {
            var competition = await _repositoryService.GetOpenCompetition(competitionId);
            var competitor = new Competitor
            {
                AadObjectId = competitorAadObjectId,
                Name = competitorName,
                JoinTime = _dateTimeService.UtcNow
            };
            competition.Competitors.Add(competitor);
            await _repositoryService.UpsertOpenCompetition(competition);
            return competition;
        }

        public async Task<Competition> Create(Guid tenantId, string teamId, string channelId, DateTimeOffset plannedDrawTime, string locale, string gift, string description, string creatorName, Guid creatorAadObject)
        {
            var competition = new Competition
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TeamId = teamId,
                ChannelId = channelId,
                MainActivityId = string.Empty,
                ResultActivityId = string.Empty,
                CreatedTime = _dateTimeService.UtcNow,
                PlannedDrawTime = plannedDrawTime,
                ActualDrawTime = DateTimeOffset.MinValue,
                Locale = locale,
                Gift = gift,
                Description = description,
                IsCompleted = false,
                CreatorName = creatorName,
                CreatorAadObject = creatorAadObject,
                WinnerAadObjectId = Guid.Empty,
                Competitors = new List<Competitor>()
            };
            await _repositoryService.UpsertOpenCompetition(competition);
            return competition;
        }

        public async Task<Competition> Draw(Guid competitionId)
        {
            var competition = await _repositoryService.GetOpenCompetition(competitionId);
            if (competition.Competitors.Count > 0)
            {
                var winner = competition.Competitors[_randomService.Next(competition.Competitors.Count)];
                competition.WinnerAadObjectId = winner.AadObjectId;
            }
            competition.ActualDrawTime = _dateTimeService.UtcNow;
            competition.IsCompleted = true;
            await _repositoryService.UpsertCompletedCompetition(competition);
            await _repositoryService.DeleteOpenCompetition(competition.Id);
            return competition;
        }

        public async Task<List<Guid>> GetToBeDrawnCompetitionIds()
        {
            return await _repositoryService.GetOpenCompetitionIds(_dateTimeService.UtcNow);
        }

        public async Task<Competition> UpdateMainActivity(Guid competitionId, string mainActivityId)
        {
            var competition = await _repositoryService.GetOpenCompetition(competitionId);
            competition.MainActivityId = mainActivityId;
            await _repositoryService.UpsertOpenCompetition(competition);
            return competition;
        }

        public async Task<Competition> UpdateResultActivity(Guid competitionId, string resultActivityId)
        {
            var competition = await _repositoryService.GetCompletedCompetition(competitionId);
            competition.ResultActivityId = resultActivityId;
            await _repositoryService.UpsertCompletedCompetition(competition);
            return competition;
        }

    }
}