using BingoCipher0.Core;

namespace BingoCipher0.Challenges;

public static class USBingoCipher
{
    public static string Encrypt(
        string plaintext,
        IReadOnlyList<int> calledNumbers,
        IReadOnlyList<int> winningTicket,
        bool deterministicFiller,
        Random? fillerRandom)
        => CreateGrid(plaintext, calledNumbers, winningTicket, deterministicFiller, fillerRandom);

    public static string Decrypt(
        string ciphertext,
        IReadOnlyList<int> calledNumbers,
        IReadOnlyList<int> winningTicket)
    {
        ArgumentNullException.ThrowIfNull(ciphertext);
        if (ciphertext.Length != USBingoCard.CellCount || ciphertext.Any(character => !char.IsAsciiLetter(character)))
            throw new ArgumentException("US ciphertext must contain exactly 25 alphabetic ASCII characters.", nameof(ciphertext));

        var markedIndices = GetMarkedIndicesInCallOrder(calledNumbers, winningTicket);
        var rotatedPlaintext = string.Concat(markedIndices.Select(index => ciphertext[index]));
        return Rot13(rotatedPlaintext);
    }

    public static string CreateGrid(
        string plaintext,
        IReadOnlyList<int> calledNumbers,
        IReadOnlyList<int> winningTicket,
        bool deterministicFiller,
        Random? fillerRandom)
    {
        ArgumentNullException.ThrowIfNull(plaintext);
        if (plaintext.Any(character => !char.IsAsciiLetter(character)))
            throw new ArgumentException("US plaintext must contain only alphabetic ASCII characters.", nameof(plaintext));
        if (!deterministicFiller && fillerRandom is null)
            throw new ArgumentNullException(nameof(fillerRandom), "Randomized US filler requires a random source.");

        var markedIndices = GetMarkedIndicesInCallOrder(calledNumbers, winningTicket);
        if (plaintext.Length != markedIndices.Count)
            throw new ArgumentException("Plaintext length must equal the number of marked numbered spaces.", nameof(plaintext));

        var rotatedPlaintext = Rot13(plaintext);
        var markedIndexSet = markedIndices.ToHashSet();
        var grid = new char[USBingoCard.CellCount];
        for (var index = 0; index < grid.Length; index++)
        {
            if (!markedIndexSet.Contains(index))
            {
                grid[index] = deterministicFiller ? 'B' : NextAlphabeticCharacter(fillerRandom!);
            }
        }

        for (var index = 0; index < markedIndices.Count; index++)
        {
            grid[markedIndices[index]] = rotatedPlaintext[index];
        }

        return new string(grid);
    }

    public static string Rot13(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var characters = value.ToCharArray();
        for (var index = 0; index < characters.Length; index++)
        {
            characters[index] = characters[index] switch
            {
                >= 'A' and <= 'Z' => (char)('A' + (characters[index] - 'A' + 13) % 26),
                >= 'a' and <= 'z' => (char)('a' + (characters[index] - 'a' + 13) % 26),
                _ => throw new ArgumentException("ROT13 input must contain only alphabetic ASCII characters.", nameof(value))
            };
        }

        return new string(characters);
    }

    public static IReadOnlyList<int> GetMarkedIndicesInCallOrder(
        IReadOnlyList<int> calledNumbers,
        IReadOnlyList<int> winningTicket)
    {
        ArgumentNullException.ThrowIfNull(calledNumbers);
        ArgumentNullException.ThrowIfNull(winningTicket);
        if (calledNumbers.Count == 0)
            throw new ArgumentException("Called numbers must not be empty.", nameof(calledNumbers));
        if (calledNumbers.Any(number => number is < 1 or > 75) || calledNumbers.Distinct().Count() != calledNumbers.Count)
            throw new ArgumentException("Called numbers must be distinct values from 1 through 75.", nameof(calledNumbers));

        var card = new USBingoCard(winningTicket);
        if (!card.IsWinner())
            throw new ArgumentException("The selected US card must be a winner.", nameof(winningTicket));

        var calledSet = calledNumbers.ToHashSet();
        foreach (var value in card.Values.Where(value => value != 0))
        {
            if (value < 0 != calledSet.Contains(Math.Abs(value)))
                throw new ArgumentException("Card marks must match the called-number sequence.", nameof(winningTicket));
        }

        var indexByNumber = card.Values
            .Select((value, index) => (Number: Math.Abs(value), Index: index, Marked: value < 0))
            .Where(cell => cell.Marked)
            .ToDictionary(cell => cell.Number, cell => cell.Index);
        return calledNumbers.Where(indexByNumber.ContainsKey).Select(number => indexByNumber[number]).ToArray();
    }

    private static char NextAlphabeticCharacter(Random random)
    {
        var value = random.Next(52);
        return value < 26 ? (char)('A' + value) : (char)('a' + value - 26);
    }
}
