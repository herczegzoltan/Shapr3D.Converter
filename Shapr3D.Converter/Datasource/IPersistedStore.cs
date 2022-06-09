using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shapr3D.Converter.Datasource
{
    public interface IPersistedStore
    {
        Task AddOrUpdateAsync(ModelEntity model);
        Task DeleteAllAsync();
        Task<List<ModelEntity>> GetAllAsync();
        Task InitAsync();
    }
}