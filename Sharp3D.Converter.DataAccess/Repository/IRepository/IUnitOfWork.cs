using System.Threading.Tasks;

namespace Sharp3D.Converter.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IModelEntityRepository ModelEntity { get; }

        Task Save();
    }
}
