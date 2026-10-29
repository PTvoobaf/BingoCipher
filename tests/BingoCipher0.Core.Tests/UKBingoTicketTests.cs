using System.Text.Json;
using BingoCipher0.Core;

namespace BingoCipher0.Core.Tests;

public sealed class UKBingoTicketTests
{
    private static readonly int[] ValidTicket =
    [
        1, 0, 21, 0, 41, 0, 61, 0, 81,
        8, 12, 0, 32, 0, 52, 0, 72, 0,
        10, 0, 0, 31, 42, 0, 62, 73, 0
    ];

    [Fact]
    public void DefaultConstructorCreatesEmptyTicket()
    {
        var ticket = new UKBingoTicket();

        Assert.Equal(Enumerable.Repeat(0, 27), ticket.Values);
        Assert.False(ticket.IsLineWinner());
    }

    [Fact]
    public void InitializeAcceptsValidTicket()
    {
        var ticket = new UKBingoTicket();

        ticket.Initialize(ValidTicket);

        Assert.Equal(ValidTicket, ticket.Values);
    }

    [Theory]
    [MemberData(nameof(InvalidTickets))]
    public void ConstructorRejectsInvalidTicket(int[] values)
    {
        Assert.Throws<ArgumentException>(() => new UKBingoTicket(values));
    }

    public static IEnumerable<object[]> InvalidTickets()
    {
        // The ticket does not have five numbers in each row, and the value 1 is outside the last column's range.
        yield return [Enumerable.Repeat(0, 26).Append(1).ToArray()];
        // The first row has more than five numbers, and several values are outside their column ranges.
        yield return [WithValues((0, 1), (1, 2), (2, 3), (3, 4), (4, 5), (5, 6))];
        // The value 1 is duplicated and is outside the valid range for the second column.
        yield return [WithValues((1, 1))];
        // The number 10 appears twice on the ticket.
        yield return [WithValues((0, 10))];
        // The first row has only four numbers instead of five.
        yield return [WithValues((0, 0))];
        // The number 1 appears twice on the ticket.
        yield return [WithValues((0, 1), (9, 1))];
        // The value 83 is outside the valid range of 71 through 80 for the eighth column.
        yield return [WithValues((25, 83))];
    }

    private static int[] WithValues(params (int Index, int Value)[] replacements)
    {
        var values = ValidTicket.ToArray();
        foreach (var (index, value) in replacements)
        {
            values[index] = value;
        }

        return values;
    }

    [Fact]
    public void MarkNumberMarksExistingNumberAndIsIdempotent()
    {
        var ticket = new UKBingoTicket(ValidTicket);

        Assert.True(ticket.MarkNumber(21));
        Assert.Equal(-21, ticket.Values[2]);
        Assert.False(ticket.MarkNumber(21));
        Assert.False(ticket.MarkNumber(90));
    }

    [Fact]
    public void LineWinnerRequiresAllFiveNumbersInOneRow()
    {
        var ticket = new UKBingoTicket(ValidTicket);

        foreach (var number in new[] { 1, 21, 41, 61, 81 })
        {
            ticket.MarkNumber(number);
        }

        Assert.True(ticket.IsLineWinner());
    }

    [Fact]
    public void JsonRoundTripPreservesValuesAndMarks()
    {
        var ticket = new UKBingoTicket(ValidTicket);
        ticket.MarkNumber(21);

        var roundTripped = UKBingoTicket.FromJson(ticket.ToJson());

        Assert.Equal(ticket.Values, roundTripped.Values);
        Assert.Equal(ticket.ToJson(), roundTripped.ToJson());
    }

    [Fact]
    public void ToStringFormatsThreeRowsOfFourCharacterFields()
    {
        var ticket = new UKBingoTicket(ValidTicket);

        var lines = ticket.ToString().Split(Environment.NewLine);

        Assert.Equal(4, lines.Length);
        Assert.Equal("   1      21      41      61      81", lines[0]);
        Assert.Equal("   8  12      32      52      72    ", lines[1]);
        Assert.Equal("  10          31  42      62  73    ", lines[2]);
        Assert.Equal(string.Empty, lines[3]);
    }

    [Fact]
    public void JsonRejectsInvalidTicketArray()
    {
        Assert.Throws<ArgumentException>(() => UKBingoTicket.FromJson(JsonSerializer.Serialize(ValidTicket.Take(26))));
        Assert.Throws<JsonException>(() => UKBingoTicket.FromJson("null"));
    }
}
