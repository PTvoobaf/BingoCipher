using System.Text.Json;

namespace BingoCipher0.Core;

/// <summary>Represents a three-by-nine UK bingo ticket.</summary>
public sealed class UKBingoTicket
{
    public const int RowCount = 3;
    public const int ColumnCount = 9;
    public const int CellCount = RowCount * ColumnCount;

    private readonly List<int> cells;

    /// <summary>Creates an empty ticket whose 27 coordinates are all zero.</summary>
    public UKBingoTicket()
    {
        cells = Enumerable.Repeat(0, CellCount).ToList();
    }

    /// <summary>Creates and validates a ticket from row-major cell values.</summary>
    public UKBingoTicket(IEnumerable<int> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        cells = ValidateAndCopy(values);
    }

    /// <summary>Gets the ticket cells in row-major order.</summary>
    public IReadOnlyList<int> Values => cells.AsReadOnly();

    /// <summary>Formats the ticket as three newline-terminated rows of four-character fields.</summary>
    public override string ToString()
    {
        var rows = Enumerable.Range(0, RowCount)
            .Select(row => string.Concat(cells.Skip(row * ColumnCount).Take(ColumnCount).Select(FormatCell)));
        return string.Join(Environment.NewLine, rows) + Environment.NewLine;
    }

    /// <summary>Replaces this ticket with validated row-major values.</summary>
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
        if (number is < 1 or > 90)
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

    /// <summary>Gets whether any row has all five of its numbered spaces marked.</summary>
    public bool IsLineWinner()
    {
        for (var row = 0; row < RowCount; row++)
        {
            var markedNumbers = 0;
            for (var column = 0; column < ColumnCount; column++)
            {
                if (cells[row * ColumnCount + column] < 0)
                {
                    markedNumbers++;
                }
            }

            if (markedNumbers == 5)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Serializes this ticket as its JSON array of 27 integers.</summary>
    public string ToJson(JsonSerializerOptions? options = null) =>
        JsonSerializer.Serialize(cells, options);

    /// <summary>Deserializes and validates a ticket JSON array.</summary>
    public static UKBingoTicket FromJson(string json, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(json);
        var values = JsonSerializer.Deserialize<List<int>>(json, options)
            ?? throw new JsonException("A UK bingo ticket JSON value must be an array.");
        return new UKBingoTicket(values);
    }

    private static List<int> ValidateAndCopy(IEnumerable<int> values)
    {
        var result = values.ToList();
        if (result.Count != CellCount)
        {
            throw new ArgumentException($"A UK bingo ticket must contain exactly {CellCount} values.", nameof(values));
        }

        var seenNumbers = new HashSet<int>();
        for (var index = 0; index < result.Count; index++)
        {
            var value = result[index];
            if (value == 0)
            {
                continue;
            }

            var number = Math.Abs(value);
            var column = index % ColumnCount;
            var minimum = column * 10 + 1;
            var maximum = column == ColumnCount - 1 ? 90 : minimum + 9;
            if (number is < 1 or > 90 || number < minimum || number > maximum)
            {
                throw new ArgumentException($"Number {value} at index {index} is outside its column range.", nameof(values));
            }

            if (!seenNumbers.Add(number))
            {
                throw new ArgumentException($"Number {number} occurs more than once.", nameof(values));
            }
        }

        for (var row = 0; row < RowCount; row++)
        {
            var rowStart = row * ColumnCount;
            var numbersInRow = result.Skip(rowStart).Take(ColumnCount).Count(value => value != 0);
            if (numbersInRow != 5)
            {
                throw new ArgumentException($"Row {row} must contain exactly five numbers.", nameof(values));
            }
        }

        for (var column = 0; column < ColumnCount; column++)
        {
            if (result[column] == 0 && result[ColumnCount + column] == 0 && result[2 * ColumnCount + column] == 0)
            {
                throw new ArgumentException($"Column {column} must contain at least one number.", nameof(values));
            }
        }

        return result;
    }

    private static string FormatCell(int value) => value == 0 ? "    " : value.ToString().PadLeft(4);
}
