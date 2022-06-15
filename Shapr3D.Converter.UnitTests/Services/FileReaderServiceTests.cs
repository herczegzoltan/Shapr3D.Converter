using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shapr3D.Converter.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Shapr3D.Converter.UnitTests.Services
{
    [TestClass]
    public class FileReaderServiceTests
    {
        private FileReaderService _fileReaderService;

        [TestInitialize]
        public void TestInitialize()
        {
            _fileReaderService = new FileReaderService();
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void WhenReadFileIntoByteArrayAsyncIsCalledWithWrongPath_ThenArgumentNullExceptionIsThrown(string path) 
        {
            // When & Then
            Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _ = _fileReaderService.ReadFileIntoByteArrayAsync(path));
        }

        [TestMethod]
        [DataRow("randomfilepath.txt")]
        public void WhenReadFileIntoByteArrayAsyncIsCalledWithNoExistFile_ThenFileNotFoundExceptionIsThrown(string path) 
        {
            // When & Then
            Assert.ThrowsExceptionAsync<FileNotFoundException>(() => _ = _fileReaderService.ReadFileIntoByteArrayAsync(path));
        }

        [TestMethod]
        // For this test manually necessary to provide access for Shapr3D.Converter.UnitTests (read README.md)
        public async Task WhenReadFileIntoByteArrayAsyncIsCalledExistFile_ThenFileIsReturnedInByteArray()
        {
            // Given
            string filePath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName+"\\input\\testFile.sharp";

            // When
            var readFile = await _fileReaderService.ReadFileIntoByteArrayAsync(filePath);

            // Then
            Assert.IsNotNull(readFile);
            Assert.AreEqual(typeof(byte[]), readFile.GetType());
            Assert.IsTrue(readFile.Length > 0);
        }
    }
}
