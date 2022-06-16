using Shapr3D.Converter.Enums;
using Shapr3D.Converter.Infrastructure;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Shapr3D.Converter.ViewModels.Interfaces
{
    public interface IMainViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Represents the selected file as view model.
        /// </summary>
        FileViewModel SelectedFile { get; set; }
        ObservableCollection<FileViewModel> Files { get; }

        /// <summary>
        /// Preventing that the user accidentally opens conversion actions twice (fast double click on Add/Delete/Close/Save could have opened dialog twice).
        /// </summary>
        bool AreActionFieldsEnabled { get; set; }

        /// <summary>
        /// Occurs when this view model has updated the selected file.
        /// </summary>
        event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Add new file into the file collection.
        /// </summary>
        void Add();

        /// <summary>
        /// Called before the project is started.
        /// </summary>
        /// <returns>Context representing the asynchronous operation.</returns>
        Task InitAsync();

        /// <summary>
        /// ICommand implementations for user interface.
        /// </summary>
        RelayCommand AddCommand { get; }
        RelayCommand CloseDetailsCommand { get; }
        RelayCommand<ConverterOutputType> ConvertActionCommand { get; }
        RelayCommand DeleteAllCommand { get; }
    }
}
