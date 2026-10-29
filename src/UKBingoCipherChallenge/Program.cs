using BingoCipher0.Challenges;

try
{
    var options = ChallengeCommandLineParser.Parse(args, "UK");
    var challenge = new UKChallengeGenerator().Generate(options);
    ChallengeFileWriter.Write(challenge, options);
    return 0;
}
catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException)
{
    Console.Error.WriteLine(exception.Message);
    return 2;
}
