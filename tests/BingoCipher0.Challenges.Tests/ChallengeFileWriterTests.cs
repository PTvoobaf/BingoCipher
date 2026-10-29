using BingoCipher0.Challenges;

namespace BingoCipher0.Challenges.Tests;

public sealed class ChallengeFileWriterTests
{
    private static readonly object ConsoleLock = new();

    [Fact]
    public void WritesIndentedJsonToModeSpecificFilenameAndOverwritesExistingFile()
    {
        var outputDirectory = Directory.CreateTempSubdirectory("BingoCipher0-3.3-");
        try
        {
            var options = ChallengeCommandLineParser.Parse(
                ["--seed", "42", "--id", "uk-test", "--encode", "--output-directory", outputDirectory.FullName], "UK");
            var challenge = new CipherChallenge(
                1, "UK", "encode", "uk-test", 42, "Hello", "aabbcc", [1],
                [-1, -2, -3, -4, -5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
                CiphertextEncoding.Hex);

            var path = ChallengeFileWriter.Write(challenge, options);
            var firstJson = File.ReadAllText(path);
            Assert.Equal(Path.Combine(outputDirectory.FullName, "UKencode-uk-test.json"), path);
            Assert.Contains("\n", firstJson);

            var replacement = new CipherChallenge(
                1, "UK", "encode", "uk-test", 42, "Hello", "ddeeff", [1],
                [-1, -2, -3, -4, -5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
                CiphertextEncoding.Hex);
            ChallengeFileWriter.Write(replacement, options);
            Assert.Contains("ddeeff", File.ReadAllText(path));
        }
        finally
        {
            outputDirectory.Delete(true);
        }
    }

    [Fact]
    public void WritesTheSameFormattedJsonToTerminalWhenRequested()
    {
        var outputDirectory = Directory.CreateTempSubdirectory("BingoCipher0-3.3-terminal-");
        var originalOutput = Console.Out;
        try
        {
            var options = ChallengeCommandLineParser.Parse(
                ["--seed", "42", "--id", "uk-terminal", "--decode", "--output-directory", outputDirectory.FullName, "--terminal"], "UK");
            var challenge = new CipherChallenge(
                1, "UK", "decode", "uk-terminal", 42, "Hello", "aabbcc", [1],
                [-1, -2, -3, -4, -5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
                CiphertextEncoding.Hex);

            lock (ConsoleLock)
            {
                using var terminalOutput = new StringWriter();
                Console.SetOut(terminalOutput);
                var path = ChallengeFileWriter.Write(challenge, options);
                Console.SetOut(originalOutput);

                Assert.Equal(File.ReadAllText(path) + Environment.NewLine, terminalOutput.ToString());
            }
        }
        finally
        {
            Console.SetOut(originalOutput);
            outputDirectory.Delete(true);
        }
    }
}
