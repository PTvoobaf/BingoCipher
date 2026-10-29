using System.Globalization;

namespace BingoCipher0.Sim;

public sealed record SimulationOptions(long Seed, int Players, bool Terminal);

public static class SimulationArgumentParser
{
    public static SimulationOptions Parse(IReadOnlyList<string> arguments)
    {
        long? seed = null;
        int? players = null;
        var terminal = false;

        for (var index = 0; index < arguments.Count; index++)
        {
            var argument = arguments[index];
            if (argument == "--terminal") { terminal = true; continue; }
            var separator = argument.IndexOf('=');
            var name = separator < 0 ? argument : argument[..separator];
            string value;
            if (separator >= 0) value = argument[(separator + 1)..];
            else if (index + 1 < arguments.Count) value = arguments[++index];
            else throw new ArgumentException($"Missing value for {name}.");

            if (name == "--seed" && TryParseSeed(value, out var parsedSeed)) seed = parsedSeed;
            else if (name == "--players" && int.TryParse(value, out var parsedPlayers)) players = parsedPlayers;
            else throw new ArgumentException($"Unknown or invalid option: {argument}.");
        }

        if (seed is null) throw new ArgumentException("The --seed option is required.");
        if (players is null or < 1) throw new ArgumentException("--players must be at least 1.");
        return new SimulationOptions(seed.Value, players.Value, terminal);
    }

    private static bool TryParseSeed(string value, out long seed)
    {
        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out seed))
        {
            return true;
        }

        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            && ulong.TryParse(value[2..], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var bits))
        {
            seed = unchecked((long)bits);
            return true;
        }

        seed = default;
        return false;
    }
}

public static class SeededRandomStreams
{
    public static (Random Caller, IReadOnlyList<Random> Players) Create(long seed, int playerCount)
    {
        var root = new Random(unchecked((int)(seed ^ (seed >> 32))));
        var caller = new Random(root.Next());
        var players = Enumerable.Range(0, playerCount).Select(_ => new Random(root.Next())).ToList();
        return (caller, players);
    }
}
