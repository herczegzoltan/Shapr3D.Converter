using Shapr3D.Converter.Enums;
using System;

namespace Shapr3D.Converter.Datasource
{
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
}