using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure.Repositories
{
    public class GenericRepository<T> (AppDbContext _context): IGenericRepository<T> where T : class
    {
        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
          return  await _context.Set<T>().ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public IQueryable<T> Query()
        {
            return _context.Set<T>().AsNoTracking();
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }
    }
}
