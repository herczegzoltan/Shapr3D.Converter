using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Shapr3D.Converter.Services
{
    public class FileReaderService : IFileReaderService
    {
        public async Task<byte[]> ReadFileIntoByteArrayAsync(string path)
        {
            if (string.IsNullOrEmpty(path)){ throw new ArgumentNullException(nameof(path)); }

            try
            {
                var storageFile = await StorageFile.GetFileFromPathAsync(path);
                // Do I need to add the accesscache? after modify package appmanifest
                //Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(storageFile);
                IBuffer buffer = await FileIO.ReadBufferAsync(storageFile);

                return buffer.ToArray();

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
