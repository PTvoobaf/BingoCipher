using BingoCipher0.Challenges;

namespace BingoCipher0.Challenges.Tests;

public sealed class ChallengeCommandLineTests
{
    [Fact]
    public void ParsesDefaultsAndNamedValues()
    {
        var options = ChallengeCommandLineParser.Parse(["--seed=0xdeadbeef8badf00d", "--id", "contest-1", "--encode", "--base64"], "UK");

        Assert.Equal(unchecked((long)0xdeadbeef8badf00dUL), options.Seed);
        Assert.Equal(21, options.Players);
        Assert.Equal("contest-1", options.ChallengeId);
        Assert.Equal("encode", options.Mode);
        Assert.Equal(".", options.OutputDirectory);
        Assert.True(options.UseBase64);
        Assert.False(options.Terminal);
    }

    [Fact]
    public void ParsesUsDecodeAndExistingOutputDirectory()
    {
        var directory = Directory.GetCurrentDirectory();
        var options = ChallengeCommandLineParser.Parse(
            ["--seed", "42", "--players=7", "--id=us_1", "--decode", "--output-directory", directory, "--terminal"], "US");

        Assert.Equal(42, options.Seed);
        Assert.Equal(7, options.Players);
        Assert.Equal("decode", options.Mode);
        Assert.Equal(directory, options.OutputDirectory);
        Assert.True(options.Terminal);
        Assert.False(options.UseBase64);
    }

    [Theory]
    [InlineData("--id", "x", "--encode")]
    [InlineData("--seed", "42", "--id", "x")]
    [InlineData("--seed", "42", "--id", "x", "--encode", "--decode")]
    [InlineData("--seed", "42", "--id", "x", "--encode", "--players", "0")]
    [InlineData("--seed", "42", "--id", "x", "--encode", "--output-directory", "missing-directory")]
    [InlineData("--seed", "42", "--id", "bad/id", "--encode")]
    public void RejectsInvalidArguments(params string[] arguments)
    {
        Assert.Throws<ArgumentException>(() => ChallengeCommandLineParser.Parse(arguments, "UK"));
    }

    [Fact]
    public void RejectsBase64ForUsChallenges()
    {
        Assert.Throws<ArgumentException>(() => ChallengeCommandLineParser.Parse(["--seed", "42", "--id", "x", "--encode", "--base64"], "US"));
    }
}
