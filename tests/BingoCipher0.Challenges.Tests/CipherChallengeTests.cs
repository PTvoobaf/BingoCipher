using BingoCipher0.Challenges;

namespace BingoCipher0.Challenges.Tests;

public sealed class CipherChallengeTests
{
    [Theory]
    [MemberData(nameof(UkChallenges))]
    public void JsonRoundTripPreservesCompleteUkChallenge(CipherChallenge challenge)
    {
        var json = challenge.ToJson();
        var roundTripped = CipherChallenge.FromJson(json);

        Assert.Contains("\n", json);
        Assert.Contains("\"plaintext\":", json);
        Assert.Contains("\"ciphertext\":", json);
        Assert.Contains("\"winningTicket\":", json);
        Assert.Contains("\"calledNumbers\":", json);
        Assert.Contains("\"ciphertextEncoding\": \"Hex\"", json);
        Assert.Equal(challenge.SchemaVersion, roundTripped.SchemaVersion);
        Assert.Equal(challenge.ChallengeType, roundTripped.ChallengeType);
        Assert.Equal(challenge.Mode, roundTripped.Mode);
        Assert.Equal(challenge.ChallengeId, roundTripped.ChallengeId);
        Assert.Equal(challenge.Seed, roundTripped.Seed);
        Assert.Equal(challenge.Plaintext, roundTripped.Plaintext);
        Assert.Equal(challenge.Ciphertext, roundTripped.Ciphertext);
        Assert.Equal(challenge.CalledNumbers, roundTripped.CalledNumbers);
        Assert.Equal(challenge.WinningTicket, roundTripped.WinningTicket);
        Assert.Equal(challenge.CiphertextEncoding, roundTripped.CiphertextEncoding);
    }

    public static IEnumerable<object[]> UkChallenges()
    {
        yield return [new CipherChallenge(
            1, "UK", "encode", "uk-example", 42, "Hello", "aabbcc", [1, 2, 3],
            [-1, -2, -3, -4, -5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
            CiphertextEncoding.Hex)];
    }

    [Theory]
    [MemberData(nameof(UsChallenges))]
    public void JsonRoundTripSupportsUsAlphabeticChallenge(CipherChallenge challenge)
    {
        var json = challenge.ToJson();

        var roundTripped = CipherChallenge.FromJson(json);

        Assert.Contains("\n", json);
        Assert.Contains("\"plaintext\":", json);
        Assert.Contains("\"ciphertext\":", json);
        Assert.Contains("\"winningTicket\":", json);
        Assert.Contains("\"calledNumbers\":", json);
        Assert.Contains("\"ciphertextEncoding\":", json);
        Assert.Equal(challenge.SchemaVersion, roundTripped.SchemaVersion);
        Assert.Equal(challenge.ChallengeType, roundTripped.ChallengeType);
        Assert.Equal(challenge.Mode, roundTripped.Mode);
        Assert.Equal(challenge.ChallengeId, roundTripped.ChallengeId);
        Assert.Equal(challenge.Seed, roundTripped.Seed);
        Assert.Equal(challenge.Plaintext, roundTripped.Plaintext);
        Assert.Equal(challenge.Ciphertext, roundTripped.Ciphertext);
        Assert.Equal(challenge.CalledNumbers, roundTripped.CalledNumbers);
        Assert.Equal(challenge.CiphertextEncoding, roundTripped.CiphertextEncoding);
        Assert.Equal(challenge.WinningTicket, roundTripped.WinningTicket);
    }

    public static IEnumerable<object[]> UsChallenges()
    {
        yield return [new CipherChallenge(
            1, "US", "decode", "us-example1", 1776, "Hulk", "AbCdE", [1, 2, 3],
            [-1, -2, -3, -4, 5, 6, 7, 8, 9, 10, 11, 0, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25],
            CiphertextEncoding.Alphabetic)];
        yield return [new CipherChallenge(
            1, "US", "decode", "us-example2", 1776, "GLaDOS", "AbCdEfG",
            [45, 30, 70, 36, 1, 18, 54, 35, 63, 6, 9, 40, 25, 68, 69, 22, 46, 34, 43, 71, 73, 14, 44],
            [ 2, 28, -43, 57, 62,
              11, 16, -44, 56, 64,
              15, -22, 0, 53, 75,
              13, 21, -45, 60, 61,
              10, 23, -36, 51, -71],
            CiphertextEncoding.Alphabetic)];
    }

    [Fact]
    public void RejectsInvalidSchemaAndVariantRules()
    {
        Assert.Throws<ArgumentException>(() => CreateUk(schemaVersion: 2));
        Assert.Throws<ArgumentException>(() => CreateUk(challengeType: "CA"));
        Assert.Throws<ArgumentException>(() => CreateUk(mode: "invalid"));
        Assert.Throws<ArgumentException>(() => CreateUk(challengeId: "  "));
        Assert.Throws<ArgumentException>(() => CreateUk(calledNumbers: []));
        Assert.Throws<ArgumentException>(() => CreateUk(winningTicket: [-1]));
        Assert.Throws<ArgumentException>(() => CreateUk(plaintext: "Hell"));
        Assert.Throws<ArgumentException>(() => CreateUk(ciphertextEncoding: CiphertextEncoding.Alphabetic));
        Assert.Throws<ArgumentException>(() => CreateUk(plaintext: "Hell\n"));
        Assert.Throws<ArgumentException>(() => CreateUs(winningTicket: [-1]));
        Assert.Throws<ArgumentException>(() => CreateUs(ciphertextEncoding: CiphertextEncoding.Hex));
        Assert.Throws<ArgumentException>(() => CreateUs(plaintext: "Hulk!"));
    }

    private static CipherChallenge CreateUk(
        int schemaVersion = 1,
        string challengeType = "UK",
        string mode = "encode",
        string challengeId = "id",
        long seed = 1,
        string plaintext = "Hello",
        string ciphertext = "aabbcc",
        IReadOnlyList<int>? calledNumbers = null,
        IReadOnlyList<int>? winningTicket = null,
        CiphertextEncoding ciphertextEncoding = CiphertextEncoding.Hex)
        => new(
            schemaVersion, challengeType, mode, challengeId, seed, plaintext, ciphertext,
            calledNumbers ?? [1],
            winningTicket ?? [-1, -2, -3, -4, -5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
            ciphertextEncoding);

    private static CipherChallenge CreateUs(
        string plaintext = "Hulk",
        string ciphertext = "AbCdE",
        IReadOnlyList<int>? calledNumbers = null,
        IReadOnlyList<int>? winningTicket = null,
        CiphertextEncoding ciphertextEncoding = CiphertextEncoding.Alphabetic)
        => new(
            1, "US", "encode", "id", 1, plaintext, ciphertext,
            calledNumbers ?? [1],
            winningTicket ?? [-1, -2, -3, -4, -5, 6, 7, 8, 9, 10, 11, 0, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25],
            ciphertextEncoding);
}
