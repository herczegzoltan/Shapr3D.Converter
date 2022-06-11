using Converter;
using Shapr3D.Converter.Datasource;
using Shapr3D.Converter.Enums;
using Shapr3D.Converter.Extensions;
using Shapr3D.Converter.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        // Getter/setter backup fields
        private IPersistedStore _persistedStore;
        private FileViewModel _fileViewModel;

        public MainViewModel()
        {
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
                await _persistedStore.AddOrUpdateAsync(model.ToModelEntity());

                Files.Add(model);
            }
        }

        public async Task InitAsync()
        {
            _persistedStore = new PersistedStore();
            await _persistedStore.InitAsync();

            foreach (var model in await _persistedStore.GetAllAsync())
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

        //when click
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
            var state = _fileViewModel.ConversionInfos[type];
            Progress<int> progress = new Progress<int>((p) =>
            {
                state.Progress = p;
            });

            state.State = ConversionState.Converting;

            try
            {
                await Convert(model, progress, type);

                if (state.IsCancellationRequested)
                {
                    // Do I need this? or just throw a task cancelled exception?
                    state.State = ConversionState.NotStarted;
                    state.Progress = 0;
                    state.IsCancellationRequested = false;
                }
                else
                {
                    state.State = ConversionState.Converted;
                }

                await _persistedStore.AddOrUpdateAsync(model.ToModelEntity());
            }
            catch (TaskCanceledException)
            {
                // Do I need this? or just throw a task cancelled exception?
                state.Progress = 0;
                //state.IsCancellationRequested = false;
                state.State = ConversionState.NotStarted;
            }
        }

        private async void Save(FileViewModel model, ConverterOutputType outputType)
        {
            var savePicker = new FileSavePicker();
            savePicker.FileTypeChoices.Add
                                  (string.Format("{0} file", outputType.ToString().ToLower()), new List<string>() { string.Format(".{0}", outputType.ToString().ToLower()) });


            savePicker.SuggestedFileName = Path.GetFileNameWithoutExtension(model.OriginalPath);
            StorageFile savedFile = await savePicker.PickSaveFileAsync();
            // TODO https://docs.microsoft.com/en-us/windows/uwp/files/

            // References from https://docs.microsoft.com/en-us/windows/uwp/files/best-practices-for-writing-to-files
            Int32 retryAttempts = 5;

            const Int32 ERROR_ACCESS_DENIED = unchecked((Int32)0x80070005);
            const Int32 ERROR_SHARING_VIOLATION = unchecked((Int32)0x80070020);

            if (savedFile != null)
            {
                // Application now has read/write access to the picked file.
                while (retryAttempts > 0)
                {
                    try
                    {
                        retryAttempts--;
                        var convertedFile = model.ConversionInfos[outputType];

                        await FileIO.WriteBytesAsync(savedFile, convertedFile.ConvertedFile.ToArray());
                        break;
                    }
                    catch (Exception ex) when ((ex.HResult == ERROR_ACCESS_DENIED) ||
                                               (ex.HResult == ERROR_SHARING_VIOLATION))
                    {
                        // This might be recovered by retrying, otherwise let the exception be raised.
                        // The app can decide to wait before retrying.
                    }
                }
            }
            else
            {
                // The operation was cancelled in the picker dialog.
            }

        }

        private async void DeleteAll()
        {
            foreach (var model in Files)
            {
                model.CancelConversions();
            }

            await _persistedStore.DeleteAllAsync();

            SelectedFile = null;
            Files.Clear();
        }

        // TODO
        private async Task Convert(FileViewModel model, IProgress<int> progress, ConverterOutputType outputType)
        {
            // Change to real file read!
            Random rnd = new Random();
            Byte[] b = new Byte[500];
            rnd.NextBytes(b);

            var currentFile = model.ConversionInfos[outputType];

            // ------------------------------
            var splitted = b.Split(100);

            int taskResolved = 0;
            currentFile.ConvertedFile = new List<byte>();
            // why do I need to chuck? or any other way?
            foreach (var value in splitted)
            {
                try
                {
                    if (!currentFile.IsCancellationRequested)
                    {
                        var convertedChunck = await Task.Run(() => ModelConverter.ConvertChunk(value));
                        currentFile.ConvertedFile.AddRange(convertedChunck);

                        if (progress != null)
                        {
                            taskResolved++;
                            var percentage = (double)taskResolved / splitted.Count();
                            percentage *= 100;
                            var pertentageInt = (int)Math.Round(percentage);
                            progress.Report(pertentageInt);
                        }
                    }
                    else
                    {
                        progress.Report(0);
                        return;
                    }
                }
                catch (ConversionFailedException ex)
                {

                    // What to do with the ex?
                    progress.Report(0);
                    return;                
                }
            }
        }
    }
}
