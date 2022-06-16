using Shapr3D.Converter.Enums;
using Shapr3D.Converter.Models;
using Sharp3D.Converter.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Shapr3D.Converter.ViewModels.Interfaces
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
}
