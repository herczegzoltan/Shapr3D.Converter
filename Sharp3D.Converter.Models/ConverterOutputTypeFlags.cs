using System;

namespace Sharp3D.Converter.Models
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