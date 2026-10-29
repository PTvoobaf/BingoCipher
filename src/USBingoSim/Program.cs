using BingoCipher0.Sim;

try
{
    var options = SimulationArgumentParser.Parse(args);
    var result = new USBingoSimulator().Run(options.Seed, options.Players);
    if (options.Terminal)
    {
        Console.WriteLine($"Called: {string.Join(", ", result.CalledNumbers)}");
        Console.WriteLine($"Winning cards: {result.WinningCards.Count}");
        for (var index = 0; index < result.WinningCards.Count; index++)
        {
            Console.Write(result.WinningCards[index]);
            if (index < result.WinningCards.Count - 1) Console.WriteLine();
        }
    }
    else Console.WriteLine(result.ToJson());
    return 0;
}
catch (ArgumentException exception)
{
    Console.Error.WriteLine(exception.Message);
    return 2;
}
