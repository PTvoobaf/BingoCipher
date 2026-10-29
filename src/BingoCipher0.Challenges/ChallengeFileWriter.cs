namespace BingoCipher0.Challenges;

public static class ChallengeFileWriter
{
    public static string Write(CipherChallenge challenge, ChallengeCommandLineOptions options)
    {
        ArgumentNullException.ThrowIfNull(challenge);
        ArgumentNullException.ThrowIfNull(options);

        if (challenge.ChallengeType != options.ChallengeType)
            throw new ArgumentException("Challenge type does not match command-line options.", nameof(challenge));
        if (challenge.Mode != options.Mode)
            throw new ArgumentException("Challenge mode does not match command-line options.", nameof(challenge));
        if (challenge.ChallengeId != options.ChallengeId)
            throw new ArgumentException("Challenge ID does not match command-line options.", nameof(challenge));

        var fileName = $"{challenge.ChallengeType}{challenge.Mode}-{challenge.ChallengeId}.json";
        var outputPath = Path.Combine(options.OutputDirectory, fileName);
        var json = challenge.ToJson();
        File.WriteAllText(outputPath, json);
        if (options.Terminal) Console.WriteLine(json);
        return outputPath;
    }
}
