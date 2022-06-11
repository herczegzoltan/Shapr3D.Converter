using System;

namespace Shapr3D.Converter.Enums
{
    [Flags]
    public enum ConverterOutputTypeFlags
    {
        None = 0,
        Obj = 1 << 0,
        Step = 1 << 1,
        Stl = 1 << 2,
    }
}