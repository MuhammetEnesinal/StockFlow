using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure.Repositories
{
    public class GenericRepository<T> (AppDbContext _context): IGenericRepository<T> where T : BaseEntity
    {
        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public async Task<PagedResult<T>> GetAllAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            // Sayfa numarası ve boyutu sınırı tek yerde (eskiden controller'lardaydı):
            // pageNumber en az 1, pageSize 1 ile 100 arası (0 veya negatifse varsayılan 10)
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

            IQueryable<T> query = _context.Set<T>().AsNoTracking();

            // Servislerdeki eski .Where(...) satırının karşılığı
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Servislerdeki eski .Include(...).ThenInclude(...) zincirinin karşılığı
            if (include != null)
            {
                query = include(query);
            }

            var totalCount = await query.CountAsync();

            // Sıralama (EF Core Pagination dokümanı): sıralama tamamen benzersiz olmalı,
            // yoksa sayfalar arasında kayıt atlanabilir. Verilen sıralamanın sonuna Id eklenir,
            // hiç sıralama verilmediyse Id'ye göre sıralanır.
            var orderedQuery = orderBy != null
                ? orderBy(query).ThenBy(e => e.Id)
                : query.OrderBy(e => e.Id);

            var items = await orderedQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }

        public IQueryable<T> Query()
        {
            return _context.Set<T>().AsNoTracking();
        }

        public void Update(T entity)
        {
            var trackedEntity = _context.Set<T>().Find(entity.Id);

            if (trackedEntity != null)
            {
                _context.Entry(trackedEntity).CurrentValues.SetValues(entity);
            }
        }
    }
}
