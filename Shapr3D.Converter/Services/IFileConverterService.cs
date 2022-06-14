using System;
using System.Threading.Tasks;

namespace Shapr3D.Converter.Services
{
    public interface IFileConverterService
    {
        Task<byte[]> ApplyConverterAndReportAsync(IProgress<int> progress, Func<byte[], byte[]> appliedConverter, byte[] source);
    }
}