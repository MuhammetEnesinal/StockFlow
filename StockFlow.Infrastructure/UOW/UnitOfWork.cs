using StockFlow.Application.Interfaces.UOW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure.UOW
{
    public class UnitOfWork(AppDbContext _context) : IUnitOfWork
    {
        public async Task<bool> SaveChangesAsync()
        {
          return await  _context.SaveChangesAsync()>0;
        }
    }
}
