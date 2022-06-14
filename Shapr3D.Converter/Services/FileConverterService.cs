using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Shapr3D.Converter.Services
{
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

                var chunks = SplitByteArrayIntoNChunksWithIndex(source, (int)Math.Ceiling((double)source.Length / NumberOfChunks));

                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism, CancellationToken = cancellationTokenSource.Token };

                var taskCompleted = 0;
                await Task.Run(() =>
                {
                    var thread = Thread.CurrentThread;

                    Parallel.ForEach(chunks, parallelOptions, (chunk) =>
                    {
                        parallelOptions.CancellationToken.ThrowIfCancellationRequested();

                        var result = appliedConverter(chunk.Item2);
                        taskCompleted++;
                        chunksBagInOrder[chunk.Item1] = result;
                        var percentageComplete = (taskCompleted * 100) / NumberOfChunks;
                        progress.Report(percentageComplete);
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
