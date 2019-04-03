using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LuckyDrawBot.Models;
using LuckyDrawBot.Services;

namespace LuckyDrawBot.Tests.MockServices
{
    public partial class SimpleCompetitionRepositoryService : ICompetitionRepositoryService
    {
        private readonly Dictionary<Guid, Competition> _openCompetitions = new Dictionary<Guid, Competition>();
        private readonly Dictionary<Guid, Competition> _completedCompetitions = new Dictionary<Guid, Competition>();

        public async Task<Competition> GetOpenCompetition(Guid competitionId)
        {
            if (!_openCompetitions.TryGetValue(competitionId, out Competition competition))
            {
                throw new Exception($"Cannot found the open competition {competitionId}");
            }
            return await Task.FromResult(competition);
        }

        public async Task<List<Guid>> GetOpenCompetitionIds(DateTimeOffset maxPlannedDrawTime)
        {
            var result = _openCompetitions.Values.Where(c => c.PlannedDrawTime < maxPlannedDrawTime).Select(c => c.Id).ToList();
            return await Task.FromResult(result);
        }

        public async Task UpsertOpenCompetition(Competition competition)
        {
            _openCompetitions[competition.Id] = competition;
            await Task.FromResult(0);
        }

        public async Task DeleteOpenCompetition(Guid competitionId)
        {
            _openCompetitions.Remove(competitionId);
            await Task.FromResult(0);
        }

        public async Task<Competition> GetCompletedCompetition(Guid competitionId)
        {
            if (!_completedCompetitions.TryGetValue(competitionId, out Competition competition))
            {
                throw new Exception($"Cannot found the completed competition {competitionId}");
            }
            return await Task.FromResult(competition);
        }

        public async Task UpsertCompletedCompetition(Competition competition)
        {
            _completedCompetitions[competition.Id] = competition;
            await Task.FromResult(0);
        }
    }
}