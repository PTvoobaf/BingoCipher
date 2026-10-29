using BingoCipher0.Challenges;
using BingoCipher0.Core;

namespace BingoCipher0.Challenges.Tests;

public sealed class UKBingoCipherTests
{
    private static IEnumerable<(
        string Plaintext,
        int[] WinningTicket,
        int[] CalledNumbers,
        int[] ExpectedIndices,
        string ExpectedGrid
        )>
        TestCases()
    {
        yield return
        (
            "ABCDEFG",
            new[]
            {
                -1, 0, -21, 0, -41, 0, -61, 0, -81,
                -8, -12, 0, 32, 0, 52, 0, 72, 0,
                10, 0, 0, 31, 42, 0, 62, 73, 0
            }, // seven numbers daubed, top line is winner
            new[] { 21, 1, 61, 41, 8, 12, 81 },
            new[] { 2, 0, 6, 4, 9, 10, 8 },
            "B_A_D_C_G" + "EF_______" + "_________"
        );

        yield return
        (
            "RunAway!",
            new[]
            {
                4, 0, -28, 0, 50, 0, 0, 78, 82,
                0, 13, 0, -32, 41, -59, 63, 0, 0,
                -8, 0, 0, -37, 0, 0, -70, -72, -81
            }, // eight numbers daubed, bottom line is winner
            new[] { 28, 32, 45, 42, 36, 34, 59, 16, 81, 85, 5, 39, 27, 88, 67, 11, 70, 89, 72, 80, 17, 8, 71, 51, 46, 73, 47, 25, 9, 37 },
            new[] { 2, 12, 14, 26, 24, 25, 18, 21 },
            "__R______" + "___u_n___" + "y__!__waA"
        );
    }

    public static IEnumerable<object[]> UkCipherCases()
    {
        foreach (var testCase in TestCases())
        {
            yield return [testCase.Plaintext, testCase.WinningTicket, testCase.CalledNumbers];
        }
    }

    public static IEnumerable<object[]> UkGridPlacementCases()
    {
        foreach (var testCase in TestCases())
        {
            yield return
            [
                testCase.Plaintext,
                testCase.WinningTicket,
                testCase.CalledNumbers,
                testCase.ExpectedIndices,
                testCase.ExpectedGrid
            ];
        }
    }

    public static IEnumerable<object[]> UkCipherEncodingCases()
    {
        foreach (var testCase in TestCases())
        {
            yield return [testCase.Plaintext, testCase.WinningTicket, testCase.CalledNumbers, CiphertextEncoding.Hex];
            yield return [testCase.Plaintext, testCase.WinningTicket, testCase.CalledNumbers, CiphertextEncoding.Base64];
        }
    }

    [Theory]
    [MemberData(nameof(UkGridPlacementCases))]
    public void WinningTicketExtractionAndDeterministicGridFollowCallOrder(
        string plaintext,
        int[] winningTicket,
        int[] calledNumbers,
        int[] expectedIndices,
        string expectedGrid)
    {
        var indices = UKBingoCipher.GetMarkedIndicesInCallOrder(calledNumbers, winningTicket);
        var grid = UKBingoCipher.CreateGrid(plaintext, calledNumbers, winningTicket, deterministicFiller: true, fillerRandom: null);

        Assert.Equal(expectedIndices, indices);
        Assert.Equal(expectedGrid, grid);
    }

    [Theory]
    [MemberData(nameof(UkCipherCases))]
    public void PlaintextLengthMustMatchAllMarkedNumberedSpaces(
        string plaintext,
        int[] winningTicket,
        int[] calledNumbers)
    {
        // Should throw when the last character is taken away.
        //   ( ..^1 range in index culls last character.)
        Assert.Throws<ArgumentException>(() =>
            UKBingoCipher.CreateGrid(plaintext[..^1], calledNumbers, winningTicket, deterministicFiller: true, fillerRandom: null));
        // Should throw when extra character is appended.
        Assert.Throws<ArgumentException>(() =>
            UKBingoCipher.CreateGrid(plaintext + "X", calledNumbers, winningTicket, deterministicFiller: true, fillerRandom: null));
    }

    [Theory]
    [MemberData(nameof(UkCipherCases))]
    public void SeededFillerIsReproducibleAndAsciiPrintable(
        string plaintext,
        int[] winningTicket,
        int[] calledNumbers)
    {
        var first = UKBingoCipher.CreateGrid(
            plaintext, calledNumbers, winningTicket, deterministicFiller: false, SeededChallengeRandom.CreateUkFiller(42));
        var second = UKBingoCipher.CreateGrid(
            plaintext, calledNumbers, winningTicket, deterministicFiller: false, SeededChallengeRandom.CreateUkFiller(42));
        var markedIndices = UKBingoCipher.GetMarkedIndicesInCallOrder(calledNumbers, winningTicket).ToHashSet();

        Assert.Equal(first, second);  // seeded PRNG should deliver repeatable results
        Assert.All(first.Select((character, index) => (character, index)).Where(cell => !markedIndices.Contains(cell.index)),
            cell => Assert.InRange(cell.character, ' ', '~'));  // range of printable ASCII characters

        var expectedFiller = SeededChallengeRandom.CreateUkFiller(42);
        Assert.Equal(
            Enumerable.Range(
                0,
                UKBingoTicket.CellCount - markedIndices.Count
                ).Select(
                    _ => (char)expectedFiller.Next(32, 127)
                ),
                first.Select(
                    (character, index) => (character, index)
                ).Where(
                    cell => !markedIndices.Contains(cell.index)
                ).Select(
                    cell => cell.character
                )
            );
    }

    [Theory]
    [MemberData(nameof(UkCipherEncodingCases))]
    public void ChaCha20RoundTripSupportsUkCiphertextEncodings(
        string plaintext,
        int[] winningTicket,
        int[] calledNumbers,
        CiphertextEncoding encoding)
    {
        var ciphertext = UKBingoCipher.Encrypt(
            plaintext, calledNumbers, winningTicket, deterministicFiller: false,
            SeededChallengeRandom.CreateUkFiller(1776), encoding);

        var recovered = UKBingoCipher.Decrypt(ciphertext, encoding, calledNumbers, winningTicket);

        Assert.Equal(plaintext, recovered);
        if (encoding == CiphertextEncoding.Hex)
        {
            Assert.Equal(54, ciphertext.Length);
            Assert.All(ciphertext, character => Assert.True(char.IsAsciiHexDigit(character)));
        }
        else
        {
            Assert.Equal(36, ciphertext.Length);
            Assert.Equal(27, Convert.FromBase64String(ciphertext).Length);
        }
    }

    [Theory]
    [MemberData(nameof(UkCipherCases))]
    public void TicketMarksMustAgreeWithCalledNumbers(
        string plaintext,
        int[] winningTicket,
        int[] calledNumbers)
    {
        _ = plaintext;

        Assert.Throws<ArgumentException>(() =>
            UKBingoCipher.GetMarkedIndicesInCallOrder(calledNumbers[..^1], winningTicket));
    }

    [Theory]
    [MemberData(nameof(UkCipherCases))]
    public void WinnerSelectionUsesFirstSimulatorWinnerDeterministically(
        string plaintext,
        int[] winningTicket,
        int[] calledNumbers)
    {
        _ = plaintext;
        _ = calledNumbers;

        var first = new UKBingoTicket(winningTicket);
        var winningRow = Enumerable.Range(0, UKBingoTicket.RowCount)
            .First(row => winningTicket.Skip(row * UKBingoTicket.ColumnCount)
                .Take(UKBingoTicket.ColumnCount)
                .Where(value => value != 0)
                .All(value => value < 0));
        var secondValues = winningTicket
            .Select((value, index) => value < 0 && index / UKBingoTicket.ColumnCount != winningRow ? Math.Abs(value) : value)
            .ToArray();
        var second = new UKBingoTicket(secondValues);

        var selected = UKChallengeGenerator.SelectWinningTicket([first, second]);

        Assert.Equal(first.Values, selected.Values);
        Assert.NotSame(first, selected);
    }
}
