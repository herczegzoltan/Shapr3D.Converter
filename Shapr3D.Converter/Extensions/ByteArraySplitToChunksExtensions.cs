using System.Collections.Generic;
using System.Linq;

namespace Shapr3D.Converter.Extensions
{
    public static class ByteArraySplitToChunksExtensions
    {
        public static IEnumerable<byte[]> Split(this byte[] value, int bufferLength)
        {
            int countOfArray = value.Length / bufferLength;
            if (value.Length % bufferLength > 0)
            {
                countOfArray++;
            }
            for (int i = 0; i < countOfArray; i++)
            {
                yield return value.Skip(i * bufferLength).Take(bufferLength).ToArray();
            }
        }
    }
}
