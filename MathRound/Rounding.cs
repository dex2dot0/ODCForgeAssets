using OutSystems.ExternalLibraries.SDK;

namespace MathRound
{
    /// <summary>
    /// The Rounding class exposes several rounding methods for decimal and double types.
    /// </summary>
    public class Rounding: IRounding { }

    [OSInterface(Description = "Exposes several rounding methods", IconResourceName = "MathRound.resources.RoundIcon.png", Name="MathRounding")]
    public interface IRounding
    {
        [OSAction(Description = "Rounds a decimal number using the specified rounding mode and decimal places", ReturnDescription = "The rounded decimal number", ReturnName = "RoundedValue")]
        public decimal RoundDecimalWithMode(
            [OSParameter(Description = "The decimal number to round")] decimal numberToRound, 
            [OSParameter(Description = "The rounding mode (0 = ToEven, 1 = AwayFromZero, 2 = ToZero, 3 = ToNegativeInfinity, 4 = ToPositiveInfinity). If not specified, defaults to ToEven(0).")] int? roundingMode = 0, 
            [OSParameter(Description = "The number of decimal places to round to. If not specified, rounds to the nearest integer. If not specified, defaults to 0")] int? decimalPlaces = 0)
        {
            var mode = roundingMode.HasValue ? (MidpointRounding)roundingMode.Value : MidpointRounding.ToEven;
            return Math.Round(numberToRound, decimalPlaces.Value, mode);
        }

        [OSAction(Description = "Rounds a double-precision number using the specified rounding mode and decimal places", ReturnDescription = "The rounded double-precision number", ReturnName = "RoundedValue")]
        public double RoundDoubleWithMode(
            [OSParameter(Description = "The double-precision number to round")] double numberToRound, 
            [OSParameter(Description = "The rounding mode (0 = ToEven, 1 = AwayFromZero, 2 = ToZero, 3 = ToNegativeInfinity, 4 = ToPositiveInfinity). If not specified, defaults to ToEven(0).")] int? roundingMode = 0,
            [OSParameter(Description = "The number of decimal places to round to. If not specified, rounds to the nearest integer. If not specified, defaults to 0")] int? decimalPlaces = 0)
        {
            var mode = roundingMode.HasValue ? (MidpointRounding)roundingMode.Value : MidpointRounding.ToEven;
            return Math.Round(numberToRound, decimalPlaces.Value, mode);
        }

        [OSAction(Description = "Returns the smallest integer greater than or equal to the specified decimal number", ReturnDescription = "The smallest integer greater than or equal to the specified decimal number", ReturnName = "CeilingValue")]
        public decimal DecimalCeiling(
            [OSParameter(Description = "The decimal number to round up")] decimal numberToRound)
        {
            return Math.Ceiling(numberToRound);
        }

        [OSAction(Description = "Returns the smallest integer greater than or equal to the specified double-precision number", ReturnDescription = "The smallest integer greater than or equal to the specified double-precision number", ReturnName = "CeilingValue")]
        public double DoubleCeiling(
            [OSParameter(Description = "The double-precision number to round up")] double numberToRound)
        {
            return Math.Ceiling(numberToRound);
        }

        [OSAction(Description = "Returns the largest integer less than or equal to the specified decimal number", ReturnDescription = "The largest integer less than or equal to the specified decimal number", ReturnName = "FloorValue")]
        public decimal DecimalFloor(
            [OSParameter(Description = "The decimal number to round down")] decimal numberToRound)
        {
            return Math.Floor(numberToRound);
        }

        [OSAction(Description = "Returns the largest integer less than or equal to the specified double-precision number", ReturnDescription = "The largest integer less than or equal to the specified double-precision number", ReturnName = "FloorValue")]
        public double DoubleFloor(
            [OSParameter(Description = "The double-precision number to round down")] double numberToRound)
        {
            return Math.Floor(numberToRound);
        }
    }
}
