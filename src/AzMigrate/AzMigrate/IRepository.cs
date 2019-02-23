using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzMigrate
{
    internal interface IRepository<TKey, TValue>
    {
        Task<TValue> GetAsync(TKey key);
        Task<IEnumerable<TValue>> GetAllAsync();
        Task<TValue> UpsertAsync(TValue entity);
        Task DeleteAsync(TKey key);

        IQueryable<TValue> CreateQuery();
        IEnumerable<T> ExecuteQuery<T>(string query);
    }
}
