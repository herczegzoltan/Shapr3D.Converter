using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shapr3D.Converter.Ui.ValueConverters;
using System;
using Windows.UI.Xaml;

namespace Shapr3D.Converter.Ui.UnitTests.ValueConverters
{
    [TestClass]
    public class BooleanToVisibilityConverterTests
    {
        [TestMethod]
        [DataRow(true, Visibility.Visible)]
        [DataRow(false, Visibility.Collapsed)]
        public void WhenConvertIsCalled_ThenCorrectIsReturned(bool isVisible, Visibility expectedResult)
        {
            // Given
            var sut = new BooleanToVisibilityConverter();

            // When
            var result = sut.Convert(isVisible, It.IsAny<Type>(), It.IsAny<object>(), It.IsAny<string>());

            // Then
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        [DataRow(Visibility.Visible, true)]
        [DataRow(Visibility.Collapsed, false)]
        public void WhenConvertBackIsCalled_ThenCorrectIsReturned(Visibility visibilityStatus, bool expectedResult)
        {
            // Given
            var sut = new BooleanToVisibilityConverter();

            // When
            var result = sut.ConvertBack(visibilityStatus, It.IsAny<Type>(), It.IsAny<object>(), It.IsAny<string>());

            // Then
            Assert.AreEqual(expectedResult, result);
        }
    }
}
