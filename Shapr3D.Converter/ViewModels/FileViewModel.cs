using Shapr3D.Converter.Enums;
using Shapr3D.Converter.Extensions;
using Shapr3D.Converter.Models;
using Sharp3D.Converter.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Shapr3D.Converter.ViewModels
{
    public interface IFileViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Required properties for each file.
        /// </summary>
        Guid Id { get; }
        string FileSizeFormatted { get; }
        bool IsConverting { get; }
        string Name { get; }
        string OriginalPath { get; }

        /// <summary>
        /// Conversion information for each file type.
        /// </summary>
        ConversionInfo ObjConversionInfo { get; }
        ConversionInfo StepConversionInfo { get; }
        ConversionInfo StlConversionInfo { get; }
        Dictionary<ConverterOutputType, ConversionInfo> ConversionInfos { get; }

        /// <summary>
        /// Occurs when this view model has updated any properties.
        /// </summary>
        event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Cancel specific running conversion.
        /// </summary>
        /// <param name="type"></param>
        void CancelConversion(ConverterOutputType type);

        /// <summary>
        /// Cancel all the running conversions.
        /// </summary>
        void CancelConversions();

        /// <summary>
        /// Reset the selected file type's property to default valuies as
        ///     Progress = 0;
        ///     IsCancellingAvailable = true;
        ///     State = ConversionState.NotStarted;
        /// </summary>
        /// <param name="type"></param>
        void ResetProperties(ConverterOutputType type);

        /// <summary>
        /// Prepare the model to interact with database
        /// </summary>
        /// <returns></returns>
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

            _fileSizeFormatted = fileSize;
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
        public bool IsConverting => ConversionInfos.Any(state => state.Value.State == ConversionState.Converting);

        /* ============================================
         * Public methods
         * ============================================ */
        public void CancelConversion(ConverterOutputType type)
        {
            if (ConversionInfos[type].State == ConversionState.Converting)
            {
                ConversionInfos[type].CancellationTokenSource.Cancel();
                ConversionInfos[type].IsCancellingAvailable = false;
            }
        }

        public void ResetProperties(ConverterOutputType type)
        {
            ConversionInfos[type].Progress = 0;
            ConversionInfos[type].IsCancellingAvailable = true;
            ConversionInfos[type].State = ConversionState.NotStarted;
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
                if (ConversionInfos[type].State == ConversionState.Converted)
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
