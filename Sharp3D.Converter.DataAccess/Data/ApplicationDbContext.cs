using Microsoft.EntityFrameworkCore;
using Sharp3D.Converter.Models;

namespace Sharp3D.Converter.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ModelEntity>().ToTable("ModelEntity");

            modelBuilder.Entity<ModelEntity>().Property(p => p.Id).IsRequired();
        }

        public DbSet<ModelEntity> ModelEntity { get; set; }
    }
}
