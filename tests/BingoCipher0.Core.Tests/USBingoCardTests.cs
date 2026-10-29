using System.Text.Json;
using BingoCipher0.Core;

namespace BingoCipher0.Core.Tests;

public sealed class USBingoCardTests
{
    private static readonly int[] ValidCard =
    [
        1, 16, 31, 46, 61,
        2, 17, 32, 47, 62,
        3, 18, 0, 48, 63,
        4, 19, 34, 49, 64,
        5, 20, 35, 50, 65
    ];
    private static readonly int[] ValidCardAlternate =
    [
        14, 29, 44, 59, 74,
        15, 30, 45, 60, 75,
        13, 28, 0, 58, 73,
        11, 26, 42, 56, 71,
        12, 27, 43, 57, 72
    ];

    [Fact]
    public void SeededConstructorCreatesValidCard()
    {
        var card = new USBingoCard(new Random(42));

        Assert.Equal(USBingoCard.CellCount, card.Values.Count);
        Assert.Equal(0, card.Values[USBingoCard.FreeSpaceIndex]);
        Assert.Equal(24, card.Values.Count(value => value > 0));
        Assert.Equal(24, card.Values.Where(value => value > 0).Distinct().Count());
        Assert.False(card.IsWinner());
    }

    [Fact]
    public void ConstructorAndInitializeAcceptValidCard()
    {
        var card = new USBingoCard(new Random(42));

        card.Initialize(ValidCard);

        Assert.Equal(ValidCard, card.Values);

        var alternateCard = new USBingoCard(ValidCardAlternate);

        Assert.Equal(ValidCardAlternate, alternateCard.Values);
    }

    [Theory]
    [MemberData(nameof(InvalidCards))]
    public void ConstructorRejectsInvalidCard(int[] values)
    {
        Assert.Throws<ArgumentException>(() => new USBingoCard(values));
    }

    public static IEnumerable<object[]> InvalidCards()
    {
        // The card does not contain exactly 25 values.
        yield return [ValidCard.Take(24).ToArray()];
        // The center free-space coordinate must contain zero.
        yield return [WithValues((12, 33))];
        // The first-column value 16 is outside the valid range of 1 through 15.
        yield return [WithValues((0, 16))];
        // The second-column value 15 is outside the valid range of 16 through 30.
        yield return [WithValuesAlternate((6, 15))];
        // The middle-column value 15 is outside the valid range of 31 through 45.
        yield return [WithValuesAlternate((17, 15))];
        // The fourth-column value 15 is outside the valid range of 46 through 60.
        yield return [WithValuesAlternate((23, 15))];
        // The rightmost-column value 55 is outside the valid range of 61 through 75.
        yield return [WithValuesAlternate((19, 55))];
        // The number 1 appears twice on the card.
        yield return [WithValues((0, 1), (5, 1))];
        // A non-center coordinate contains zero, but only the center may be zero.
        yield return [WithValues((12, 0), (13, 0))];
    }

    private static int[] WithValues(params (int Index, int Value)[] replacements)
    {
        var values = ValidCard.ToArray();
        foreach (var (index, value) in replacements)
        {
            values[index] = value;
        }

        return values;
    }

    private static int[] WithValuesAlternate(params (int Index, int Value)[] replacements)
    {
        var values = ValidCardAlternate.ToArray();
        foreach (var (index, value) in replacements)
        {
            values[index] = value;
        }

        return values;
    }

    [Fact]
    public void MarkNumberMarksExistingNumberAndIsIdempotent()
    {
        var card = new USBingoCard(ValidCard);

        Assert.True(card.MarkNumber(31));
        Assert.Equal(-31, card.Values[2]);
        Assert.False(card.MarkNumber(31));
        Assert.False(card.MarkNumber(0));
        Assert.False(card.MarkNumber(76));
    }

    [Fact]
    public void WinnerDetectionRecognizesRowsColumnsAndBothDiagonals()
    {
        foreach (var indices in new[]
        {
            new[] { 0, 1, 2, 3, 4 },
            new[] { 0, 5, 10, 15, 20 },
            new[] { 0, 6, 12, 18, 24 },
            new[] { 4, 8, 12, 16, 20 }
        })
        {
            var card = new USBingoCard(ValidCard);
            foreach (var index in indices.Where(index => index != USBingoCard.FreeSpaceIndex))
            {
                card.MarkNumber(Math.Abs(card.Values[index]));
            }

            Assert.True(card.IsWinner());
        }
    }

    [Fact]
    public void CenterFreeSpaceCompletesAWinningLineWithoutPlaintextMarking()
    {
        var card = new USBingoCard(ValidCard);
        foreach (var index in new[] { 10, 11, 13, 14 })
        {
            card.MarkNumber(Math.Abs(card.Values[index]));
        }

        Assert.Equal(0, card.Values[USBingoCard.FreeSpaceIndex]);
        Assert.True(card.IsWinner());
    }

    [Fact]
    public void JsonRoundTripPreservesValuesAndMarks()
    {
        var card = new USBingoCard(ValidCard);
        card.MarkNumber(31);

        var roundTripped = USBingoCard.FromJson(card.ToJson());

        Assert.Equal(card.Values, roundTripped.Values);
        Assert.Equal(card.ToJson(), roundTripped.ToJson());
    }

    [Fact]
    public void ToStringFormatsHeaderAndFiveRowsOfFourCharacterFields()
    {
        var card = new USBingoCard(ValidCard);

        var lines = card.ToString().Split(Environment.NewLine);

        Assert.Equal(7, lines.Length);
        Assert.Equal("   B   I   N   G   O", lines[0]);
        Assert.Equal("   1  16  31  46  61", lines[1]);
        Assert.Equal("   3  18   0  48  63", lines[3]);
        Assert.Equal(string.Empty, lines[6]);
    }

    [Fact]
    public void JsonRejectsInvalidCardArray()
    {
        Assert.Throws<JsonException>(() => USBingoCard.FromJson("null"));
        Assert.Throws<JsonException>(() => USBingoCard.FromJson(JsonSerializer.Serialize(WithValues((12, 33)))));
    }
}
