using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shapr3D.Converter.Ui.ValueConverters;
using System;

namespace Shapr3D.Converter.Ui.UnitTests.ValueConverters
{
    [TestClass]
    public class NotNullBooleanConverterTests
    {
        [TestMethod]
        [DataRow(null, false)]
        [DataRow(1, true)]
        public void WhenConvertIsCalled_ThenCorrectIsReturned(int? obj, bool expectedResult)
        {
            // Given
            var sut = new NotNullBooleanConverter();

            // When
            var result = sut.Convert(obj, It.IsAny<Type>(), It.IsAny<object>(), It.IsAny<string>());

            // Then
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void WhenConvertBackIsCalled_ThenThrowsNotSzpportedException()
        {
            // Given
            var sut = new NotNullBooleanConverter();

            // When && Then
            Assert.ThrowsException<NotSupportedException>(() => _ = sut.ConvertBack(It.IsAny<object>(), It.IsAny<Type>(), It.IsAny<object>(), It.IsAny<string>()));
        }
    }
}
