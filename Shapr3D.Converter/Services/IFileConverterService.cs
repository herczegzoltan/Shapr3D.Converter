using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shapr3D.Converter.Services
{
    public interface IFileConverterService
    {
        /// <summary>
        /// Apply any <see cref="byte[]"/> to <see cref="byte[]"/> converter with percentage of the task completed callback and cancellation token.
        /// </summary>
        /// <param name="progress">An instance of the percentage callback, representing the operation in progress status.</param>
        /// <param name="cancellationTokenSource">An instance of the cancellation token source</param>
        /// <param name="appliedConverter">The converter to be applied.</param>
        /// <param name="source">The convertable <see cref="byte[]"/></param>
        /// <returns>Returns the converted <see cref="byte[]"/> result</returns>
        Task<byte[]> ApplyConverterAndReportAsync(IProgress<int> progress, CancellationTokenSource cancellationTokenSource, Func<byte[], byte[]> appliedConverter, byte[] source);
    }
}