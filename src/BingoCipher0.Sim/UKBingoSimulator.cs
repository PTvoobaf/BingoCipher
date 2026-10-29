using System.Text.Json;
using BingoCipher0.Core;

namespace BingoCipher0.Sim;

public sealed class UKBingoPlayer(int id, Random random)
{
    public int Id { get; } = id;
    public UKBingoStrip Strip { get; } = new(random);
}

public sealed record UKSimulationResult(IReadOnlyList<int> CalledNumbers, IReadOnlyList<UKBingoTicket> WinningTickets)
{
    public string ToJson() => JsonSerializer.Serialize(new
    {
        calledNumbers = CalledNumbers,
        winningTickets = WinningTickets.Select(ticket => ticket.Values)
    });
}

public sealed class UKBingoSimulator
{
    public UKSimulationResult Run(long seed, int playerCount)
    {
        if (playerCount < 1) throw new ArgumentOutOfRangeException(nameof(playerCount));
        var (random, playerRandoms) = SeededRandomStreams.Create(seed, playerCount);
        var players = playerRandoms.Select((source, id) => new UKBingoPlayer(id, source)).ToList();
        var caller = new NumberCaller(1, 90, random);

        while (true)
        {
            var number = caller.DrawNext();
            foreach (var player in players) player.Strip.MarkNumber(number);
            // Preserve ascending player ID and, within each strip, ticket order. Challenge
            // generation relies on this stable order when selecting among simultaneous winners.
            var winners = players.SelectMany(player => player.Strip.Tickets)
                .Where(ticket => ticket.IsLineWinner())
                .Select(ticket => new UKBingoTicket(ticket.Values))
                .ToList();
            if (winners.Count > 0) return new UKSimulationResult(caller.CalledNumbers.ToArray(), winners);
        }
    }
}
