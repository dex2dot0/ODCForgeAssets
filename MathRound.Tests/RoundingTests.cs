using Xunit;
using MathRound;

namespace MathRound.Tests
{
    public class RoundingTests
    {
        private readonly IRounding _rounding;

        public RoundingTests()
        {
            _rounding = new Rounding();
        }

        #region Decimal Tests

        [Fact]
        public void RoundWithModeAndPlaces_Decimal_ToEven_WithDecimalPlaces()
        {
            // Arrange
            decimal number = 2.5m;
            int? roundingMode = (int)MidpointRounding.ToEven;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(2m, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Decimal_AwayFromZero_WithDecimalPlaces()
        {
            // Arrange
            decimal number = 2.5m;
            int? roundingMode = (int)MidpointRounding.AwayFromZero;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(3m, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Decimal_ToEven_WithoutDecimalPlaces()
        {
            // Arrange
            decimal number = 3.5m;
            int? roundingMode = (int)MidpointRounding.ToEven;
            int? decimalPlaces = null;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(4m, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Decimal_AwayFromZero_WithoutDecimalPlaces()
        {
            // Arrange
            decimal number = 3.5m;
            int? roundingMode = (int)MidpointRounding.AwayFromZero;
            int? decimalPlaces = null;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(4m, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Decimal_NullMode_DefaultsToToEven()
        {
            // Arrange
            decimal number = 2.5m;
            int? roundingMode = null;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(2m, result); // ToEven rounds 2.5 to 2
        }

        [Fact]
        public void RoundWithModeAndPlaces_Decimal_NullModeAndPlaces_DefaultsToToEven()
        {
            // Arrange
            decimal number = 3.5m;
            int? roundingMode = null;
            int? decimalPlaces = null;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(4m, result); // ToEven rounds 3.5 to 4
        }

        [Fact]
        public void RoundWithModeAndPlaces_Decimal_ToEven_TwoDecimalPlaces()
        {
            // Arrange
            decimal number = 2.345m;
            int? roundingMode = (int)MidpointRounding.ToEven;
            int? decimalPlaces = 2;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(2.34m, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Decimal_AwayFromZero_TwoDecimalPlaces()
        {
            // Arrange
            decimal number = 2.345m;
            int? roundingMode = (int)MidpointRounding.AwayFromZero;
            int? decimalPlaces = 2;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(2.35m, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Decimal_NegativeNumber_ToEven()
        {
            // Arrange
            decimal number = -2.5m;
            int? roundingMode = (int)MidpointRounding.ToEven;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(-2m, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Decimal_NegativeNumber_AwayFromZero()
        {
            // Arrange
            decimal number = -2.5m;
            int? roundingMode = (int)MidpointRounding.AwayFromZero;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(-3m, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Decimal_ToZero_WithDecimalPlaces()
        {
            // Arrange
            decimal number = 2.7m;
            int? roundingMode = (int)MidpointRounding.ToZero;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(2m, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Decimal_ToZero_NegativeNumber()
        {
            // Arrange
            decimal number = -2.7m;
            int? roundingMode = (int)MidpointRounding.ToZero;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(-2m, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Decimal_ToZero_TwoDecimalPlaces()
        {
            // Arrange
            decimal number = 2.567m;
            int? roundingMode = (int)MidpointRounding.ToZero;
            int? decimalPlaces = 2;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(2.56m, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Decimal_ToNegativeInfinity_WithDecimalPlaces()
        {
            // Arrange
            decimal number = 2.3m;
            int? roundingMode = (int)MidpointRounding.ToNegativeInfinity;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(2m, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Decimal_ToNegativeInfinity_NegativeNumber()
        {
            // Arrange
            decimal number = -2.3m;
            int? roundingMode = (int)MidpointRounding.ToNegativeInfinity;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(-3m, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Decimal_ToNegativeInfinity_TwoDecimalPlaces()
        {
            // Arrange
            decimal number = 2.567m;
            int? roundingMode = (int)MidpointRounding.ToNegativeInfinity;
            int? decimalPlaces = 2;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(2.56m, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Decimal_ToPositiveInfinity_WithDecimalPlaces()
        {
            // Arrange
            decimal number = 2.3m;
            int? roundingMode = (int)MidpointRounding.ToPositiveInfinity;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(3m, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Decimal_ToPositiveInfinity_NegativeNumber()
        {
            // Arrange
            decimal number = -2.7m;
            int? roundingMode = (int)MidpointRounding.ToPositiveInfinity;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(-2m, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Decimal_ToPositiveInfinity_TwoDecimalPlaces()
        {
            // Arrange
            decimal number = 2.561m;
            int? roundingMode = (int)MidpointRounding.ToPositiveInfinity;
            int? decimalPlaces = 2;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(2.57m, result);
        }

        #endregion

        #region Double Tests

        [Fact]
        public void RoundWithModeAndPlaces_Double_ToEven_WithDecimalPlaces()
        {
            // Arrange
            double number = 2.5;
            int? roundingMode = (int)MidpointRounding.ToEven;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(2.0, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Double_AwayFromZero_WithDecimalPlaces()
        {
            // Arrange
            double number = 2.5;
            int? roundingMode = (int)MidpointRounding.AwayFromZero;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(3.0, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Double_ToEven_WithoutDecimalPlaces()
        {
            // Arrange
            double number = 3.5;
            int? roundingMode = (int)MidpointRounding.ToEven;
            int? decimalPlaces = null;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(4.0, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Double_AwayFromZero_WithoutDecimalPlaces()
        {
            // Arrange
            double number = 3.5;
            int? roundingMode = (int)MidpointRounding.AwayFromZero;
            int? decimalPlaces = null;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(4.0, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Double_NullMode_DefaultsToToEven()
        {
            // Arrange
            double number = 2.5;
            int? roundingMode = null;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(2.0, result); // ToEven rounds 2.5 to 2
        }

        [Fact]
        public void RoundWithModeAndPlaces_Double_NullModeAndPlaces_DefaultsToToEven()
        {
            // Arrange
            double number = 3.5;
            int? roundingMode = null;
            int? decimalPlaces = null;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(4.0, result); // ToEven rounds 3.5 to 4
        }

        [Fact]
        public void RoundWithModeAndPlaces_Double_ToEven_TwoDecimalPlaces()
        {
            // Arrange
            double number = 2.335;
            int? roundingMode = (int)MidpointRounding.ToEven;
            int? decimalPlaces = 2;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(2.34, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Double_AwayFromZero_TwoDecimalPlaces()
        {
            // Arrange
            double number = 2.335;
            int? roundingMode = (int)MidpointRounding.AwayFromZero;
            int? decimalPlaces = 2;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(2.34, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Double_NegativeNumber_ToEven()
        {
            // Arrange
            double number = -2.5;
            int? roundingMode = (int)MidpointRounding.ToEven;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(-2.0, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Double_NegativeNumber_AwayFromZero()
        {
            // Arrange
            double number = -2.5;
            int? roundingMode = (int)MidpointRounding.AwayFromZero;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(-3.0, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Double_ToZero_WithDecimalPlaces()
        {
            // Arrange
            double number = 2.7;
            int? roundingMode = (int)MidpointRounding.ToZero;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(2.0, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Double_ToZero_NegativeNumber()
        {
            // Arrange
            double number = -2.7;
            int? roundingMode = (int)MidpointRounding.ToZero;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(-2.0, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Double_ToZero_TwoDecimalPlaces()
        {
            // Arrange
            double number = 2.567;
            int? roundingMode = (int)MidpointRounding.ToZero;
            int? decimalPlaces = 2;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(2.56, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Double_ToNegativeInfinity_WithDecimalPlaces()
        {
            // Arrange
            double number = 2.3;
            int? roundingMode = (int)MidpointRounding.ToNegativeInfinity;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(2.0, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Double_ToNegativeInfinity_NegativeNumber()
        {
            // Arrange
            double number = -2.3;
            int? roundingMode = (int)MidpointRounding.ToNegativeInfinity;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(-3.0, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Double_ToNegativeInfinity_TwoDecimalPlaces()
        {
            // Arrange
            double number = 2.567;
            int? roundingMode = (int)MidpointRounding.ToNegativeInfinity;
            int? decimalPlaces = 2;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(2.56, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Double_ToPositiveInfinity_WithDecimalPlaces()
        {
            // Arrange
            double number = 2.3;
            int? roundingMode = (int)MidpointRounding.ToPositiveInfinity;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(3.0, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Double_ToPositiveInfinity_NegativeNumber()
        {
            // Arrange
            double number = -2.7;
            int? roundingMode = (int)MidpointRounding.ToPositiveInfinity;
            int? decimalPlaces = 0;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(-2.0, result);
        }

        [Fact]
        public void RoundWithModeAndPlaces_Double_ToPositiveInfinity_TwoDecimalPlaces()
        {
            // Arrange
            double number = 2.561;
            int? roundingMode = (int)MidpointRounding.ToPositiveInfinity;
            int? decimalPlaces = 2;

            // Act
            var result = _rounding.RoundWithModeAndPlaces(number, roundingMode, decimalPlaces);

            // Assert
            Assert.Equal(2.57, result);
        }

        #endregion
    }
}

