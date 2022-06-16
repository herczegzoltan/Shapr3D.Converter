using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Shapr3D.Converter.Services
{
    /// <inheritdoc/>
    public class FileConverterService : IFileConverterService
    {
        private const int MaxDegreeOfParallelism = 8;
        private const int NumberOfChunks = 100;

        public async Task<byte[]> ApplyConverterAndReportAsync(
            IProgress<int> progress,
            CancellationTokenSource cancellationTokenSource,
            Func<byte[], byte[]> appliedConverter,
            byte[] source)
        {
            var chunksBagInOrder = new ConcurrentDictionary<int, byte[]>();

            try
            {
                _ = progress ?? throw new ArgumentNullException(nameof(progress));
                _ = cancellationTokenSource ?? throw new ArgumentNullException(nameof(cancellationTokenSource));
                _ = appliedConverter ?? throw new ArgumentNullException(nameof(appliedConverter));
                _ = source ?? throw new ArgumentNullException(nameof(source));

                var chunks = await Task.Run(() => SplitByteArrayIntoNChunksWithIndex(source, (int)Math.Ceiling((double)source.Length / NumberOfChunks)));
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism, CancellationToken = cancellationTokenSource.Token };
                var taskCompleted = 0;
                var lockTarget = new object();

                await Task.Run(() =>
                {
                    Parallel.ForEach(chunks, parallelOptions, (chunk, state) =>
                    {
                        try
                        {
                            var result = appliedConverter(chunk.Item2);
                            chunksBagInOrder[chunk.Item1] = result;
                        }
                        finally
                        {
                            lock (lockTarget)
                            {
                                taskCompleted++;
                                var percentageComplete = (taskCompleted * 100) / NumberOfChunks;
                                progress.Report(percentageComplete);
                            }
                        }
                    });
                });
            }
            catch (OperationCanceledException ex)
            {
                throw ex;
            }
            catch (Exception)
            {
                throw;
            }
            finally 
            {
                cancellationTokenSource.Dispose();
            }

            return chunksBagInOrder.Values.SelectMany(x => x).ToArray();
        }

        /// <summary>
        /// Split a byte[] into chunks with specific length and index each chunk to keep the order
        /// </summary>
        /// <param name="value">The array to be split</param>
        /// <param name="bufferLength">The length of each chunk</param>
        /// <returns>An enumerable tuple with Item1=index of the chunk, Item2=chunk array</returns>
        private IEnumerable<(int, byte[])> SplitByteArrayIntoNChunksWithIndex(byte[] value, int bufferLength)
        {
            int countOfArray = value.Length / bufferLength;
            if (value.Length % bufferLength > 0)
            {
                countOfArray++;
            }
            for (int i = 0; i < countOfArray; i++)
            {
                yield return (i, value.Skip(i * bufferLength).Take(bufferLength).ToArray());
            }
        }
    }
}
