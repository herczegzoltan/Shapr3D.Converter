using Shapr3D.Converter.Enums;
using Shapr3D.Converter.ViewModels;
using Sharp3D.Converter.Models;
using System;

namespace Shapr3D.Converter.Helpers
{
    public static class ConverterOutputTypeFlagsExtensions
    {
        public static ConverterOutputTypeFlags ToFlag(this ConverterOutputType type)
        {
            switch (type)
            {
                case ConverterOutputType.Obj: return ConverterOutputTypeFlags.Obj;
                case ConverterOutputType.Step: return ConverterOutputTypeFlags.Step;
                case ConverterOutputType.Stl: return ConverterOutputTypeFlags.Stl;
            }
            throw new ArgumentException($"Unknown type: parameter {nameof(type)} with value of {type}");
        }
    }
}
