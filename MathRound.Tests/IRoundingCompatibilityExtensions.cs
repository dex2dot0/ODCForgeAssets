using MathRound;

namespace MathRound.Tests;

internal static class IRoundingCompatibilityExtensions
{
    internal static decimal RoundWithModeAndPlaces(
        this IRounding rounding,
        decimal numberToRound,
        int? roundingMode,
        int? decimalPlaces)
    {
        return rounding.RoundDecimalWithMode(numberToRound, roundingMode, decimalPlaces);
    }

    internal static double RoundWithModeAndPlaces(
        this IRounding rounding,
        double numberToRound,
        int? roundingMode,
        int? decimalPlaces)
    {
        return rounding.RoundDoubleWithMode(numberToRound, roundingMode, decimalPlaces);
    }
}
