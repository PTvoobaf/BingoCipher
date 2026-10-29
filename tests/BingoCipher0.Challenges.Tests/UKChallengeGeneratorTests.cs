using BingoCipher0.Challenges;
using BingoCipher0.Sim;

namespace BingoCipher0.Challenges.Tests;

public sealed class UKChallengeGeneratorTests
{
    [Theory]
    [InlineData("--encode", false, CiphertextEncoding.Hex)]
    [InlineData("--decode", false, CiphertextEncoding.Hex)]
    [InlineData("--encode", true, CiphertextEncoding.Base64)]
    [InlineData("--decode", true, CiphertextEncoding.Base64)]
    public void GeneratesCompleteReproducibleUkChallenge(string mode, bool base64, CiphertextEncoding expectedEncoding)
    {
        var arguments = new List<string> { "--seed", "42", "--players", "21", "--id", "uk-3-5", mode };
        if (base64) arguments.Add("--base64");
        var options = ChallengeCommandLineParser.Parse(arguments, "UK");
        var generator = new UKChallengeGenerator();

        var first = generator.Generate(options);
        var second = generator.Generate(options);

        Assert.Equal(first.ToJson(), second.ToJson());
        Assert.Equal("UK", first.ChallengeType);
        Assert.Equal(mode[2..], first.Mode);
        Assert.Equal(expectedEncoding, first.CiphertextEncoding);
        Assert.InRange(first.Plaintext.Length, 5, 15);
        Assert.Equal(first.Plaintext.Length, first.WinningTicket.Count(value => value < 0));
        Assert.Equal(
            first.Plaintext,
            UKBingoCipher.Decrypt(first.Ciphertext, first.CiphertextEncoding, first.CalledNumbers, first.WinningTicket));
        Assert.Contains("\"plaintext\":", first.ToJson());
        Assert.Contains("\"ciphertext\":", first.ToJson());
        Assert.Contains($"\"ciphertextEncoding\": \"{expectedEncoding}\"", first.ToJson());
    }

    [Fact]
    public void EncodeAndDecodeUseDeterministicAndRandomizedFillerRespectively()
    {
        var encode = new UKChallengeGenerator().Generate(
            ChallengeCommandLineParser.Parse(["--seed", "42", "--id", "encode", "--encode"], "UK"));
        var decode = new UKChallengeGenerator().Generate(
            ChallengeCommandLineParser.Parse(["--seed", "42", "--id", "decode", "--decode"], "UK"));

        Assert.Equal(encode.Plaintext, decode.Plaintext);
        Assert.Equal(encode.CalledNumbers, decode.CalledNumbers);
        Assert.Equal(encode.WinningTicket, decode.WinningTicket);
        Assert.NotEqual(encode.Ciphertext, decode.Ciphertext);
    }

    [Fact]
    public void SimultaneousSimulatorWinnersAreSelectedReproducibly()
    {
        var simulator = new UKBingoSimulator();
        var result = Enumerable.Range(0, 1_000)
            .Select(seed => (Seed: seed, Result: simulator.Run(seed, 21)))
            .First(candidate => candidate.Result.WinningTickets.Count > 1);

        var repeated = simulator.Run(result.Seed, 21);
        var firstSelection = UKChallengeGenerator.SelectWinningTicket(result.Result.WinningTickets);
        var secondSelection = UKChallengeGenerator.SelectWinningTicket(repeated.WinningTickets);

        Assert.True(result.Result.WinningTickets.Count > 1);
        Assert.Equal(result.Result.WinningTickets[0].Values, firstSelection.Values);
        Assert.Equal(firstSelection.Values, secondSelection.Values);
    }
}
