using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shapr3D.Converter.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shapr3D.Converter.UnitTests.Services
{
    [TestClass]
    public class FileConverterServiceTests
    {
        private FileConverterService _fileConverterService;

        [TestInitialize]
        public void TestInitialize()
        {
            _fileConverterService = new FileConverterService();
        }

        [TestMethod]
        [DataRow(true, false, false, false)]
        [DataRow(false, true, false, false)]
        [DataRow(false, false, true, false)]
        [DataRow(false, false, false, true)]
        public void WhenApplyConverterAndReportAsyncIsCalledWithNullArgument_ThenArgumentNullExceptionIsThrown(
            bool isProgressNull,
            bool isCancellationSourceNull,
            bool isAppliedConverterNull,
            bool isSourceNull)
        {
            // Given
            var progress = isProgressNull ? null : new Progress<int>();
            var cancellationTokenSource = isCancellationSourceNull ? null : new CancellationTokenSource();
            var source = isSourceNull ? null : new byte[0];
            Func<byte[], byte[]> callback = null;

            if (!isAppliedConverterNull)
            {
                callback = DummyConverter;
            }

            // When & Then
            Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _ = _fileConverterService.ApplyConverterAndReportAsync(progress, cancellationTokenSource, callback, source));
        }

        [TestMethod]
        public async Task WhenApplyConverterAndReportAsyncIsCalled_ThenProgressIsSetCorrectly()
        {
            // Given
            int progessStatus = 0;
            var progress = new Progress<int>((p) =>
            {
                progessStatus = p;
            });

            var rnd = new Random();
            var testSource = new Byte[5000];
            rnd.NextBytes(testSource);

            // When
            var result = await _fileConverterService.ApplyConverterAndReportAsync(progress, new CancellationTokenSource(), DummyConverter, testSource);
            
            // Then
            Assert.AreEqual(100, progessStatus);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task WhenApplyConverterAndReportAsyncIsCalledAndCancelled_ThenOperationCancell()
        {
            // Given
            int progessStatus = 0;
            var progress = new Progress<int>((p) =>
            {
                progessStatus = p;
            });
            var rnd = new Random();
            var testSource = new Byte[500000];
            rnd.NextBytes(testSource);
            var cts = new CancellationTokenSource();
            cts.CancelAfter(10);

            // When
            var result = await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => _ = await _fileConverterService.ApplyConverterAndReportAsync(progress, cts, DummyConverter, testSource));

            // Then
            Assert.AreNotEqual(100, progessStatus);
            Assert.AreEqual("The operation was canceled.", result.Message);
            Assert.IsTrue(cts.IsCancellationRequested);
        }


        private byte[] DummyConverter(byte[] input) => input;
    }
}
