using Converter;
using Shapr3D.Converter.Datasource;
using Shapr3D.Converter.Enums;
using Shapr3D.Converter.Infrastructure;
using Sharp3D.Converter.Ui.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

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
        private IPersistedStore _persistedStore;
        // Getter/setter backup fields
        private FileViewModel _fileViewModel;
        private const Int32 ErrorAccessDenied = unchecked((Int32)0x80070005);
        const Int32 ErrorSharingViolation = unchecked((Int32)0x80070020);


        public MainViewModel(IDialogService dialogService, IPersistedStore persistedStore)
        {
            _dialogService = dialogService;
            _persistedStore = persistedStore;

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
            //_persistedStore = new PersistedStore();
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
                await Task.Run(() => Convert(model, progress, type));

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
            catch (Exception ex) when ((ex.HResult == ErrorAccessDenied) || (ex.HResult == ErrorSharingViolation))
            {
                // TODO
                // Do I need to add a dialog with close and try again due error of access
            }
            catch (TaskCanceledException)
            {
                // Do I need this? or just throw a task cancelled exception?
                state.Progress = 0;
                //state.IsCancellationRequested = false;
                state.State = ConversionState.NotStarted;
            }
            catch (ConversionFailedException) 
            {

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
                    catch (Exception ex) when ((ex.HResult == ErrorAccessDenied) || (ex.HResult == ErrorSharingViolation))
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
            var storageFile = await StorageFile.GetFileFromPathAsync(model.OriginalPath);
            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(storageFile);
            IBuffer buffer = await FileIO.ReadBufferAsync(storageFile);

            byte[] bytes = buffer.ToArray();

            //what if pressing cancel when creating chunks?
            //var chunks = bytes.Select((s, i) => new { s, i })
            //                      .GroupBy(x => x.i % 100)
            //                      .Select(g => g.Select(x => x.s).ToList())
            //                      .ToList();

            int i = 0;
            var chunks = from chunk in bytes
                         group chunk by i++ % 100 into part
                         select part.AsEnumerable();

            var currentFile = model.ConversionInfos[outputType];

            int taskResolved = 0;
            currentFile.ConvertedFile = new List<byte>();

            Debug.WriteLine(@"TEST3:");

            // Note: How is it possible to run paralell and the result is in ordered too.
            foreach (var item in chunks)
            {
                try
                {
                    Debug.WriteLine(@"TEST:" + item.Count());

                    if (!currentFile.IsCancellationRequested)
                    {
                        var convertedChunck = await Task.Run(() => ConvertChunk(item.ToArray()));
                        currentFile.ConvertedFile.AddRange(convertedChunck);

                        if (progress != null)
                        {
                            taskResolved++;
                            var percentage = (double)taskResolved / chunks.Count();
                            percentage *= 100;
                            var pertentageInt = (int)Math.Round(percentage);
                            await Task.Run(() => progress.Report(pertentageInt));

                            Debug.WriteLine(@"TEST:" + pertentageInt);
                        }
                        else
                        {
                            await Task.Run(() => progress.Report(0));
                            return;
                        }
                    }
                }
                catch (ConversionFailedException ex)
                {
                    // What to do with the ex?
                    await Task.Run(() => progress.Report(0));
                    currentFile.State = ConversionState.NotStarted;
                    return;
                }
                catch (Exception)
                {
                    throw;
                }
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

            //int num4 = new Random().Next(0, 1000);
            //if (num4 == 0)
            //{
            //    throw new ConversionFailedException($"Conversion failed. Error code {num4}");
            //}

            return array;
        }
    }
}
