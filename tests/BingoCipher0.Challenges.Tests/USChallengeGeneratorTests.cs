using BingoCipher0.Challenges;
using BingoCipher0.Sim;

namespace BingoCipher0.Challenges.Tests;

public sealed class USChallengeGeneratorTests
{
    [Theory]
    [InlineData("--encode")]
    [InlineData("--decode")]
    public void GeneratesCompleteReproducibleUsChallenge(string mode)
    {
        // argument supplied for mode by InLineData
        //   is either "--encode" or "--decode".
        var options = ChallengeCommandLineParser.Parse(
            ["--seed", "1776", "--players", "21", "--id", "us-3-6", mode], "US");
        var generator = new USChallengeGenerator();

        var first = generator.Generate(options);
        var second = generator.Generate(options);

        Assert.Equal(first.ToJson(), second.ToJson());
        Assert.Equal("US", first.ChallengeType);
        Assert.Equal(mode[2..], first.Mode);
        Assert.Equal(CiphertextEncoding.Alphabetic, first.CiphertextEncoding);
        Assert.InRange(first.Plaintext.Length, 4, 20);
        Assert.Equal(first.Plaintext.Length, first.WinningTicket.Count(value => value < 0));
        Assert.Equal(25, first.Ciphertext.Length);
        Assert.All(first.Ciphertext, character => Assert.True(char.IsAsciiLetter(character)));
        Assert.Equal(first.Plaintext, USBingoCipher.Decrypt(first.Ciphertext, first.CalledNumbers, first.WinningTicket));
        Assert.Contains("\"plaintext\":", first.ToJson());
        Assert.Contains("\"ciphertext\":", first.ToJson());
        Assert.Contains("\"ciphertextEncoding\": \"Alphabetic\"", first.ToJson());
    }

    [Fact]
    public void EncodeAndDecodeUseDeterministicAndRandomizedFillerRespectively()
    {
        var encode = new USChallengeGenerator().Generate(
            ChallengeCommandLineParser.Parse(["--seed", "1776", "--id", "encode", "--encode"], "US"));
        var decode = new USChallengeGenerator().Generate(
            ChallengeCommandLineParser.Parse(["--seed", "1776", "--id", "decode", "--decode"], "US"));

        Assert.Equal(encode.Plaintext, decode.Plaintext);
        Assert.Equal(encode.CalledNumbers, decode.CalledNumbers);
        Assert.Equal(encode.WinningTicket, decode.WinningTicket);
        Assert.NotEqual(encode.Ciphertext, decode.Ciphertext);
    }

    [Fact]
    public void SimultaneousSimulatorWinnersAreSelectedReproducibly()
    {
        var simulator = new USBingoSimulator();
        var result = Enumerable.Range(0, 1_000)
            .Select(seed => (Seed: seed, Result: simulator.Run(seed, 21)))
            .First(candidate => candidate.Result.WinningCards.Count > 1);

        var repeated = simulator.Run(result.Seed, 21);
        var firstSelection = USChallengeGenerator.SelectWinningCard(result.Result.WinningCards);
        var secondSelection = USChallengeGenerator.SelectWinningCard(repeated.WinningCards);

        Assert.True(result.Result.WinningCards.Count > 1);
        Assert.Equal(result.Result.WinningCards[0].Values, firstSelection.Values);
        Assert.Equal(firstSelection.Values, secondSelection.Values);
    }
}
