using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shapr3D.Converter.Services;
using Shapr3D.Converter.ViewModels;
using Sharp3D.Converter.DataAccess.Repository;
using Sharp3D.Converter.DataAccess.Repository.IRepository;
using Sharp3D.Converter.Models;
using Sharp3D.Converter.Ui.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        [TestInitialize]
        public void Initialize()
        {
            _fileViewModel = new Mock<IFileViewModel>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _modelEntityRepositoryMock = new Mock<IModelEntityRepository>();
            _dialogServiceMock = new Mock<IDialogService>();
            _fileConverterServiceMock = new Mock<IFileConverterService>();
            _fileReaderServiceMock = new Mock<IFileReaderService>();
            
            _unitOfWorkMock.Setup(_ => _.ModelEntity).Returns(_modelEntityRepositoryMock.Object);
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
            _dialogServiceMock.Verify(d => d.ShowExceptionModalDialog(specificException, It.IsAny<string>()));
        }


        /* --------------------------------------------
         * Command tests
         * -------------------------------------------- */

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
            _dialogServiceMock.Verify(d => d.ShowExceptionModalDialog(specificException, It.IsAny<string>()));
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
                _fileReaderServiceMock.Object);
        }
    }
}
