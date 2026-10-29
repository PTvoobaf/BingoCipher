using BingoCipher0.Challenges;
using BingoCipher0.Core;

namespace BingoCipher0.Challenges.Tests;

public sealed class USBingoCipherTests
{
    private static IEnumerable<(string Plaintext, int[] WinningTicket, int[] CalledNumbers, int[] ExpectedIndices, string ExpectedGrid)> TestCases()
    {
        yield return
        (
            "Hulk",
            new[]
            {
                -1, 16, 31, 46, 61,
                2, -17, 32, 47, 62,
                3, 18, 0, 48, 63,
                4, 19, 34, -49, 64,
                5, 20, 35, 50, -65
            }, // four numbers daubed, main diagonal is winner through the free space
            new[] { 1, 17, 49, 65 },
            new[] { 0, 6, 18, 24 },
            "UBBBB" + "BhBBB" + "BBBBB" + "BBByB" + "BBBBx"
        );

        yield return
        (
            "ABCDE",
            new[]
            {
                1, 16, 31, 46, 61,
                2, 17, 32, 47, 62,
                -3, -18, 0, -48, -63,
                -4, 19, 34, 49, 64,
                5, 20, 35, 50, 65
            }, // five numbers daubed, middle row is winner through the free space
            new[] { 3, 4, 18, 48, 63 },
            new[] { 10, 15, 11, 13, 14 },
            "BBBBB" + "BBBBB" + "NPBQR" + "OBBBB" + "BBBBB"
        );
    }

    public static IEnumerable<object[]> UsCipherCases()
    {
        foreach (var testCase in TestCases())
        {
            yield return [testCase.Plaintext, testCase.WinningTicket, testCase.CalledNumbers];
        }
    }

    public static IEnumerable<object[]> UsGridPlacementCases()
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

    [Theory]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "NOPQRSTUVWXYZABCDEFGHIJKLM")]
    [InlineData("abcdefghijklmnopqrstuvwxyz", "nopqrstuvwxyzabcdefghijklm")]
    [InlineData("Hulk", "Uhyx")]
    public void Rot13PreservesCaseAndIsSelfInverse(string plaintext, string rotated)
    {
        Assert.Equal(rotated, USBingoCipher.Rot13(plaintext));
        Assert.Equal(plaintext, USBingoCipher.Rot13(rotated));
    }

    [Theory]
    [MemberData(nameof(UsGridPlacementCases))]
    public void WinningCardExtractionAndDeterministicGridFollowCallOrder(
        string plaintext,
        int[] winningTicket,
        int[] calledNumbers,
        int[] expectedIndices,
        string expectedGrid)
    {
        var indices = USBingoCipher.GetMarkedIndicesInCallOrder(calledNumbers, winningTicket);
        var grid = USBingoCipher.CreateGrid(plaintext, calledNumbers, winningTicket, deterministicFiller: true, fillerRandom: null);

        Assert.Equal(expectedIndices, indices);
        Assert.Equal(expectedGrid, grid);
        Assert.Equal('B', grid[USBingoCard.FreeSpaceIndex]);
    }

    [Theory]
    [MemberData(nameof(UsCipherCases))]
    public void PlaintextLengthMustMatchAllMarkedNumberedSpaces(string plaintext, int[] winningTicket, int[] calledNumbers)
    {
        Assert.Throws<ArgumentException>(() =>
            USBingoCipher.CreateGrid(plaintext[..^1], calledNumbers, winningTicket, deterministicFiller: true, fillerRandom: null));
        Assert.Throws<ArgumentException>(() =>
            USBingoCipher.CreateGrid(plaintext + "X", calledNumbers, winningTicket, deterministicFiller: true, fillerRandom: null));
    }

    [Theory]
    [MemberData(nameof(UsCipherCases))]
    public void SeededFillerIsReproducibleAndAlphabetic(string plaintext, int[] winningTicket, int[] calledNumbers)
    {
        var first = USBingoCipher.CreateGrid(
            plaintext, calledNumbers, winningTicket, deterministicFiller: false, SeededChallengeRandom.CreateUsFiller(42));
        var second = USBingoCipher.CreateGrid(
            plaintext, calledNumbers, winningTicket, deterministicFiller: false, SeededChallengeRandom.CreateUsFiller(42));
        var markedIndices = USBingoCipher.GetMarkedIndicesInCallOrder(calledNumbers, winningTicket).ToHashSet();

        Assert.Equal(first, second);
        Assert.All(
            first.Select(
                (character, index) => (character, index)
                    ).Where(
                        cell => !markedIndices.Contains(cell.index)
                    ),
                cell => Assert.True(char.IsAsciiLetter(cell.character))
            );

        var expectedFiller = SeededChallengeRandom.CreateUsFiller(42);
        Assert.Equal(
            Enumerable.Range(
                0,
                USBingoCard.CellCount - markedIndices.Count
                ).Select(
                    _ => NextAlphabeticCharacter(expectedFiller)
                ),
            first.Select(
                (character, index) => (character, index)
                ).Where(
                    cell => !markedIndices.Contains(cell.index)
                ).Select(cell => cell.character)
            );
    }

    [Theory]
    [MemberData(nameof(UsCipherCases))]
    public void CipherRoundTripRecoversPlaintext(string plaintext, int[] winningTicket, int[] calledNumbers)
    {
        var ciphertext = USBingoCipher.Encrypt(
            plaintext, calledNumbers, winningTicket, deterministicFiller: false, SeededChallengeRandom.CreateUsFiller(1776));

        var recovered = USBingoCipher.Decrypt(ciphertext, calledNumbers, winningTicket);

        Assert.Equal(USBingoCard.CellCount, ciphertext.Length);
        Assert.All(ciphertext, character => Assert.True(char.IsAsciiLetter(character)));
        Assert.Equal(plaintext, recovered);
    }

    [Theory]
    [MemberData(nameof(UsCipherCases))]
    public void CardMarksMustAgreeWithCalledNumbers(string plaintext, int[] winningTicket, int[] calledNumbers)
    {
        _ = plaintext;

        Assert.Throws<ArgumentException>(() =>
            USBingoCipher.GetMarkedIndicesInCallOrder(calledNumbers[..^1], winningTicket));
    }

    [Fact]
    public void RejectsNonAlphabeticPlaintextCiphertextAndRot13Input()
    {
        var testCase = TestCases().First();

        Assert.Throws<ArgumentException>(() => USBingoCipher.CreateGrid(
            "ABC!", testCase.CalledNumbers, testCase.WinningTicket, deterministicFiller: true, fillerRandom: null));
        Assert.Throws<ArgumentException>(() => USBingoCipher.Decrypt(
            new string('A', USBingoCard.CellCount - 1) + "!", testCase.CalledNumbers, testCase.WinningTicket));
        Assert.Throws<ArgumentException>(() => USBingoCipher.Rot13("ABC!"));
    }

    private static char NextAlphabeticCharacter(Random random)
    {
        var value = random.Next(52);
        return value < 26 ? (char)('A' + value) : (char)('a' + value - 26);
    }
}
