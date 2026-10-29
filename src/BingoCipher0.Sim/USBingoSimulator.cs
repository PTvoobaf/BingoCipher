using System.Text.Json;
using BingoCipher0.Core;

namespace BingoCipher0.Sim;

public sealed class USBingoPlayer(int id, Random random)
{
    public int Id { get; } = id;
    public USBingoCard Card { get; } = new(random);
}

public sealed record USSimulationResult(IReadOnlyList<int> CalledNumbers, IReadOnlyList<USBingoCard> WinningCards)
{
    public string ToJson() => JsonSerializer.Serialize(new
    {
        calledNumbers = CalledNumbers,
        winningCards = WinningCards.Select(card => card.Values)
    });
}

public sealed class USBingoSimulator
{
    public USSimulationResult Run(long seed, int playerCount)
    {
        if (playerCount < 1) throw new ArgumentOutOfRangeException(nameof(playerCount));
        var (random, playerRandoms) = SeededRandomStreams.Create(seed, playerCount);
        var players = playerRandoms.Select((source, id) => new USBingoPlayer(id, source)).ToList();
        var caller = new NumberCaller(1, 75, random);

        while (true)
        {
            var number = caller.DrawNext();
            foreach (var player in players) player.Card.MarkNumber(number);
            // Preserve ascending player ID. Challenge generation relies on this stable order
            // when selecting among simultaneous winners.
            var winners = players.Where(player => player.Card.IsWinner())
                .Select(player => new USBingoCard(player.Card.Values))
                .ToList();
            if (winners.Count > 0) return new USSimulationResult(caller.CalledNumbers.ToArray(), winners);
        }
    }
}
