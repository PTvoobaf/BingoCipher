using BingoCipher0.Sim;

namespace BingoCipher0.Sim.Tests;

public sealed class SimulatorTests
{
    [Fact]
    public void CallerCreatesCompletePermutationAndTracksCalls()
    {
        var caller = new NumberCaller(1, 75, new Random(42));
        Assert.Equal(Enumerable.Range(1, 75).OrderBy(value => value), caller.DrawOrder.OrderBy(value => value));
        Assert.Empty(caller.CalledNumbers);
        caller.DrawNext();
        Assert.Single(caller.CalledNumbers);
    }

    [Fact]
    public void UKSimulationIsReproducibleAndReturnsAWinningTicket()
    {
        var first = new UKBingoSimulator().Run(42, 1);
        var second = new UKBingoSimulator().Run(42, 1);
        Assert.Equal(first.ToJson(), second.ToJson());
        Assert.NotEmpty(first.CalledNumbers);
        Assert.NotEmpty(first.WinningTickets);
        Assert.All(first.WinningTickets, ticket => Assert.True(ticket.IsLineWinner()));
    }

    [Fact]
    public void USSimulationIsReproducibleAndReturnsWinningCards()
    {
        var first = new USBingoSimulator().Run(1776, 7);
        var second = new USBingoSimulator().Run(1776, 7);
        Assert.Equal(first.ToJson(), second.ToJson());
        Assert.NotEmpty(first.CalledNumbers);
        Assert.NotEmpty(first.WinningCards);
        Assert.All(first.WinningCards, card => Assert.True(card.IsWinner()));
    }

    [Fact]
    public void PlayerCountMustBePositive()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new UKBingoSimulator().Run(42, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new USBingoSimulator().Run(42, 0));
    }

    [Theory]
    [InlineData("--seed=42", "--players=1")]
    [InlineData("--seed", "42", "--players", "1")]
    [InlineData("--seed=0xdeadbeef8badf00d", "--players=1")]
    public void ParserAcceptsNamedOptions(params string[] arguments)
    {
        var options = SimulationArgumentParser.Parse(arguments);
        Assert.Equal(arguments[0].Contains("0x", StringComparison.OrdinalIgnoreCase)
            ? unchecked((long)0xdeadbeef8badf00dUL)
            : 42, options.Seed);
        Assert.Equal(1, options.Players);
    }
}
