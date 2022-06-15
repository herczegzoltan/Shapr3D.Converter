﻿using Shapr3D.Converter.Enums;
using System.ComponentModel;
using System.Threading;

namespace Shapr3D.Converter.Models
{
    public class ConversionInfo : INotifyPropertyChanged
    {
        private ConversionState _state;
        private int _progress;
        private bool _isCancellingAvailable;

        public ConversionInfo(bool isConverted)
        {
            _isCancellingAvailable = true;
            State = isConverted ? ConversionState.Converted : ConversionState.NotStarted;
            if (isConverted)
            {
                Progress = 100;
            }
        }

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

        public CancellationTokenSource CancellationTokenSource { get; set; }
        public byte[] ConvertedResult { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
