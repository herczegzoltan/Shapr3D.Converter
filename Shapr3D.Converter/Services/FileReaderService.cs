using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;

namespace Shapr3D.Converter.Services
{
    /// <inheritdoc/>
    public class FileReaderService : IFileReaderService
    {
        public async Task<byte[]> ReadFileIntoByteArrayAsync(string path)
        {
            if (string.IsNullOrEmpty(path)){ throw new ArgumentNullException(nameof(path)); }

            try
            {
                var storageFile = await StorageFile.GetFileFromPathAsync(path);

                if (!storageFile.IsAvailable)
                {
                    throw new FileNotFoundException(path);
                }

                var buffer = await FileIO.ReadBufferAsync(storageFile);

                return buffer.ToArray();
            }
            catch
            {
                throw;
            }
        }
    }
}
