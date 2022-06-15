using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shapr3D.Converter.Ui.ValueConverters;
using System;
using Windows.UI.Xaml;

namespace Shapr3D.Converter.Ui.UnitTests.ValueConverters
{
    [TestClass]
    public class EnumVisibilityConverterTests
    {
        public enum TestEnum 
        {
            Option1,
        }

        [TestMethod]
        [DataRow(null, "Option1", Visibility.Collapsed)]
        [DataRow(TestEnum.Option1, null, Visibility.Collapsed)]
        [DataRow(TestEnum.Option1, "NotFound", Visibility.Collapsed)]
        [DataRow(TestEnum.Option1, "Option 1", Visibility.Collapsed)]
        [DataRow(TestEnum.Option1, "Option1", Visibility.Visible)]
        public void WhenConvertIsCalled_ThenCorrectIsReturned(object obj, object parameter, Visibility expectedResult)
        {
            // Given
            var sut = new EnumVisibilityConverter();

            // When
            var result = sut.Convert(obj, It.IsAny<Type>(), parameter, It.IsAny<string>());

            // Then
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void WhenConvertBackIsCalled_ThenThrowsNotImplementedException()
        {
            // Given
            var sut = new EnumVisibilityConverter();

            // When && Then
            Assert.ThrowsException<NotImplementedException>(() => _ = sut.ConvertBack(It.IsAny<object>(), It.IsAny<Type>(), It.IsAny<object>(), It.IsAny<string>()));
        }
    }
}
