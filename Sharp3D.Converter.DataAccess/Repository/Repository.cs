using Microsoft.EntityFrameworkCore;
using Sharp3D.Converter.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sharp3D.Converter.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _applicationDbContext;

        private DbSet<T> _entities;

        public Repository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
            _entities = _applicationDbContext.Set<T>();
        }

        public async Task InitAsync()
        {
            await _applicationDbContext.Database.EnsureCreatedAsync();
        }

        public async Task Add(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Insert entity is null");
            }

            await _entities.AddAsync(entity);
        }

        public async Task Update(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Update entity is null");
            }

            await Task.Run(() => _entities.Update(entity));
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _entities.ToListAsync();
        }

        public async Task DeleteAllAsync()
        {
            _entities.RemoveRange(await _entities.ToListAsync());
        }
    }
}
