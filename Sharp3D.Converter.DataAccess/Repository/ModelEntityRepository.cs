using Sharp3D.Converter.DataAccess.Data;
using Sharp3D.Converter.DataAccess.Repository.IRepository;
using Sharp3D.Converter.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Sharp3D.Converter.DataAccess.Repository
{
    internal class ModelEntityRepository : Repository<ModelEntity>, IModelEntityRepository
    {
        private readonly ApplicationDbContext _context;

        public ModelEntityRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public new async Task Update(ModelEntity modelEntity)
        {
            var objFromDb = _context.ModelEntity.FirstOrDefault(_ => _.Id == modelEntity.Id);

            if (objFromDb != null) 
            {
                await Task.Run(() => Update());
            }

            void Update()
            {
                objFromDb.ConvertedTypes = modelEntity.ConvertedTypes;
                objFromDb.FileSize = modelEntity.FileSize;
                objFromDb.OriginalPath = modelEntity.OriginalPath;
            }
        }
    }
}
