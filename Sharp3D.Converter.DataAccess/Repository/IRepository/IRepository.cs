using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sharp3D.Converter.DataAccess.Repository
{
    public interface IRepository<T> where T : class
    {
        Task Add(T entity);
        Task DeleteAllAsync();
        Task<List<T>> GetAllAsync();
        Task InitAsync();
        Task Update(T entity);
    }
}