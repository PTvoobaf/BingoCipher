using System.Text.Json;

namespace BingoCipher0.Core;

/// <summary>Represents a five-by-five US bingo card with a permanently free center space.</summary>
public sealed class USBingoCard
{
    public const int RowCount = 5;
    public const int ColumnCount = 5;
    public const int CellCount = RowCount * ColumnCount;
    public const int FreeSpaceIndex = 12;

    private readonly List<int> cells;

    /// <summary>Creates a randomly generated valid card.</summary>
    public USBingoCard()
        : this(Random.Shared)
    {
    }

    /// <summary>Creates a valid card using the supplied random source.</summary>
    public USBingoCard(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        cells = GenerateCells(random);
    }

    /// <summary>Creates and validates a card from row-major cell values.</summary>
    public USBingoCard(IEnumerable<int> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        cells = ValidateAndCopy(values);
    }

    /// <summary>Gets the card cells in row-major order.</summary>
    public IReadOnlyList<int> Values => cells.AsReadOnly();

    /// <summary>Formats the card as a header and five newline-terminated rows of four-character fields.</summary>
    public override string ToString()
    {
        var lines = new List<string> { "   B   I   N   G   O" };
        lines.AddRange(Enumerable.Range(0, RowCount)
            .Select(row => string.Concat(cells.Skip(row * ColumnCount).Take(ColumnCount).Select(FormatCell))));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    /// <summary>Replaces this card with validated row-major values.</summary>
    public void Initialize(IEnumerable<int> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var validatedValues = ValidateAndCopy(values);
        cells.Clear();
        cells.AddRange(validatedValues);
    }

    /// <summary>Marks a positive occurrence of <paramref name="number"/>.</summary>
    /// <returns>True when an unmarked number was found and marked; otherwise false.</returns>
    public bool MarkNumber(int number)
    {
        if (number is < 1 or > 75)
        {
            return false;
        }

        var index = cells.IndexOf(number);
        if (index < 0)
        {
            return false;
        }

        cells[index] = -number;
        return true;
    }

    /// <summary>Gets whether the card has a marked row, column, or diagonal.</summary>
    public bool IsWinner()
    {
        for (var row = 0; row < RowCount; row++)
        {
            if (Enumerable.Range(0, ColumnCount).All(column => cells[row * ColumnCount + column] <= 0))
            {
                return true;
            }
        }

        for (var column = 0; column < ColumnCount; column++)
        {
            if (Enumerable.Range(0, RowCount).All(row => cells[row * ColumnCount + column] <= 0))
            {
                return true;
            }
        }

        return Enumerable.Range(0, RowCount).All(index => cells[index * ColumnCount + index] <= 0)
            || Enumerable.Range(0, RowCount).All(index => cells[index * ColumnCount + (ColumnCount - 1 - index)] <= 0);
    }

    /// <summary>Serializes this card as its JSON array of 25 integers.</summary>
    public string ToJson(JsonSerializerOptions? options = null) =>
        JsonSerializer.Serialize(cells, options);

    /// <summary>Deserializes and validates a card JSON array.</summary>
    public static USBingoCard FromJson(string json, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(json);
        var values = JsonSerializer.Deserialize<List<int>>(json, options)
            ?? throw new JsonException("A US bingo card JSON value must be an array.");
        try
        {
            return new USBingoCard(values);
        }
        catch (ArgumentException exception)
        {
            throw new JsonException("The JSON does not contain a valid US bingo card.", exception);
        }
    }

    private static List<int> GenerateCells(Random random)
    {
        var generatedCells = new int[CellCount];
        for (var column = 0; column < ColumnCount; column++)
        {
            var minimum = column * 15 + 1;
            var numbers = Enumerable.Range(minimum, 15).ToArray();
            Shuffle(numbers, random);
            for (var row = 0; row < RowCount; row++)
            {
                generatedCells[row * ColumnCount + column] = numbers[row];
            }
        }

        generatedCells[FreeSpaceIndex] = 0;
        return generatedCells.ToList();
    }

    private static void Shuffle<T>(IList<T> values, Random random)
    {
        for (var index = values.Count - 1; index > 0; index--)
        {
            var swapIndex = random.Next(index + 1);
            (values[index], values[swapIndex]) = (values[swapIndex], values[index]);
        }
    }

    private static List<int> ValidateAndCopy(IEnumerable<int> values)
    {
        var result = values.ToList();
        if (result.Count != CellCount)
        {
            throw new ArgumentException($"A US bingo card must contain exactly {CellCount} values.", nameof(values));
        }

        if (result[FreeSpaceIndex] != 0)
        {
            throw new ArgumentException("The center space of a US bingo card must be zero.", nameof(values));
        }

        var seenNumbers = new HashSet<int>();
        for (var index = 0; index < result.Count; index++)
        {
            var value = result[index];
            if (index == FreeSpaceIndex)
            {
                continue;
            }

            var number = Math.Abs(value);
            var column = index % ColumnCount;
            var minimum = column * 15 + 1;
            var maximum = minimum + 14;
            if (value == 0 || number < minimum || number > maximum)
            {
                throw new ArgumentException($"Number {value} at index {index} is outside its column range.", nameof(values));
            }

            if (!seenNumbers.Add(number))
            {
                throw new ArgumentException($"Number {number} occurs more than once.", nameof(values));
            }
        }

        return result;
    }

    private static string FormatCell(int value) => value.ToString().PadLeft(4);
}
