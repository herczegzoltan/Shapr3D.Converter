using Shapr3D.Converter.Enums;
using System.ComponentModel;
using System.Threading;

namespace Shapr3D.Converter.Models
{
    public class ConversionInfo : INotifyPropertyChanged
    {
        // Getter/setter backup fields
        private ConversionState _state;
        private int _progress;
        private bool _isCancellingAvailable;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversionInfo"/> class. 
        /// </summary>
        /// <param name="isConverted">Set state by passing the state of the conversion.</param>
        public ConversionInfo(bool isConverted)
        {
            _isCancellingAvailable = true;
            State = isConverted ? ConversionState.Converted : ConversionState.NotStarted;
            if (isConverted)
            {
                Progress = 100;
            }
        }

        /// <summary>
        /// Gets or sets state of the pregress.
        /// </summary>
        public ConversionState State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the file path is required or not.
        /// </summary>
        public int Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether cancelling of the progress is available..
        /// </summary>
        public bool IsCancellingAvailable
        {
            get => _isCancellingAvailable;
            set
            {
                if (_isCancellingAvailable != value)
                {
                    _isCancellingAvailable = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCancellingAvailable)));
                }
            }
        }

        /// <summary>
        /// Gets or sets the cancellation token source.
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        /// Gets or sets the converted result.
        /// </summary>
        public byte[] ConvertedResult { get; set; }

        /// <summary>
        /// Occurs for any property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
