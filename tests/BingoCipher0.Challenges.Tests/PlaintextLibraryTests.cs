using BingoCipher0.Challenges;

namespace BingoCipher0.Challenges.Tests;

public sealed class PlaintextLibraryTests
{
    [Fact]
    public void EmbeddedUkLibraryContainsValidCandidatesForEverySupportedLength()
    {
        var library = EmbeddedPlaintextLibrary.LoadUk();

        Assert.Equal(Enumerable.Range(5, 11), library.MessagesByLength.Keys.Order());
        foreach (var (length, messages) in library.MessagesByLength)
        {
            Assert.InRange(messages.Count, 1, 19);
            Assert.All(messages, message =>
            {
                Assert.Equal(length, message.Length);
                Assert.All(message, character => Assert.InRange(character, ' ', '~'));
            });
        }
    }

    [Fact]
    public void EmbeddedUsLibraryContainsAlphabeticCandidatesForEverySupportedLength()
    {
        var library = EmbeddedPlaintextLibrary.LoadUs();

        Assert.Equal(Enumerable.Range(4, 17), library.MessagesByLength.Keys.Order());
        foreach (var (length, messages) in library.MessagesByLength)
        {
            Assert.InRange(messages.Count, 1, 19);
            Assert.All(messages, message =>
            {
                Assert.Equal(length, message.Length);
                Assert.All(message, character => Assert.True(char.IsAsciiLetter(character)));
            });
        }
    }

    [Fact]
    public void SeededSelectionIsReproducible()
    {
        var library = EmbeddedPlaintextLibrary.LoadUs();
        var firstRandom = SeededChallengeRandom.CreatePlaintextSelection(42);
        var secondRandom = SeededChallengeRandom.CreatePlaintextSelection(42);

        var first = Enumerable.Range(4, 17).Select(length => library.Select(length, firstRandom)).ToArray();
        var second = Enumerable.Range(4, 17).Select(length => library.Select(length, secondRandom)).ToArray();

        Assert.Equal(first, second);
    }

    [Fact]
    public void PurposeStreamsAreDomainSeparatedAndReproducible()
    {
        var firstPlaintext = SeededChallengeRandom.CreatePlaintextSelection(42);
        var secondPlaintext = SeededChallengeRandom.CreatePlaintextSelection(42);
        var filler = SeededChallengeRandom.CreateUkFiller(42);

        var firstSequence = Enumerable.Range(0, 8).Select(_ => firstPlaintext.Next()).ToArray();
        var secondSequence = Enumerable.Range(0, 8).Select(_ => secondPlaintext.Next()).ToArray();
        var fillerSequence = Enumerable.Range(0, 8).Select(_ => filler.Next()).ToArray();

        Assert.Equal(firstSequence, secondSequence);
        Assert.NotEqual(firstSequence, fillerSequence);
    }
}
