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
using System.Windows.Input;
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
        private IUnitOfWork _unitOfWork;
        private readonly IFileConverterService _fileConverterService;
        private readonly IFileReaderService _fileReaderService;

        // Getter/setter backup fields
        private FileViewModel _fileViewModel;
        private const Int32 ErrorAccessDenied = unchecked((Int32)0x80070005);
        const Int32 ErrorSharingViolation = unchecked((Int32)0x80070020);

        public ICommand TreeViewLoadOnDemandCommand { get; set; }

        public MainViewModel(
            IDialogService dialogService,
            IUnitOfWork unitOfWork,
            IFileConverterService fileConverterService,
            IFileReaderService fileReaderService)
        {
            _dialogService = dialogService;
            _unitOfWork = unitOfWork;
            _fileConverterService = fileConverterService;
            _fileReaderService = fileReaderService;

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
                return _fileViewModel;
            }
            set
            {
                if (_fileViewModel != value)
                {
                    _fileViewModel = value;
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
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".shapr");

            StorageFile file = await picker.PickSingleFileAsync();
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

        public async Task InitAsync()
        {
            await _unitOfWork.ModelEntity.InitAsync();

            foreach (var model in await _unitOfWork.ModelEntity.GetAllAsync())
            {
                Files.Add(new FileViewModel(model.Id, model.OriginalPath, model.ConvertedTypes, model.FileSize));
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
                    await ConvertFile(SelectedFile, type);
                    break;
                case ConversionState.Converting:
                    SelectedFile.CancelConversion(type);
                    break;
                case ConversionState.Converted:
                    Save(SelectedFile, type);
                    break;
            }
        }

        private async Task ConvertFile(FileViewModel model, ConverterOutputType type)
        {
            var conversionStatus = _fileViewModel.ConversionInfos[type];
            conversionStatus.CancellationTokenSource = new CancellationTokenSource();

            var progress = new Progress<int>((percentageComplete) =>
            {
                conversionStatus.Progress = percentageComplete;
            });

            try
            {
                conversionStatus.State = ConversionState.Converting;
                var input = await _fileReaderService.ReadFileIntoByteArrayAsync(model.OriginalPath);
                var result = await _fileConverterService.ApplyConverterAndReportAsync(progress, conversionStatus.CancellationTokenSource, ConvertChunk, input);
                conversionStatus.ConvertedResult = result;

                conversionStatus.State = ConversionState.Converted;
                await _unitOfWork.ModelEntity.Update(model.ToModelEntity());
                await _unitOfWork.Save();
            }
            catch (Exception ex) when ((ex.HResult == ErrorAccessDenied) || (ex.HResult == ErrorSharingViolation))
            {
                await _dialogService.ShowExceptionModalDialog(ex, $"{model.OriginalPath} (Read README.md for instructions.)");
            }
            catch (OperationCanceledException)
            {
                // Should I dispose CancellationTokenSource here or insude ApplyConverterAndReportAsync?
                conversionStatus.Progress = 0;
                conversionStatus.State = ConversionState.NotStarted;
                await _dialogService.ShowOkModalDialog("Confirmation", "The conversion was cancelled.");
            }
            catch (ConversionFailedException ex)
            {
                await _dialogService.ShowExceptionModalDialog(ex, "The file could not be converted.");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowExceptionModalDialog(ex, ex.StackTrace);
            }
        }

        private async void Save(FileViewModel model, ConverterOutputType outputType)
        {
            try
            {
                var savePicker = new FileSavePicker();
                savePicker.FileTypeChoices.Add
                                      (string.Format("{0} file", outputType.ToString().ToLower()), new List<string>() { string.Format(".{0}", outputType.ToString().ToLower()) });

                savePicker.SuggestedFileName = Path.GetFileNameWithoutExtension(model.OriginalPath);
                var savedFile = await savePicker.PickSaveFileAsync();

                // References from https://docs.microsoft.com/en-us/windows/uwp/files/best-practices-for-writing-to-files
                int retryAttempts = 5;
                if (savedFile != null)
                {
                    while (retryAttempts > 0)
                    {
                        retryAttempts--;
                        var convertedFile = model.ConversionInfos[outputType];

                        await FileIO.WriteBytesAsync(savedFile, convertedFile.ConvertedResult);
                        await _dialogService.ShowOkModalDialog("Success", $"The file has been saved to {savedFile.Path}");
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
                await _dialogService.ShowExceptionModalDialog(ex, $"{model.OriginalPath} (Read README.md for instructions.)");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowExceptionModalDialog(ex, "Unexpected error occured during file save.");
            }
        }

        private async void DeleteAll()
        {
            var result = await _dialogService.ShowBlockingQuestionModalDialog("Confirmation", "Are you sure you want to remove all files?");
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

        // Temp Convertchunk to remove delay
        public static byte[] ConvertChunk(byte[] bytes)
        {
            int num = bytes.Length;
            int num2 = 300000;
            int num3 = (int)Math.Pow(2.0, num / 1000000) * 1000;
            //Thread.Sleep((num3 > num2) ? num2 : num3);
            byte[] array = new byte[num];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = bytes[num - i - 1];
            }
            return array;
        }
    }
}
