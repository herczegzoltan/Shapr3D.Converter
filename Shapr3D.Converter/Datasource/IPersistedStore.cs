using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shapr3D.Converter.Datasource
{
    [Flags]
    public enum ConverterOutputTypeFlags
    {
        None = 0,
        Obj = 1 << 0,
        Step = 1 << 1,
        Stl = 1 << 2,
    }

    public class ModelEntity
    {
        public Guid Id { get; set; }
        public string OriginalPath { get; set; }
        public ulong FileSize { get; set; }
        public ConverterOutputTypeFlags ConvertedTypes { get; set; }

        public void CopyPropertiesFrom(ModelEntity other)
        {
            ConvertedTypes = other.ConvertedTypes;
            FileSize = other.FileSize;
            OriginalPath = other.OriginalPath;
        }
    }

    public interface IPersistedStore
    {
        Task AddOrUpdateAsync(ModelEntity model);
        Task DeleteAllAsync();
        Task<List<ModelEntity>> GetAllAsync();
        Task InitAsync();
    }
}