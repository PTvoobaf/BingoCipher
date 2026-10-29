using BingoCipher0.Core;
using BingoCipher0.Sim;

namespace BingoCipher0.Challenges;

public sealed class USChallengeGenerator
{
    public CipherChallenge Generate(ChallengeCommandLineOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.ChallengeType != "US")
            throw new ArgumentException("US challenge generation requires US command-line options.", nameof(options));

        var simulation = new USBingoSimulator().Run(options.Seed, options.Players);
        var winningCard = SelectWinningCard(simulation.WinningCards);
        var markedCount = winningCard.Values.Count(value => value < 0);
        var plaintext = EmbeddedPlaintextLibrary.LoadUs().Select(
            markedCount,
            SeededChallengeRandom.CreatePlaintextSelection(options.Seed));
        var ciphertext = USBingoCipher.Encrypt(
            plaintext,
            simulation.CalledNumbers,
            winningCard.Values,
            deterministicFiller: options.IsEncode,
            fillerRandom: options.IsEncode ? null : SeededChallengeRandom.CreateUsFiller(options.Seed));

        return new CipherChallenge(
            1,
            "US",
            options.Mode,
            options.ChallengeId,
            options.Seed,
            plaintext,
            ciphertext,
            simulation.CalledNumbers,
            winningCard.Values,
            CiphertextEncoding.Alphabetic);
    }

    public static USBingoCard SelectWinningCard(IReadOnlyList<USBingoCard> winningCards)
    {
        ArgumentNullException.ThrowIfNull(winningCards);
        if (winningCards.Count == 0)
            throw new ArgumentException("At least one winning card is required.", nameof(winningCards));

        // USBingoSimulator returns winners in ascending player ID.
        return new USBingoCard(winningCards[0].Values);
    }
}
