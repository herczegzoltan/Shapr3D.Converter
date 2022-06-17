using Shapr3D.Converter.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shapr3D.Converter.Services
{
    /// <inheritdoc/>
    public class FileConverterService : IFileConverterService
    {
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

                var bufferSize = (int)Math.Ceiling((double)source.Length / NumberOfChunks);
                var chunks = await Task.Run(() => SplitByteArrayIntoNChunksWithIndex(source, bufferSize));

                var parallelOptions = new ParallelOptions { CancellationToken = cancellationTokenSource.Token };
                var taskCompleted = 0;
                var lockTarget = new object();

                await Task.Run(() =>
                {
                    Parallel.ForEach(chunks, parallelOptions, (chunk, state) =>
                    {
                        try
                        {
                            var result = appliedConverter(chunk.Value);
                            chunksBagInOrder[chunk.Key] = result;
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
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                throw new FileConversionException();
            }
            finally
            {
                cancellationTokenSource.Dispose();
                GC.Collect();
            }

            // Create array result of the same order of the original incoming array
            var arrayResultOfChunksBag = new List<byte>();

            foreach (var item in chunksBagInOrder.Values)
            {
                arrayResultOfChunksBag.AddRange(item);
            }

            return arrayResultOfChunksBag.ToArray();
        }


        /// <summary>
        /// Split a byte[] into chunks with specific length and index each chunk to keep the order
        /// </summary>
        /// <param name="value">The array to be split</param>
        /// <param name="bufferLength">The length of each chunk</param>
        /// <returns>An enumerable tuple with Item1=index of the chunk, Item2=chunk array</returns>
        private Dictionary<int, byte[]> SplitByteArrayIntoNChunksWithIndex(byte[] value, int bufferLength)
        {
            Dictionary<int, byte[]> result = new Dictionary<int, byte[]>();
            int countOfArray = value.Length / bufferLength;
            for (int i = 0; i < countOfArray; i++)
            {
                var singleSlice = new byte[bufferLength];

                if ((value.Length - i*bufferLength) < bufferLength)
                {
                    Array.Copy(value, i * bufferLength, singleSlice, 0, value.Length - i * bufferLength);
                }
                else
                {
                    Array.Copy(value, i * bufferLength, singleSlice, 0, bufferLength);
                }
                result.Add(i, singleSlice);
            }

            return result;
        }
    }
}
