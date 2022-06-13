using Sharp3D.Converter.Models;
using System.Threading.Tasks;

namespace Sharp3D.Converter.DataAccess.Repository.IRepository
{
    public interface IModelEntityRepository : IRepository<ModelEntity>
    {
        new Task Update(ModelEntity modelEntity);
    }
}
