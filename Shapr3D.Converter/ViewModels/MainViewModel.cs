using Converter;
using Shapr3D.Converter.Enums;
using Shapr3D.Converter.Infrastructure;
using Shapr3D.Converter.Services;
using Sharp3D.Converter.DataAccess.Repository.IRepository;
using Sharp3D.Converter.Models;
using Sharp3D.Converter.Ui.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Shapr3D.Converter.ViewModels
{
    public interface IMainViewModel : INotifyPropertyChanged
    {
        RelayCommand AddCommand { get; }
        RelayCommand CloseDetailsCommand { get; }
        RelayCommand<ConverterOutputType> ConvertActionCommand { get; }
        RelayCommand DeleteAllCommand { get; }
        ObservableCollection<FileViewModel> Files { get; }
        FileViewModel SelectedFile { get; set; }

        event PropertyChangedEventHandler PropertyChanged;

        void Add();
        Task InitAsync();
    }

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
        private FileViewModel _selectedFile;
        private const string FileTypeFilter = ".shapr";
        private const Int32 ErrorAccessDenied = unchecked((Int32)0x80070005);
        private const Int32 ErrorSharingViolation = unchecked((Int32)0x80070020);

        public MainViewModel(
            IDialogService dialogService,
            IUnitOfWork unitOfWork,
            IFileConverterService fileConverterService,
            IFileReaderService fileReaderService,
            ResourceLoader resourceLoader,
            FileOpenPicker fileOpenPicker,
            FileSavePicker fileSavePicker)
        {
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

            // Rework Load with navigation to? 
            _ = InitAsync();

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

        public ObservableCollection<FileViewModel> Files { get; } = new ObservableCollection<FileViewModel>();
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
                else
                {
                    throw new FileNotFoundException();
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowExceptionModalDialog(ex, string.Format(_resourceLoader.GetString("UnexpectedError"), "adding a new file."));
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
                var input = await _fileReaderService.ReadFileIntoByteArrayAsync(_selectedFile.OriginalPath);
                var result = await _fileConverterService.ApplyConverterAndReportAsync(
                    progress,
                    conversionInfoOfSelectedFile.CancellationTokenSource,
                    ModelConverter.ConvertChunk,
                    input);
                
                conversionInfoOfSelectedFile.ConvertedResult = result;
                conversionInfoOfSelectedFile.State = ConversionState.Converted;
                
                await _unitOfWork.ModelEntity.Update(_selectedFile.ToModelEntity());
                await _unitOfWork.Save();
            }
            catch (Exception ex) when ((ex.HResult == ErrorAccessDenied) || (ex.HResult == ErrorSharingViolation))
            {
                await _dialogService.ShowExceptionModalDialog(ex, string.Format(_resourceLoader.GetString("ReadMeMessage"), _selectedFile.OriginalPath));
            }
            catch (OperationCanceledException)
            {
                conversionInfoOfSelectedFile.Progress = 0;
                conversionInfoOfSelectedFile.State = ConversionState.NotStarted;
                
                await _dialogService.ShowOkModalDialog(_resourceLoader.GetString("ConfirmationMessage"), _resourceLoader.GetString("CancelledMessage"));
            }
            catch (ConversionFailedException ex)
            {
                await _dialogService.ShowExceptionModalDialog(ex, _resourceLoader.GetString("CouldNotConvertMessage"));
            }
            catch (Exception ex)
            {
                await _dialogService.ShowExceptionModalDialog(ex, string.Format(_resourceLoader.GetString("UnexpectedError"), "converting the file."));
            }
        }

        private async void Save(FileViewModel model, ConverterOutputType outputType)
        {
            try
            {
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
        }

        private async void DeleteAll()
        {
            try
            {
                var result = await _dialogService.ShowBlockingQuestionModalDialog(
                _resourceLoader.GetString("ConfirmationMessage"),
                _resourceLoader.GetString("AreSureRemoveMessage"));
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
            catch (Exception ex)
            {
                await _dialogService.ShowExceptionModalDialog(ex, string.Format(_resourceLoader.GetString("UnexpectedError"), "delete all files."));
            }
        }
    }
}
