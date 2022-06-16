using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shapr3D.Converter.Enums;
using Shapr3D.Converter.Extensions;
using Shapr3D.Converter.Services;
using Shapr3D.Converter.ViewModels;
using Sharp3D.Converter.DataAccess.Repository.IRepository;
using Sharp3D.Converter.Models;
using Sharp3D.Converter.Ui.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage.Pickers;

namespace Shapr3D.Converter.UnitTests.ViewModels
{
    [TestClass]
    public class MainViewModelTests
    {
        private Mock<IFileViewModel> _fileViewModel;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IModelEntityRepository> _modelEntityRepositoryMock;
        private Mock<IDialogService> _dialogServiceMock;
        private Mock<IFileConverterService> _fileConverterServiceMock;
        private Mock<IFileReaderService> _fileReaderServiceMock;
        private FileSavePicker _fileSavePicker;

        [TestInitialize]
        public void Initialize()
        {
            _fileViewModel = new Mock<IFileViewModel>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _modelEntityRepositoryMock = new Mock<IModelEntityRepository>();
            _dialogServiceMock = new Mock<IDialogService>();
            _fileConverterServiceMock = new Mock<IFileConverterService>();
            _fileReaderServiceMock = new Mock<IFileReaderService>();
            _fileSavePicker = new FileSavePicker();

            _unitOfWorkMock.Setup(_ => _.ModelEntity).Returns(_modelEntityRepositoryMock.Object);
            _modelEntityRepositoryMock.Setup(_ => _.GetAllAsync()).Returns(Task.FromResult(new List<ModelEntity>()));
        }

        /* --------------------------------------------
         * Instantiate tests
         * -------------------------------------------- */

        [TestMethod]
        public void WhenMainViewModelInstansiated_ThenInitAsyncIsCalled()
        {
            _ = InstantiateViewModel();

            // When & Then
            _unitOfWorkMock.Verify(m => m.ModelEntity.InitAsync(), Times.Once);
        }

        [TestMethod]
        [DataRow(1, true)]
        [DataRow(0, false)]
        public void WhenInstansiatedAndInitAsyncIsCalled_ThenFilesAreFetchedFromDatabase(int expectedNumberOfFiles, bool isFileInclude)
        {
            // Given
            if (isFileInclude)
            {
                SetupOneFileToBeLoadedForDatabase();
            }

            // When
            var sut = InstantiateViewModel();

            // Then
            Assert.AreEqual(expectedNumberOfFiles, sut.Files.Count());
        }

        [TestMethod]
        public void WhenInstansiatedAndInitAsyncIsCalledAndExceptionIsThrown_ThenDialogIsShown()
        {
            // Given
            var specificException = new Exception();
            _unitOfWorkMock.Setup(_ => _.ModelEntity.GetAllAsync())
               .Throws(specificException);
            var sut = InstantiateViewModel();

            // When
            _ = sut.InitAsync();

            // Then
            _dialogServiceMock.Verify(d => d.ShowExceptionModalDialog(specificException, It.IsAny<string>(), It.IsAny<string>()));
        }


        /* --------------------------------------------
         * Command tests
         * -------------------------------------------- */

        [TestMethod]
        public void WhenCloseCommand_ThenSelectedFileIsCleared()
        {
            // Given
            var sut = InstantiateViewModel();
            sut.SelectedFile = new FileViewModel(It.IsAny<Guid>(), "RandomForOriginalPath", It.IsAny<ConverterOutputTypeFlags>(), It.IsAny<ulong>());

            // When
            sut.CloseDetailsCommand.Execute(null);

            // Then
            Assert.IsNull(sut.SelectedFile);
        }

        [TestMethod]
        public void WhenDeleteAllCommandAndExceptionIsThrown_ThenDialogIsShown()
        {
            // Given
            var specificException = new Exception();
            _unitOfWorkMock.Setup(_ => _.ModelEntity.DeleteAllAsync())
               .Throws(specificException);
            var sut = InstantiateViewModel();

            bool? isDeleteSelected = true;
            _dialogServiceMock.Setup(_ => _.ShowBlockingQuestionModalDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(isDeleteSelected));

            // When
            sut.DeleteAllCommand.Execute(null);

            // Then
            _dialogServiceMock.Verify(d => d.ShowExceptionModalDialog(specificException, It.IsAny<string>(), It.IsAny<string>()));
        }

        [TestMethod]
        [DataRow(true, 0)]
        [DataRow(false, 1)]
        public void WhenDeleteAllCommand_ThenAllFilesAreCleared(bool? isDeleteClicked, int expectedFileRemained)
        {
            // Before Given
            SetupOneFileToBeLoadedForDatabase();
            var sut = InstantiateViewModel();

            // Before Then
            Assert.AreEqual(1, sut.Files.Count());

            _dialogServiceMock.Setup(_ => _.ShowBlockingQuestionModalDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(isDeleteClicked));

            // When
            sut.DeleteAllCommand.Execute(null);

            // After Then
            _dialogServiceMock.Verify(_ => _.ShowBlockingQuestionModalDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            Assert.AreEqual(expectedFileRemained, sut.Files.Count());
        }

        [TestMethod]
        [DataRow(ConversionState.NotStarted, ConverterOutputType.Obj)]
        [DataRow(ConversionState.Converted, ConverterOutputType.Obj)]
        [DataRow(ConversionState.Converting, ConverterOutputType.Obj)]
        [DataRow(ConversionState.NotStarted, ConverterOutputType.Stl)]
        [DataRow(ConversionState.Converted, ConverterOutputType.Stl)]
        [DataRow(ConversionState.Converting, ConverterOutputType.Stl)]
        [DataRow(ConversionState.NotStarted, ConverterOutputType.Step)]
        [DataRow(ConversionState.Converted, ConverterOutputType.Step)]
        [DataRow(ConversionState.Converting, ConverterOutputType.Step)]
        public void WhenConvertActionIsCalledWithDifferentConversionStates_ThenCorrectActionIsCalled(ConversionState conversionState, ConverterOutputType converterOutputType)
        {
            // Given
            var sut = InstantiateViewModel();
            var fileName = "RandomFileName.shapr";
            var filePath = $"RandomForOriginalPath\\{fileName}";
            var file = new FileViewModel(It.IsAny<Guid>(), filePath, (ConverterOutputTypeFlags)converterOutputType, It.IsAny<ulong>());
            sut.SelectedFile = file;

            switch (converterOutputType)
            {
                case ConverterOutputType.Stl:
                    sut.SelectedFile.StlConversionInfo.State = conversionState;
                    break;
                case ConverterOutputType.Obj:
                    sut.SelectedFile.ObjConversionInfo.State = conversionState;
                    break;
                case ConverterOutputType.Step:
                    sut.SelectedFile.StepConversionInfo.State = conversionState;
                    break;
                default:
                    break;
            }

            // When
            sut.ConvertActionCommand.Execute(converterOutputType);

            // Then
            switch (conversionState)
            {
                case ConversionState.NotStarted: // Converting
                    _fileReaderServiceMock.Verify(_ => _.ReadFileIntoByteArrayAsync(filePath));
                    break;
                case ConversionState.Converting: // Cancel
                    Assert.IsTrue(sut.SelectedFile.ConversionInfos[converterOutputType].CancellationTokenSource.IsCancellationRequested);
                    break;
                case ConversionState.Converted: // Save
                    var expectedFileName = "RandomFileName";
                    Assert.AreEqual(expectedFileName, _fileSavePicker.SuggestedFileName);
                    break;
                default:
                    break;
            }
        }

        [TestMethod]
        [DataRow(ConverterOutputType.Obj)]
        [DataRow(ConverterOutputType.Stl)]
        [DataRow(ConverterOutputType.Step)]
        public void WhenConvertCommandWithValidInput_ThenFileIsConvertedAndSetPropertiesCorrectly(ConverterOutputType converterOutputType)
        {
            // Given
            var sut = InstantiateViewModel();
            var fileName = "RandomFileName.shapr";
            var filePath = $"RandomForOriginalPath\\{fileName}";
            var file = new FileViewModel(It.IsAny<Guid>(), filePath, converterOutputType.ToFlag(), It.IsAny<ulong>());
            sut.SelectedFile = file;
            sut.SelectedFile.StlConversionInfo.State = ConversionState.NotStarted;
            sut.SelectedFile.ObjConversionInfo.State = ConversionState.NotStarted;
            sut.SelectedFile.StepConversionInfo.State = ConversionState.NotStarted;

            // Setup services
            var readFile = GenerateRandomFile();
            var converterResult = GenerateRandomFile();
            _fileReaderServiceMock.Setup(_ => _.ReadFileIntoByteArrayAsync(It.IsAny<string>())).Returns(Task.FromResult(readFile));
            _fileConverterServiceMock.Setup(_ => _.ApplyConverterAndReportAsync(It.IsAny<IProgress<int>>(),
                It.IsAny<CancellationTokenSource>(),
                It.IsAny<Func<byte[], byte[]>>(),
                It.IsAny<byte[]>()))
                .Returns(Task.FromResult(converterResult));

            // When
            sut.ConvertActionCommand.Execute(converterOutputType);

            // Then
            switch (converterOutputType)
            {
                case ConverterOutputType.Stl:
                    Assert.AreEqual(ConversionState.Converted, sut.SelectedFile.StlConversionInfo.State);
                    Assert.AreEqual(converterResult, sut.SelectedFile.StlConversionInfo.ConvertedResult);
                    Assert.AreEqual(100, sut.SelectedFile.StlConversionInfo.Progress);
                    break;
                case ConverterOutputType.Obj:
                    Assert.AreEqual(ConversionState.Converted, sut.SelectedFile.ObjConversionInfo.State);
                    Assert.AreEqual(converterResult, sut.SelectedFile.ObjConversionInfo.ConvertedResult);
                    Assert.AreEqual(100, sut.SelectedFile.ObjConversionInfo.Progress);
                    break;
                case ConverterOutputType.Step:
                    Assert.AreEqual(ConversionState.Converted, sut.SelectedFile.StepConversionInfo.State);
                    Assert.AreEqual(converterResult, sut.SelectedFile.StepConversionInfo.ConvertedResult);
                    Assert.AreEqual(100, sut.SelectedFile.StepConversionInfo.Progress);
                    break;
                default:
                    break;
            }

            _unitOfWorkMock.Verify(m => m.ModelEntity.Update(It.IsAny<ModelEntity>()), Times.Once);
            _unitOfWorkMock.Verify(m => m.Save(), Times.Once);

            byte[] GenerateRandomFile()
            {
                var rnd = new Random();
                var testSource = new Byte[5000];
                rnd.NextBytes(testSource);
                return testSource;
            }
        }

        /* ============================================
         * Private methods
         * ============================================ */

        private void SetupOneFileToBeLoadedForDatabase()
        {
            var models = new List<ModelEntity>() {
                new ModelEntity()
                {
                    Id = It.IsAny<Guid>(),
                    OriginalPath = "RandomFilePath",
                    ConvertedTypes = It.IsAny<ConverterOutputTypeFlags>(),
                    FileSize = It.IsAny<ulong>()
                } };

            _modelEntityRepositoryMock.Setup(_ => _.GetAllAsync()).Returns(Task.FromResult(models));
        }

        private MainViewModel InstantiateViewModel()
        {
            return new MainViewModel(
                _dialogServiceMock.Object,
                _unitOfWorkMock.Object,
                _fileConverterServiceMock.Object,
                _fileReaderServiceMock.Object,
                new ResourceLoader(),
                new FileOpenPicker(),
                _fileSavePicker);
        }
    }
}
