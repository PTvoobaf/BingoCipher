using System.Text.Json;

namespace BingoCipher0.Core;

/// <summary>Represents a six-ticket UK bingo strip containing every number from 1 through 90 once.</summary>
public sealed class UKBingoStrip
{
    public const int TicketCount = 6;

    private readonly List<UKBingoTicket> tickets;

    /// <summary>Creates a randomly generated valid strip.</summary>
    public UKBingoStrip()
        : this(Random.Shared)
    {
    }

    /// <summary>Creates a valid strip using the supplied random source.</summary>
    public UKBingoStrip(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        tickets = GenerateTickets(random);
    }

    private UKBingoStrip(IEnumerable<UKBingoTicket> tickets)
    {
        this.tickets = ValidateAndCopy(tickets);
    }

    /// <summary>Gets the six tickets in strip order.</summary>
    public IReadOnlyList<UKBingoTicket> Tickets => tickets.AsReadOnly();

    /// <summary>Formats the six tickets as newline-separated three-line blocks.</summary>
    public override string ToString() => string.Join(Environment.NewLine, tickets) ;

    /// <summary>Marks a number on the ticket containing it.</summary>
    /// <returns>True when an unmarked number was found and marked; otherwise false.</returns>
    public bool MarkNumber(int number)
    {
        foreach (var ticket in tickets)
        {
            if (ticket.MarkNumber(number))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Serializes the strip as a JSON array of six ticket arrays.</summary>
    public string ToJson(JsonSerializerOptions? options = null) =>
        JsonSerializer.Serialize(tickets.Select(ticket => ticket.Values), options);

    /// <summary>Deserializes and validates a six-ticket strip JSON array.</summary>
    public static UKBingoStrip FromJson(string json, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(json);
        var values = JsonSerializer.Deserialize<List<List<int>>>(json, options)
            ?? throw new JsonException("A UK bingo strip JSON value must be an array.");

        try
        {
            return new UKBingoStrip(values.Select(ticketValues => new UKBingoTicket(ticketValues)));
        }
        catch (ArgumentException exception)
        {
            throw new JsonException("The JSON does not contain a valid UK bingo strip.", exception);
        }
    }

    private static List<UKBingoTicket> GenerateTickets(Random random)
    {
        var occupiedCoordinates = GenerateOccupancy(random);
        var generatedTickets = Enumerable.Range(0, TicketCount)
            .Select(_ => new int[UKBingoTicket.CellCount])
            .ToArray();

        for (var column = 0; column < UKBingoTicket.ColumnCount; column++)
        {
            var columnNumbers = Enumerable.Range(column * 10 + 1, 10).ToArray();
            Shuffle(columnNumbers, random);
            var numberIndex = 0;

            for (var ticketIndex = 0; ticketIndex < TicketCount; ticketIndex++)
            {
                for (var row = 0; row < UKBingoTicket.RowCount; row++)
                {
                    if (!occupiedCoordinates[ticketIndex, row, column])
                    {
                        continue;
                    }

                    generatedTickets[ticketIndex][row * UKBingoTicket.ColumnCount + column] = columnNumbers[numberIndex++];
                }
            }
        }

        return generatedTickets.Select(values => new UKBingoTicket(values)).ToList();
    }

    private static bool[,,] GenerateOccupancy(Random random)
    {
        while (true)
        {
            var columnTokens = Enumerable.Range(0, UKBingoTicket.ColumnCount)
                .SelectMany(column => Enumerable.Repeat(column, 4))
                .ToList();
            Shuffle(columnTokens, random);
            var numbersPerColumn = new int[TicketCount, UKBingoTicket.ColumnCount];

            for (var ticket = 0; ticket < TicketCount; ticket++)
            {
                foreach (var column in columnTokens.Skip(ticket * 6).Take(6))
                {
                    numbersPerColumn[ticket, column]++;
                }
            }

            if (Enumerable.Range(0, TicketCount)
                .SelectMany(ticket => Enumerable.Range(0, UKBingoTicket.ColumnCount)
                    .Select(column => numbersPerColumn[ticket, column]))
                .Any(extra => extra > 2))
            {
                continue;
            }

            for (var ticket = 0; ticket < TicketCount; ticket++)
            {
                for (var column = 0; column < UKBingoTicket.ColumnCount; column++)
                {
                    numbersPerColumn[ticket, column]++;
                }
            }

            var occupancy = new bool[TicketCount, UKBingoTicket.RowCount, UKBingoTicket.ColumnCount];
            var validRowAssignments = true;
            for (var ticket = 0; ticket < TicketCount; ticket++)
            {
                var requiredNumbersPerRow = new[] { 5, 5, 5 };
                var columnOrder = Enumerable.Range(0, UKBingoTicket.ColumnCount).ToArray();
                Shuffle(columnOrder, random);
                if (!AssignColumnsToRows(0, columnOrder, numbersPerColumn, requiredNumbersPerRow, occupancy, ticket, random))
                {
                    validRowAssignments = false;
                    break;
                }
            }

            if (validRowAssignments) return occupancy;
        }
    }

    private static bool AssignColumnsToRows(
        int columnOrderIndex,
        int[] columnOrder,
        int[,] numbersPerColumn,
        int[] remainingNumbersPerRow,
        bool[,,] occupancy,
        int ticket,
        Random random)
    {
        if (columnOrderIndex == columnOrder.Length)
        {
            return remainingNumbersPerRow.All(count => count == 0);
        }

        var column = columnOrder[columnOrderIndex];
        var numbersInColumn = numbersPerColumn[ticket, column];
        var rowSelections = Enumerable.Range(0, UKBingoTicket.RowCount)
            .Where(row => remainingNumbersPerRow[row] > 0)
            .ToArray();

        var rowCombinations = Combinations(rowSelections, numbersInColumn).ToList();
        Shuffle(rowCombinations, random);
        foreach (var selectedRows in rowCombinations)
        {
            foreach (var row in selectedRows)
            {
                remainingNumbersPerRow[row]--;
                occupancy[ticket, row, column] = true;
            }

            if (AssignColumnsToRows(columnOrderIndex + 1, columnOrder, numbersPerColumn, remainingNumbersPerRow, occupancy, ticket, random))
            {
                return true;
            }

            foreach (var row in selectedRows)
            {
                remainingNumbersPerRow[row]++;
                occupancy[ticket, row, column] = false;
            }
        }

        return false;
    }

    private static IEnumerable<int[]> Combinations(int[] values, int length)
    {
        if (length == 0)
        {
            yield return [];
            yield break;
        }

        for (var index = 0; index <= values.Length - length; index++)
        {
            foreach (var remainder in Combinations(values[(index + 1)..], length - 1))
            {
                yield return [values[index], .. remainder];
            }
        }
    }

    private static void Shuffle<T>(IList<T> values, Random random)
    {
        for (var index = values.Count - 1; index > 0; index--)
        {
            var swapIndex = random.Next(index + 1);
            (values[index], values[swapIndex]) = (values[swapIndex], values[index]);
        }
    }

    private static List<UKBingoTicket> ValidateAndCopy(IEnumerable<UKBingoTicket> sourceTickets)
    {
        var result = sourceTickets.ToList();
        if (result.Count != TicketCount)
        {
            throw new ArgumentException($"A UK bingo strip must contain exactly {TicketCount} tickets.", nameof(sourceTickets));
        }

        var numbers = result.SelectMany(ticket => ticket.Values)
            .Where(value => value != 0)
            .Select(Math.Abs)
            .ToList();
        if (numbers.Count != 90 || numbers.Distinct().Count() != 90 || !numbers.OrderBy(number => number).SequenceEqual(Enumerable.Range(1, 90)))
        {
            throw new ArgumentException("A UK bingo strip must contain every number from 1 through 90 exactly once.", nameof(sourceTickets));
        }

        return result;
    }
}
