using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shapr3D.Converter.Datasource
{
    public class PersistedStore : IPersistedStore
    {
        public async Task InitAsync()
        {
            using (var db = new StoreContext())
            {
                await db.Database.EnsureCreatedAsync();
            }
        }

        public async Task AddOrUpdateAsync(ModelEntity model)
        {
            using (var db = new StoreContext())
            {
                var existing = db.Models.SingleOrDefault(m => m.Id == model.Id);
                if (existing == null)
                {
                    db.Add(model);
                }
                else
                {
                    existing.CopyPropertiesFrom(model);
                    db.Models.Update(existing);
                }
                await db.SaveChangesAsync();
            }
        }

        public async Task<List<ModelEntity>> GetAllAsync()
        {
            using (var db = new StoreContext())
            {
                return await db.Models.ToListAsync();
            }
        }

        public async Task DeleteAllAsync()
        {
            using (var db = new StoreContext())
            {
                db.Models.RemoveRange(await db.Models.ToListAsync());
                await db.SaveChangesAsync();
            }
        }

        private class StoreContext : DbContext
        {
            internal DbSet<ModelEntity> Models { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite("FileName=models.db");
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ModelEntity>().Property(p => p.Id).IsRequired();
            }
        }
    }
}
