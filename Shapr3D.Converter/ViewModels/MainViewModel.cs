using Converter;
using Shapr3D.Converter.Datasource;
using Shapr3D.Converter.Enums;
using Shapr3D.Converter.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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

        private async void ConvertAction(ConverterOutputType type)
        {
            var state = SelectedFile.ConversionInfos[type];
            switch (state.State)
            {
                case ConversionInfo.ConversionState.NotStarted:
                    await ConvertFile(SelectedFile, type);
                    break;
                case ConversionInfo.ConversionState.Converting:
                    SelectedFile.CancelConversion(type);
                    break;
                case ConversionInfo.ConversionState.Converted:
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

            state.State = ConversionInfo.ConversionState.Converting;

            try
            {
                await Convert(model, progress, type);
                state.State = ConversionInfo.ConversionState.Converted;

                await _persistedStore.AddOrUpdateAsync(model.ToModelEntity());
            }
            catch (TaskCanceledException)
            {
                state.Progress = 0;
                state.State = ConversionInfo.ConversionState.NotStarted;
            }
        }

        private async void Save(FileViewModel model, ConverterOutputType type)
        {
            var savePicker = new FileSavePicker();
            savePicker.FileTypeChoices.Add
                                  (string.Format("{0} file", type.ToString().ToLower()), new List<string>() { string.Format(".{0}", type.ToString().ToLower()) });


            savePicker.SuggestedFileName = Path.GetFileNameWithoutExtension(model.OriginalPath);

            StorageFile savedFile = await savePicker.PickSaveFileAsync();
            // TODO https://docs.microsoft.com/en-us/windows/uwp/files/
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

        private async Task Convert(FileViewModel model, IProgress<int> progress, ConverterOutputType outputType)
        {
            // TODO
            var converted = ModelConverter.ConvertChunk(new byte[0]);
            await Task.Delay(2000);
        }
    }
}
