using System.Globalization;
using System.Text.RegularExpressions;

namespace BingoCipher0.Challenges;

public sealed record ChallengeCommandLineOptions(
    string ChallengeType,
    long Seed,
    int Players,
    string ChallengeId,
    string Mode,
    string OutputDirectory,
    bool Terminal,
    bool UseBase64)
{
    public bool IsEncode => Mode == "encode";
    public bool IsDecode => Mode == "decode";
}

public static partial class ChallengeCommandLineParser
{
    public static ChallengeCommandLineOptions Parse(IReadOnlyList<string> arguments, string challengeType)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        if (challengeType is not ("UK" or "US")) throw new ArgumentException("Challenge type must be UK or US.", nameof(challengeType));

        long? seed = null;
        var players = 21;
        string? challengeId = null;
        string? mode = null;
        var outputDirectory = ".";
        var terminal = false;
        var useBase64 = false;

        for (var index = 0; index < arguments.Count; index++)
        {
            var argument = arguments[index];
            if (argument == "--encode" || argument == "--decode")
            {
                if (mode is not null) throw new ArgumentException("Specify exactly one of --encode or --decode.");
                mode = argument[2..];
                continue;
            }

            if (argument == "--terminal")
            {
                terminal = true;
                continue;
            }

            if (argument == "--base64")
            {
                if (challengeType != "UK") throw new ArgumentException("--base64 is valid only for UK challenges.");
                useBase64 = true;
                continue;
            }

            var (name, value) = ReadNamedArgument(argument, arguments, ref index);
            switch (name)
            {
                case "--seed" when TryParseSeed(value, out var parsedSeed):
                    seed = parsedSeed;
                    break;
                case "--players" when int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPlayers)
                    && parsedPlayers > 0:
                    players = parsedPlayers;
                    break;
                case "--id" when IsSafeMachineId(value):
                    challengeId = value;
                    break;
                case "--output-directory" when Directory.Exists(value):
                    outputDirectory = value;
                    break;
                default:
                    throw new ArgumentException($"Unknown or invalid option: {argument}.");
            }
        }

        if (seed is null) throw new ArgumentException("The --seed option is required.");
        if (challengeId is null) throw new ArgumentException("The --id option is required and must be filename-safe.");
        if (mode is null) throw new ArgumentException("Exactly one of --encode or --decode is required.");

        return new ChallengeCommandLineOptions(challengeType, seed.Value, players, challengeId, mode, outputDirectory, terminal, useBase64);
    }

    private static (string Name, string Value) ReadNamedArgument(string argument, IReadOnlyList<string> arguments, ref int index)
    {
        var separator = argument.IndexOf('=');
        var name = separator < 0 ? argument : argument[..separator];
        if (!name.StartsWith("--", StringComparison.Ordinal) || name.Length == 2)
            throw new ArgumentException($"Unknown or invalid option: {argument}.");

        if (separator >= 0) return (name, argument[(separator + 1)..]);
        if (index + 1 >= arguments.Count) throw new ArgumentException($"Missing value for {name}.");
        return (name, arguments[++index]);
    }

    private static bool TryParseSeed(string value, out long seed)
    {
        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out seed)) return true;
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            && ulong.TryParse(value[2..], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var bits))
        {
            seed = unchecked((long)bits);
            return true;
        }

        seed = default;
        return false;
    }

    private static bool IsSafeMachineId(string value) => SafeMachineIdPattern().IsMatch(value);

    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9._-]{0,63}$")]
    private static partial Regex SafeMachineIdPattern();
}
