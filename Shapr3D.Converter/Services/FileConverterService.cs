using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shapr3D.Converter.Services
{
    public class FileConverterService : IFileConverterService
    {
        private const int MaxDegreeOfParallelism = 8;
        private const int NumberOfChunks = 100;

        public async Task<byte[]> ApplyConverterAndReportAsync(
            IProgress<int> progress,
            Func<byte[], byte[]> appliedConverter,
            byte[] source)
        {
            var chunksBagInOrder = new ConcurrentDictionary<int, byte[]>();

            try
            {
                _ = progress ?? throw new ArgumentNullException(nameof(progress));
                _ = appliedConverter ?? throw new ArgumentNullException(nameof(appliedConverter));
                _ = source ?? throw new ArgumentNullException(nameof(source));

                var chunks = SplitByteArrayIntoNChunksWithIndex(source, (int)Math.Ceiling((double)source.Length / NumberOfChunks));

                var taskCompleted = 0;
                await Task.Run(() =>
                {
                    Parallel.ForEach(chunks, new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism }, (chunk) =>
                    {
                        var result = appliedConverter(chunk.Item2.ToArray());
                        taskCompleted++;
                        chunksBagInOrder[chunk.Item1] = result;
                        var percentageComplete = (taskCompleted * 100) / NumberOfChunks;
                        progress.Report(percentageComplete);
                    });
                });
            }
            catch (Exception)
            {
                throw;
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
