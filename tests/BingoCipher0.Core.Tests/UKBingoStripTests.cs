using System.Text.Json;
using BingoCipher0.Core;

namespace BingoCipher0.Core.Tests;

public sealed class UKBingoStripTests
{
    [Fact]
    public void SeededConstructorCreatesSixValidTicketsCoveringEveryNumber()
    {
        var strip = new UKBingoStrip(new Random(42));

        Assert.Equal(UKBingoStrip.TicketCount, strip.Tickets.Count);
        Assert.All(strip.Tickets, ticket =>
        {
            Assert.All(Enumerable.Range(0, UKBingoTicket.RowCount), row =>
                Assert.Equal(5, ticket.Values.Skip(row * UKBingoTicket.ColumnCount)
                    .Take(UKBingoTicket.ColumnCount)
                    .Count(value => value != 0)));
            Assert.Equal(15, ticket.Values.Count(value => value != 0));
        });

        var numbers = strip.Tickets.SelectMany(ticket => ticket.Values).Where(value => value != 0).OrderBy(Math.Abs);
        Assert.Equal(Enumerable.Range(1, 90), numbers.Select(Math.Abs));
    }

    [Fact]
    public void SeedProducesReproducibleJson()
    {
        var first = new UKBingoStrip(new Random(42));
        var second = new UKBingoStrip(new Random(42));

        Assert.Equal(first.ToJson(), second.ToJson());
    }

    [Fact]
    public void MarkNumberMarksTheNumberOnItsTicket()
    {
        var strip = new UKBingoStrip(new Random(42));

        Assert.True(strip.MarkNumber(42));
        Assert.Contains(-42, strip.Tickets.SelectMany(ticket => ticket.Values));
        Assert.False(strip.MarkNumber(42));
        Assert.False(strip.MarkNumber(91));
    }

    [Fact]
    public void MarkingEveryNumberSucceedsAndLeavesNoPositiveValues()
    {
        var strip = new UKBingoStrip(new Random(31337));

        foreach (var number in Enumerable.Range(1, 90))
        {
            Assert.True(strip.MarkNumber(number));
        }

        Assert.All(strip.Tickets, ticket =>
            Assert.DoesNotContain(ticket.Values, value => value > 0));
    }

    [Fact]
    public void JsonRoundTripPreservesStripAndMarks()
    {
        var strip = new UKBingoStrip(new Random(42));
        strip.MarkNumber(42);

        var roundTripped = UKBingoStrip.FromJson(strip.ToJson());

        Assert.Equal(strip.ToJson(), roundTripped.ToJson());
    }

    [Fact]
    public void JsonRejectsWrongNumberOfTickets()
    {
        var json = JsonSerializer.Serialize(new[] { new int[UKBingoTicket.CellCount] });

        Assert.Throws<JsonException>(() => UKBingoStrip.FromJson(json));
    }

    [Fact]
    public void ToStringFormatsSixTicketBlocksWithBlankSeparators()
    {
        var strip = new UKBingoStrip(new Random(42));

        var lines = strip.ToString().Split(Environment.NewLine);

        Assert.Equal(24, lines.Length);
        Assert.Equal(string.Empty, lines[3]);
        Assert.Equal(string.Empty, lines[7]);
        Assert.Equal(string.Empty, lines[11]);
        Assert.Equal(string.Empty, lines[15]);
        Assert.Equal(string.Empty, lines[19]);
        Assert.Equal(string.Empty, lines[23]);
    }

    [Fact]
    public void SeededGenerationProducesMoreThanTheFormerFixedColumnProfile()
    {
        var profiles = Enumerable.Range(0, 100)
            .SelectMany(seed => new UKBingoStrip(new Random(seed)).Tickets)
            .Select(ticket => Enumerable.Range(0, UKBingoTicket.ColumnCount)
                .Select(column => ticket.Values.Select((value, index) => (value, index))
                    .Count(cell => cell.index % UKBingoTicket.ColumnCount == column && cell.value != 0))
                .OrderBy(count => count)
                .ToArray())
            .Select(profile => string.Join(",", profile))
            .ToHashSet();

        Assert.Contains("1,1,1,2,2,2,2,2,2", profiles);
        Assert.Contains("1,1,1,1,2,2,2,2,3", profiles);
        Assert.Contains("1,1,1,1,1,2,2,3,3", profiles);
        Assert.Contains("1,1,1,1,1,1,3,3,3", profiles);
    }
}
