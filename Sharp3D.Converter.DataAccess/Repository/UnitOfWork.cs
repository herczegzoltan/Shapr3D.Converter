using Sharp3D.Converter.DataAccess.Data;
using Sharp3D.Converter.DataAccess.Repository.IRepository;
using System.Threading.Tasks;

namespace Sharp3D.Converter.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            ModelEntity = new ModelEntityRepository(_context);
        }

        public IModelEntityRepository ModelEntity { get; private set; }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
    }
}
