using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using LuckyDrawBot.Infrastructure.Database;
using Microsoft.WindowsAzure.Storage.Table;

namespace LuckyDrawBot.Tests.Infrastructure.Database
{
    public class InMemoryDataTable<TSettings, TEntity> : IDataTable<TSettings, TEntity> where TSettings : DataTableSettings, new() where TEntity : ITableEntity, new()
    {
        private readonly Dictionary<string, Dictionary<string, TEntity>> _data = new Dictionary<string, Dictionary<string, TEntity>>();

        public IReadOnlyList<TEntity> AllEntities => _data.Values.SelectMany(p => p.Values).ToImmutableList();

        public Task<bool> Delete(TEntity entity)
        {
            return Delete(entity.PartitionKey, entity.RowKey);
        }

        public async Task<bool> Delete(string partitionKey, string rowKey)
        {
            Dictionary<string, TEntity> partition;
            if (_data.TryGetValue(partitionKey, out partition))
            {
                return partition.Remove(rowKey);
            }
            return await Task.FromResult(false);
        }

        public async Task InsertOrReplace(TEntity entity)
        {
            if (!_data.ContainsKey(entity.PartitionKey))
            {
                _data[entity.PartitionKey] = new Dictionary<string, TEntity>();
            }
            _data[entity.PartitionKey][entity.RowKey] = entity;
            await Task.FromResult(0);
        }

        public async Task<List<TEntity>> Query(string filterString = null, int? takeCount = null, IList<string> selectColumns = null)
        {
            var result = new List<TEntity>();
            await Query(
                (segment) => result.AddRange(segment),
                filterString,
                takeCount,
                selectColumns);
            return result;
        }

        public async Task Query(Action<List<TEntity>> segmentAction, string filterString = null, int? takeCount = null, IList<string> selectColumns = null)
        {
            var data = _data.SelectMany(d => d.Value.Select(p => p.Value));
            data = data.OrderBy(d => d.PartitionKey).OrderBy(d => d.RowKey);
            if (takeCount.HasValue)
            {
                data = data.Take(takeCount.Value);
            }
            segmentAction(data.ToList());
            await Task.FromResult(0);
        }

        public Task<TEntity> Retrieve(TEntity entity)
        {
            return Retrieve(entity.PartitionKey, entity.RowKey);
        }

        public async Task<TEntity> Retrieve(string partitionKey, string rowKey)
        {
            Dictionary<string, TEntity> partition;
            if (_data.TryGetValue(partitionKey, out partition))
            {
                TEntity entity;
                if (partition.TryGetValue(rowKey, out entity))
                {
                    return await Task.FromResult(entity);
                }
            }
            return default(TEntity);
        }
    }
}
