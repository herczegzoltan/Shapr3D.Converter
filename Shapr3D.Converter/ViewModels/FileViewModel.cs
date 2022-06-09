using Shapr3D.Converter.Datasource;
using Shapr3D.Converter.Enums;
using Shapr3D.Converter.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Shapr3D.Converter.ViewModels
{
    public class ConversionInfo : INotifyPropertyChanged
    {
        public enum ConversionState
        {
            NotStarted,
            Converting,
            Converted
        }

        private ConversionState state;
        private int progress;

        public ConversionInfo(bool isConverted)
        {
            State = isConverted ? ConversionState.Converted : ConversionState.NotStarted;
            if (isConverted)
            {
                Progress = 100;
            }
        }

        public ConversionState State
        {
            get => state;
            set
            {
                if (state != value)
                {
                    state = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
                }
            }
        }

        public int Progress
        {
            get => progress;
            set
            {
                if (progress != value)
                {
                    progress = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public interface IFileViewModel : INotifyPropertyChanged
    {
        Dictionary<ConverterOutputType, ConversionInfo> ConversionInfos { get; }
        string FileSizeFormatted { get; }
        Guid Id { get; }
        bool IsConverting { get; }
        string Name { get; }
        ConversionInfo ObjConversionInfo { get; }
        string OriginalPath { get; }
        ConversionInfo StepConversionInfo { get; }
        ConversionInfo StlConversionInfo { get; }

        event PropertyChangedEventHandler PropertyChanged;

        void CancelConversion(ConverterOutputType type);
        void CancelConversions();
        ModelEntity ToModelEntity();
    }

    public class FileViewModel : IFileViewModel
    {
        // Getter/setter backup fields
        private readonly ulong _fileSizeFormatted;

        public FileViewModel(Guid id, string originalPath, ConverterOutputTypeFlags converterOutputTypes, ulong fileSize)
        {
            Id = id;
            OriginalPath = originalPath;

            foreach (ConverterOutputType type in Enum.GetValues(typeof(ConverterOutputType)))
            {
                ConversionInfos.Add(type, new ConversionInfo(converterOutputTypes.HasFlag(type.ToFlag())));
            }

            foreach (var (type, state) in ConversionInfos)
            {
                state.PropertyChanged += OnConvertingStatePropertyChanged;
            }

            var nameLower = Path.GetFileNameWithoutExtension(originalPath).ToLower().Replace("_", " ");
            Name = nameLower.Substring(0, 1).ToUpper() + nameLower.Substring(1);

            this._fileSizeFormatted = fileSize;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /* ============================================
        * Public properties
        * ============================================ */
        public Guid Id { get; }
        public string OriginalPath { get; }
        public string Name { get; }

        public Dictionary<ConverterOutputType, ConversionInfo> ConversionInfos { get; } = new Dictionary<ConverterOutputType, ConversionInfo>();
        public ConversionInfo ObjConversionInfo => ConversionInfos[ConverterOutputType.Obj];
        public ConversionInfo StepConversionInfo => ConversionInfos[ConverterOutputType.Step];
        public ConversionInfo StlConversionInfo => ConversionInfos[ConverterOutputType.Stl];
        public bool IsConverting => ConversionInfos.Any(state => state.Value.State == ConversionInfo.ConversionState.Converting);

        /* ============================================
         * Public methods
         * ============================================ */
        public void CancelConversion(ConverterOutputType type)
        {
            // TODO
        }

        public void CancelConversions()
        {
            foreach (var convertingType in ConversionInfos.Keys)
            {
                CancelConversion(convertingType);
            }
        }

        public string FileSizeFormatted
        {
            get
            {
                return string.Format("{0} megabytes", ((double)_fileSizeFormatted / 1024 / 1024).ToString("0.00"));
            }
        }

        public ModelEntity ToModelEntity()
        {
            ConverterOutputTypeFlags convertedTypes = ConverterOutputTypeFlags.None;
            foreach (ConverterOutputType type in Enum.GetValues(typeof(ConverterOutputType)))
            {
                if (ConversionInfos[type].State == ConversionInfo.ConversionState.Converted)
                {
                    convertedTypes |= type.ToFlag();
                }
            }

            return new ModelEntity()
            {
                Id = Id,
                ConvertedTypes = convertedTypes,
                OriginalPath = OriginalPath,
                FileSize = _fileSizeFormatted,
            };
        }


        /* ============================================
        * Private Methods
        * ============================================ */

        private void OnConvertingStatePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ConversionInfo.State))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConverting)));
            }
        }

    }
}
