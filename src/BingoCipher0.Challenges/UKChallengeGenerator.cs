using BingoCipher0.Core;
using BingoCipher0.Sim;

namespace BingoCipher0.Challenges;

public sealed class UKChallengeGenerator
{
    public CipherChallenge Generate(ChallengeCommandLineOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.ChallengeType != "UK")
            throw new ArgumentException("UK challenge generation requires UK command-line options.", nameof(options));

        var simulation = new UKBingoSimulator().Run(options.Seed, options.Players);
        var winningTicket = SelectWinningTicket(simulation.WinningTickets);
        var markedCount = winningTicket.Values.Count(value => value < 0);
        var plaintext = EmbeddedPlaintextLibrary.LoadUk().Select(
            markedCount,
            SeededChallengeRandom.CreatePlaintextSelection(options.Seed));
        var encoding = options.UseBase64 ? CiphertextEncoding.Base64 : CiphertextEncoding.Hex;
        var ciphertext = UKBingoCipher.Encrypt(
            plaintext,
            simulation.CalledNumbers,
            winningTicket.Values,
            deterministicFiller: options.IsEncode,
            fillerRandom: options.IsEncode ? null : SeededChallengeRandom.CreateUkFiller(options.Seed),
            encoding);

        return new CipherChallenge(
            1,
            "UK",
            options.Mode,
            options.ChallengeId,
            options.Seed,
            plaintext,
            ciphertext,
            simulation.CalledNumbers,
            winningTicket.Values,
            encoding);
    }

    public static UKBingoTicket SelectWinningTicket(IReadOnlyList<UKBingoTicket> winningTickets)
    {
        ArgumentNullException.ThrowIfNull(winningTickets);
        if (winningTickets.Count == 0)
            throw new ArgumentException("At least one winning ticket is required.", nameof(winningTickets));

        // UKBingoSimulator returns winners in ascending player ID, then strip-ticket order.
        return new UKBingoTicket(winningTickets[0].Values);
    }
}
