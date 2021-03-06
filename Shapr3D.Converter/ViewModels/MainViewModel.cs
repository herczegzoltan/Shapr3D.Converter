using Converter;
using Shapr3D.Converter.Enums;
using Shapr3D.Converter.Extensions;
using Shapr3D.Converter.Infrastructure;
using Shapr3D.Converter.Services;
using Shapr3D.Converter.ViewModels.Interfaces;
using Sharp3D.Converter.DataAccess.Repository.IRepository;
using Sharp3D.Converter.Models;
using Sharp3D.Converter.Ui.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Shapr3D.Converter.ViewModels
{
    /// <inheritdoc/>
    public class MainViewModel : IMainViewModel
    {
        // Infrastructure fields
        private readonly IDialogService _dialogService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileConverterService _fileConverterService;
        private readonly IFileReaderService _fileReaderService;
        private readonly ResourceLoader _resourceLoader;
        private readonly FileOpenPicker _fileOpenPicker;
        private readonly FileSavePicker _fileSavePicker;

        // Getter/setter backup fields
        private bool _areActionFieldsEnabled;
        private FileViewModel _selectedFile;
        private const string FileTypeFilter = ".shapr";
        private const Int32 ErrorAccessDenied = unchecked((Int32)0x80070005);
        private const Int32 ErrorSharingViolation = unchecked((Int32)0x80070020);

        // DI container picks up the constructor with the parameters
        public MainViewModel(
            IDialogService dialogService,
            IUnitOfWork unitOfWork,
            IFileConverterService fileConverterService,
            IFileReaderService fileReaderService,
            ResourceLoader resourceLoader,
            FileOpenPicker fileOpenPicker,
            FileSavePicker fileSavePicker)
        {
            AreActionFieldsEnabled = true;

            // Initialize commands
            _dialogService = dialogService;
            _unitOfWork = unitOfWork;
            _fileConverterService = fileConverterService;
            _fileReaderService = fileReaderService;
            _resourceLoader = resourceLoader;
            _fileOpenPicker = fileOpenPicker;
            _fileSavePicker = fileSavePicker;
            _fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
            _fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            _fileOpenPicker.FileTypeFilter.Add(FileTypeFilter);

            // Initialize files
            _ = InitAsync();

            // Initialize commands
            AddCommand = new RelayCommand(Add);
            DeleteAllCommand = new RelayCommand(DeleteAll);
            ConvertActionCommand = new RelayCommand<ConverterOutputType>(ConvertAction);
            CloseDetailsCommand = new RelayCommand(CloseDetails);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /* ============================================
         * Public properties
         * ============================================ */
        public FileViewModel SelectedFile
        {
            get
            {
                return _selectedFile;
            }
            set
            {
                if (_selectedFile != value)
                {
                    _selectedFile = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFile)));
                }
            }
        }

        public bool AreActionFieldsEnabled
        {
            get
            {
                return _areActionFieldsEnabled;
            }
            set
            {
                if (_areActionFieldsEnabled != value)
                {
                    _areActionFieldsEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AreActionFieldsEnabled)));
                }
            }
        }

        public ObservableCollection<FileViewModel> Files { get; } = new ObservableCollection<FileViewModel>();

        /* --------------------------------------------
         * Commands
         * -------------------------------------------- */
        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteAllCommand { get; }
        public RelayCommand<ConverterOutputType> ConvertActionCommand { get; }
        public RelayCommand CloseDetailsCommand { get; }

        /* ============================================
         * Public methods
         * ============================================ */
        public async void Add()
        {
            try
            {
                AreActionFieldsEnabled = false;

                StorageFile file = await _fileOpenPicker.PickSingleFileAsync();
                if (file != null)
                {
                    var id = Guid.NewGuid();
                    var props = await file.GetBasicPropertiesAsync();
                    var model = new FileViewModel(id, file.Path, ConverterOutputTypeFlags.None, props.Size);

                    await _unitOfWork.ModelEntity.Add(model.ToModelEntity());
                    await _unitOfWork.Save();

                    Files.Add(model);
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowExceptionModalDialog(ex, string.Format(_resourceLoader.GetString("UnexpectedError"), "adding a new file."));
            }
            finally
            {
                AreActionFieldsEnabled = true;
            }
        }

        public async Task InitAsync()
        {
            try
            {
                await _unitOfWork.ModelEntity.InitAsync();

                foreach (var model in await _unitOfWork.ModelEntity.GetAllAsync())
                {
                    Files.Add(new FileViewModel(model.Id, model.OriginalPath, model.ConvertedTypes, model.FileSize));
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowExceptionModalDialog(ex, string.Format(_resourceLoader.GetString("UnexpectedError"), "initializing."));
            }
        }

        /* ============================================
        * Private Methods
        * ============================================ */
        private void CloseDetails()
        {
            SelectedFile = null;
        }

        private async void ConvertAction(ConverterOutputType type)
        {
            var state = SelectedFile.ConversionInfos[type];

            switch (state.State)
            {
                case ConversionState.NotStarted:
                    await ConvertFile(type);
                    break;
                case ConversionState.Converting:
                    SelectedFile.CancelConversion(type);
                    break;
                case ConversionState.Converted:
                    Save(SelectedFile, type);
                    break;
            }
        }

        private async Task ConvertFile(ConverterOutputType type)
        {
            var conversionInfoOfSelectedFile = _selectedFile.ConversionInfos[type];
            conversionInfoOfSelectedFile.CancellationTokenSource = new CancellationTokenSource();

            var progress = new Progress<int>((percentageComplete) =>
            {
                conversionInfoOfSelectedFile.Progress = percentageComplete;
            });

            try
            {
                conversionInfoOfSelectedFile.State = ConversionState.Converting;

                // Store temporary the file content so if the same file has different conversion action, the file will not be read again.
                if (_selectedFile.TemporaryFileContent == null || _selectedFile.TemporaryFileContent.Length == 0)
                {
                    _selectedFile.TemporaryFileContent = await _fileReaderService.ReadFileIntoByteArrayAsync(_selectedFile.OriginalPath);
                }

                var result = await _fileConverterService.ApplyConverterAndReportAsync(
                    progress,
                    conversionInfoOfSelectedFile.CancellationTokenSource,
                    ModelConverter.ConvertChunk,
                    _selectedFile.TemporaryFileContent);

                conversionInfoOfSelectedFile.ConvertedResult = result;
                conversionInfoOfSelectedFile.State = ConversionState.Converted;

                await _unitOfWork.ModelEntity.Update(_selectedFile.ToModelEntity());
                await _unitOfWork.Save();
            }
            catch (Exception ex) when ((ex.HResult == ErrorAccessDenied) || (ex.HResult == ErrorSharingViolation))
            {
                _selectedFile.ResetProperties(type);
                await _dialogService.ShowExceptionModalDialog(ex, string.Format(_resourceLoader.GetString("ReadMeMessage"), _selectedFile.OriginalPath));
            }
            catch (OperationCanceledException)
            {
                _selectedFile.ResetProperties(type);
                await _dialogService.ShowOkModalDialog(_resourceLoader.GetString("ConfirmationMessage"), _resourceLoader.GetString("CancelledMessage"));
            }
            catch (FileConversionException)
            {
                _selectedFile.ResetProperties(type);
                await _dialogService.ShowOkModalDialog(_resourceLoader.GetString("FailMessage"), _resourceLoader.GetString("CouldNotConvertMessage"));
            }
            catch (Exception ex)
            {
                _selectedFile.ResetProperties(type);
                await _dialogService.ShowExceptionModalDialog(ex, string.Format(_resourceLoader.GetString("UnexpectedError"), "converting the file."));
            }
        }

        private async void Save(FileViewModel model, ConverterOutputType outputType)
        {
            try
            {
                AreActionFieldsEnabled = false;

                _fileSavePicker.FileTypeChoices.Clear();
                _fileSavePicker.FileTypeChoices.Add(string.Format("{0} file",
                    outputType.ToString().ToLower()),
                    new List<string>() { string.Format(".{0}",
                    outputType.ToString().ToLower()) });

                _fileSavePicker.SuggestedFileName = Path.GetFileNameWithoutExtension(model.OriginalPath);

                var savedFile = await _fileSavePicker.PickSaveFileAsync();

                // References from https://docs.microsoft.com/en-us/windows/uwp/files/best-practices-for-writing-to-files
                int retryAttempts = 5;
                if (savedFile != null)
                {
                    while (retryAttempts > 0)
                    {
                        retryAttempts--;
                        var convertedFile = model.ConversionInfos[outputType];

                        await FileIO.WriteBytesAsync(savedFile, convertedFile.ConvertedResult);
                        await _dialogService.ShowOkModalDialog(_resourceLoader.GetString("SuccessMessage"),
                           string.Format(_resourceLoader.GetString("SavedToMessage"), savedFile.Path));
                        break;
                    }
                }
                else
                {
                    // The operation was cancelled in the picker dialog.
                }
            }
            catch (Exception ex) when ((ex.HResult == ErrorAccessDenied) || (ex.HResult == ErrorSharingViolation))
            {
                await _dialogService.ShowExceptionModalDialog(ex, string.Format(_resourceLoader.GetString("ReadMeMessage"), model.OriginalPath));
            }
            catch (Exception ex)
            {
                await _dialogService.ShowExceptionModalDialog(ex, string.Format(_resourceLoader.GetString("UnexpectedError"), "saving the file."));
            }
            finally 
            {
                AreActionFieldsEnabled = true;
            }
        }

        private async void DeleteAll()
        {
            try
            {
                if (Files.Count > 0)
                {
                    AreActionFieldsEnabled = false;

                    var title = _resourceLoader.GetString("ConfirmationMessage");
                    var description = _resourceLoader.GetString("AreSureRemoveMessage");

                    if (Files.Any(_ => _.IsConverting))
                    {
                        title = _resourceLoader.GetString("ConversionInProgressMessage");
                        description = _resourceLoader.GetString("AreSureRemoveInProgressMessage");
                    }

                    var result = await _dialogService.ShowBlockingQuestionModalDialog(title, description);

                    if (result ?? false)
                    {
                        foreach (var model in Files)
                        {
                            model.CancelConversions();
                        }

                        await _unitOfWork.ModelEntity.DeleteAllAsync();
                        await _unitOfWork.Save();

                        SelectedFile = null;
                        Files.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowExceptionModalDialog(ex, string.Format(_resourceLoader.GetString("UnexpectedError"), "delete all files."));
            }
            finally 
            {
                AreActionFieldsEnabled  = true;
            }
        }
    }
}
