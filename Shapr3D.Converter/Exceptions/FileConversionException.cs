using System;
using System.Runtime.Serialization;

namespace Shapr3D.Converter.Extensions
{
    [Serializable]
    public class FileConversionException : Exception
    {
        public FileConversionException()
        {
        }

        public FileConversionException(string message) : base(message)
        {
        }

        public FileConversionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FileConversionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}