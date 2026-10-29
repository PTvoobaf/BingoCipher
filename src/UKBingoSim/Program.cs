using BingoCipher0.Sim;

try
{
    var options = SimulationArgumentParser.Parse(args);
    var result = new UKBingoSimulator().Run(options.Seed, options.Players);
    if (options.Terminal)
    {
        Console.WriteLine($"Called: {string.Join(", ", result.CalledNumbers)}");
        Console.WriteLine($"Winning tickets: {result.WinningTickets.Count}");
        for (var index = 0; index < result.WinningTickets.Count; index++)
        {
            Console.Write(result.WinningTickets[index]);
            if (index < result.WinningTickets.Count - 1) Console.WriteLine();
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
