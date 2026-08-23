using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T: class
    {
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        IQueryable<T> Query();
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);

    }
}
