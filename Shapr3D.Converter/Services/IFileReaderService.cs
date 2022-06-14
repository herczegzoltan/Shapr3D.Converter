using System.Threading.Tasks;

namespace Shapr3D.Converter.Services
{
    public interface IFileReaderService
    {
        Task<byte[]> ReadFileIntoByteArrayAsync(string path);
    }
}