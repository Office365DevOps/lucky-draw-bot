using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LuckyDrawBot.Infrastructure.Database
{
    public interface IDataTable<TSettings, TEntity> where TSettings : DataTableSettings, new() where TEntity : ITableEntity, new()
    {
        Task InsertOrReplace(TEntity entity);
        Task<TEntity> Retrieve(TEntity entity);
        Task<TEntity> Retrieve(string partitionKey, string rowKey);
        Task<bool> Delete(string partitionKey, string rowKey);
        Task<List<TEntity>> Query(string filterString = null, int? takeCount = null, IList<string> selectColumns = null);
        Task Query(Action<List<TEntity>> segmentAction, string filterString = null, int? takeCount = null, IList<string> selectColumns = null);
    }

    public class DataTable<TSettings, TEntity> : IDataTable<TSettings, TEntity> where TSettings : DataTableSettings, new() where TEntity : ITableEntity, new()
    {
        private readonly CloudTable _table;

        public DataTable(IOptions<TSettings> options)
        {
            var settings = options.Value;

            var storageAccount = CloudStorageAccount.Parse(settings.ConnectionString);
            var client = storageAccount.CreateCloudTableClient();
            _table = client.GetTableReference(GetTableName());
        }

        private string GetTableName()
        {
            var type = typeof(TEntity);
            var attributes = type.GetCustomAttributes(typeof(TableAttribute), false);
            if (attributes.Length > 0)
            {
                var tableAttribute = (TableAttribute)attributes[0];
                return tableAttribute.Name;
            }

            var typeName = type.Name;
            var tableName = typeName.EndsWith("Entity") ? typeName.Substring(0, typeName.Length - "Entity".Length) : typeName;
            tableName = tableName.ToLowerInvariant();
            return tableName;
        }

        public async Task InsertOrReplace(TEntity entity)
        {
            TableOperation operation = TableOperation.InsertOrReplace(entity);
            await _table.ExecuteAsync(operation);
        }

        public Task<TEntity> Retrieve(TEntity entity)
        {
            return Retrieve(entity.PartitionKey, entity.RowKey);
        }

        public async Task<TEntity> Retrieve(string partitionKey, string rowKey)
        {
            TableOperation operation = TableOperation.Retrieve<TEntity>(partitionKey, rowKey);
            var result = await _table.ExecuteAsync(operation);
            if (result.Result != null)
            {
                return (TEntity)result.Result;
            }
            return default(TEntity);
        }

        public async Task<bool> Delete(string partitionKey, string rowKey)
        {
            TableOperation operation = TableOperation.Retrieve<TEntity>(partitionKey, rowKey);
            var result = await _table.ExecuteAsync(operation);
            if (result.Result != null)
            {
                operation = TableOperation.Delete((TEntity)result.Result);
                await _table.ExecuteAsync(operation);
                return true;
            }
            return false;
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
            var query = new TableQuery<TEntity>();
            if (!string.IsNullOrEmpty(filterString))
            {
                query = query.Where(filterString);
            }
            if (takeCount.HasValue)
            {
                query = query.Take(takeCount.Value);
            }
            if (selectColumns != null)
            {
                query = query.Select(selectColumns);
            }

            List<TEntity> result;
            TableContinuationToken token = null;
            do
            {
                var seg = await _table.ExecuteQuerySegmentedAsync<TEntity>(query, token);
                token = seg.ContinuationToken;
                result = seg.ToList();
                segmentAction(result);
            } while (token != null && (query.TakeCount == null || result.Count < query.TakeCount.Value));
        }
    }

}
