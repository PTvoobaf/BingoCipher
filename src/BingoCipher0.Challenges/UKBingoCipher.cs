using System.Text;
using BingoCipher0.Core;

namespace BingoCipher0.Challenges;

public static class UKBingoCipher
{
    private const string KeyHex = "23f99a75efdafdd8ac728bdeaf2feda1d6b2fc8fdfa2e9ddfdafdca26a6eb5ea";
    private const string NonceHex = "b5e95e7289a6ba789c6ada2b";
    private const uint InitialCounter = 1;
    private static readonly byte[] Key = Convert.FromHexString(KeyHex);
    private static readonly byte[] Nonce = Convert.FromHexString(NonceHex);

    public static string Encrypt(
        string plaintext,
        IReadOnlyList<int> calledNumbers,
        IReadOnlyList<int> winningTicket,
        bool deterministicFiller,
        Random? fillerRandom,
        CiphertextEncoding encoding)
    {
        var grid = CreateGrid(plaintext, calledNumbers, winningTicket, deterministicFiller, fillerRandom);
        var encrypted = ChaCha20Cipher.Encrypt(Encoding.ASCII.GetBytes(grid), Key, Nonce, InitialCounter);
        return FormatCiphertext(encrypted, encoding);
    }

    public static string Decrypt(
        string ciphertext,
        CiphertextEncoding encoding,
        IReadOnlyList<int> calledNumbers,
        IReadOnlyList<int> winningTicket)
    {
        ArgumentNullException.ThrowIfNull(ciphertext);
        var encrypted = ParseCiphertext(ciphertext, encoding);
        if (encrypted.Length != UKBingoTicket.CellCount)
            throw new ArgumentException("UK ciphertext must decode to exactly 27 bytes.", nameof(ciphertext));

        var grid = Encoding.ASCII.GetString(ChaCha20Cipher.Decrypt(encrypted, Key, Nonce, InitialCounter));
        var markedIndices = GetMarkedIndicesInCallOrder(calledNumbers, winningTicket);
        return string.Concat(markedIndices.Select(index => grid[index]));
    }

    public static string CreateGrid(
        string plaintext,
        IReadOnlyList<int> calledNumbers,
        IReadOnlyList<int> winningTicket,
        bool deterministicFiller,
        Random? fillerRandom)
    {
        ArgumentNullException.ThrowIfNull(plaintext);
        if (plaintext.Any(character => character is < (char)32 or > (char)126))
            throw new ArgumentException("UK plaintext must contain only ASCII printable characters.", nameof(plaintext));
        if (!deterministicFiller && fillerRandom is null)
            throw new ArgumentNullException(nameof(fillerRandom), "Randomized UK filler requires a random source.");

        var markedIndices = GetMarkedIndicesInCallOrder(calledNumbers, winningTicket);
        if (plaintext.Length != markedIndices.Count)
            throw new ArgumentException("Plaintext length must equal the number of marked numbered spaces.", nameof(plaintext));

        var markedIndexSet = markedIndices.ToHashSet();
        var grid = new char[UKBingoTicket.CellCount];
        for (var index = 0; index < grid.Length; index++)
        {
            if (!markedIndexSet.Contains(index))
            {
                grid[index] = deterministicFiller ? '_' : (char)fillerRandom!.Next(32, 127);
            }
        }

        for (var index = 0; index < markedIndices.Count; index++)
        {
            grid[markedIndices[index]] = plaintext[index];
        }

        return new string(grid);
    }

    public static IReadOnlyList<int> GetMarkedIndicesInCallOrder(
        IReadOnlyList<int> calledNumbers,
        IReadOnlyList<int> winningTicket)
    {
        ArgumentNullException.ThrowIfNull(calledNumbers);
        ArgumentNullException.ThrowIfNull(winningTicket);
        if (calledNumbers.Count == 0)
            throw new ArgumentException("Called numbers must not be empty.", nameof(calledNumbers));
        if (calledNumbers.Any(number => number is < 1 or > 90) || calledNumbers.Distinct().Count() != calledNumbers.Count)
            throw new ArgumentException("Called numbers must be distinct values from 1 through 90.", nameof(calledNumbers));

        var ticket = new UKBingoTicket(winningTicket);
        if (!ticket.IsLineWinner())
            throw new ArgumentException("The selected UK ticket must be a line winner.", nameof(winningTicket));

        var calledSet = calledNumbers.ToHashSet();
        foreach (var value in ticket.Values.Where(value => value != 0))
        {
            if (value < 0 != calledSet.Contains(Math.Abs(value)))
                throw new ArgumentException("Ticket marks must match the called-number sequence.", nameof(winningTicket));
        }

        var indexByNumber = ticket.Values
            .Select((value, index) => (Number: Math.Abs(value), Index: index, Marked: value < 0))
            .Where(cell => cell.Marked)
            .ToDictionary(cell => cell.Number, cell => cell.Index);
        return calledNumbers.Where(indexByNumber.ContainsKey).Select(number => indexByNumber[number]).ToArray();
    }

    private static string FormatCiphertext(byte[] ciphertext, CiphertextEncoding encoding) => encoding switch
    {
        CiphertextEncoding.Hex => Convert.ToHexString(ciphertext).ToLowerInvariant(),
        CiphertextEncoding.Base64 => Convert.ToBase64String(ciphertext),
        _ => throw new ArgumentException("UK ciphertext encoding must be Hex or Base64.", nameof(encoding))
    };

    private static byte[] ParseCiphertext(string ciphertext, CiphertextEncoding encoding) => encoding switch
    {
        CiphertextEncoding.Hex => Convert.FromHexString(ciphertext),
        CiphertextEncoding.Base64 => Convert.FromBase64String(ciphertext),
        _ => throw new ArgumentException("UK ciphertext encoding must be Hex or Base64.", nameof(encoding))
    };
}
