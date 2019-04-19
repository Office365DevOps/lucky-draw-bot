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
        Task<Competition> CreateDraftCompetition(string serviceUrl, Guid tenantId, string teamId, string channelId,
                                                 DateTimeOffset drawTime, string locale, double offsetHours,
                                                 string creatorName, string creatorAadObject);
        Task<Competition> CreateActiveCompetition(string serviceUrl, Guid tenantId, string teamId, string channelId,
                                                  DateTimeOffset drawTime, string locale, double offsetHours,
                                                  string gift, string giftImageUrl, int winnerCount,
                                                  string creatorName, string creatorAadObject);
        Task<Competition> GetCompetition(Guid competitionId);
        Task<Competition> UpdateMainActivity(Guid competitionId, string mainActivityId);
        Task<Competition> AddCompetitor(Guid competitionId, string competitorAadObjectId, string competitorName);
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

        public async Task<Competition> AddCompetitor(Guid competitionId, string competitorAadObjectId, string competitorName)
        {
            var competition = await _repositoryService.GetOpenCompetition(competitionId);
            var competitor = competition.Competitors.FirstOrDefault(c => c.AadObjectId == competitorAadObjectId);
            if (competitor == null)
            {
                competitor = new Competitor
                {
                    AadObjectId = competitorAadObjectId,
                    Name = competitorName,
                    JoinTime = _dateTimeService.UtcNow
                };
                competition.Competitors.Add(competitor);
            }
            else
            {
                competitor.JoinTime = _dateTimeService.UtcNow;
            }

            await _repositoryService.UpsertOpenCompetition(competition);
            return competition;
        }

        private Competition CreateCompetition(string serviceUrl, Guid tenantId, string teamId, string channelId, DateTimeOffset plannedDrawTime, string locale, double offsetHours, string creatorName, string creatorAadObject)
        {
            return new Competition
            {
                Id = Guid.NewGuid(),
                ServiceUrl = serviceUrl,
                TenantId = tenantId,
                TeamId = teamId,
                ChannelId = channelId,
                MainActivityId = string.Empty,
                ResultActivityId = string.Empty,
                CreatedTime = _dateTimeService.UtcNow,
                PlannedDrawTime = plannedDrawTime,
                ActualDrawTime = null,
                Locale = locale,
                OffsetHours = offsetHours,
                Gift = string.Empty,
                GiftImageUrl = string.Empty,
                WinnerCount = 0,
                Status = CompetitionStatus.Draft,
                CreatorName = creatorName,
                CreatorAadObject = creatorAadObject,
                WinnerAadObjectIds = new List<string>(),
                Competitors = new List<Competitor>()
            };
        }

        public async Task<Competition> CreateDraftCompetition(string serviceUrl, Guid tenantId, string teamId, string channelId, DateTimeOffset plannedDrawTime, string locale, double offsetHours, string creatorName, string creatorAadObject)
        {
            var competition = CreateCompetition(serviceUrl, tenantId, teamId, channelId, plannedDrawTime, locale, offsetHours, creatorName, creatorAadObject);
            competition.Status = CompetitionStatus.Draft;
            await _repositoryService.UpsertOpenCompetition(competition);
            return competition;
        }

        public async Task<Competition> CreateActiveCompetition(string serviceUrl, Guid tenantId, string teamId, string channelId, DateTimeOffset plannedDrawTime, string locale, double offsetHours, string gift, string giftImageUrl, int winnerCount, string creatorName, string creatorAadObject)
        {
            var competition = CreateCompetition(serviceUrl, tenantId, teamId, channelId, plannedDrawTime, locale, offsetHours, creatorName, creatorAadObject);
            competition.Status = CompetitionStatus.Active;
            competition.Gift = gift;
            competition.GiftImageUrl = giftImageUrl;
            competition.WinnerCount = winnerCount;
            await _repositoryService.UpsertOpenCompetition(competition);
            return competition;
        }

        public async Task<Competition> GetCompetition(Guid competitionId)
        {
            var competition = await _repositoryService.GetOpenCompetition(competitionId);
            if (competition == null)
            {
                competition= await _repositoryService.GetClosedCompetition(competitionId);
            }
            return competition;
        }

        public async Task<Competition> Draw(Guid competitionId)
        {
            var competition = await _repositoryService.GetOpenCompetition(competitionId);
            if (competition.Competitors.Count > 0)
            {
                var candidates = competition.Competitors.Select(c => c.AadObjectId).ToList();
                while ((candidates.Count > 0) && (competition.WinnerAadObjectIds.Count < competition.WinnerCount))
                {
                    var winnerIndex = _randomService.Next(candidates.Count);
                    competition.WinnerAadObjectIds.Add(candidates[winnerIndex]);
                    candidates.RemoveAt(winnerIndex);
                }
            }
            competition.ActualDrawTime = _dateTimeService.UtcNow;
            competition.Status = CompetitionStatus.Completed;
            await _repositoryService.UpsertClosedCompetition(competition);
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
            var competition = await _repositoryService.GetClosedCompetition(competitionId);
            competition.ResultActivityId = resultActivityId;
            await _repositoryService.UpsertClosedCompetition(competition);
            return competition;
        }

    }
}