using System.Threading.Tasks;

namespace Shapr3D.Converter.Services
{
    public interface IFileReaderService
    {
        /// <summary>
        /// Read file into <see cref="byte[]"/>
        /// </summary>
        /// <param name="path">The path as <see cref="string"/> of the readable file</param>
        /// <returns>Returns the read file as <see cref="byte[]"/></returns>
        Task<byte[]> ReadFileIntoByteArrayAsync(string path);
    }
}