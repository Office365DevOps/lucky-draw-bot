using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly Dictionary<Guid, Competition> _openCompetitions = new Dictionary<Guid, Competition>();
        private readonly Dictionary<Guid, Competition> _completedCompetitions = new Dictionary<Guid, Competition>();

        public async Task<Competition> GetOpenCompetition(Guid competitionId)
        {
            lock(_openCompetitions)
            {
                if (!_openCompetitions.TryGetValue(competitionId, out Competition competition))
                {
                    throw new Exception($"Cannot found the open competition {competitionId}");
                }
                return competition;
            }
        }

        public async Task<List<Guid>> GetOpenCompetitionIds(DateTimeOffset maxPlannedDrawTime)
        {
            lock(_openCompetitions)
            {
                return _openCompetitions.Values.Where(c => c.PlannedDrawTime < maxPlannedDrawTime).Select(c => c.Id).ToList();
            }
        }

        public async Task UpsertOpenCompetition(Competition competition)
        {
            lock(_openCompetitions)
            {
                _openCompetitions[competition.Id] = competition;
            }
        }

        public async Task DeleteOpenCompetition(Guid competitionId)
        {
            lock(_openCompetitions)
            {
                _openCompetitions.Remove(competitionId);
            }
        }

        public async Task<Competition> GetCompletedCompetition(Guid competitionId)
        {
            lock(_completedCompetitions)
            {
                if (!_completedCompetitions.TryGetValue(competitionId, out Competition competition))
                {
                    throw new Exception($"Cannot found the completed competition {competitionId}");
                }
                return competition;
            }
        }

        public async Task UpsertCompletedCompetition(Competition competition)
        {
            lock(_completedCompetitions)
            {
                _completedCompetitions[competition.Id] = competition;
            }
        }
    }
}