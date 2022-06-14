using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shapr3D.Converter.Services
{
    public interface IFileConverterService
    {
        Task<byte[]> ApplyConverterAndReportAsync(IProgress<int> progress, CancellationTokenSource cancellationTokenSource, Func<byte[], byte[]> appliedConverter, byte[] source);
    }
}