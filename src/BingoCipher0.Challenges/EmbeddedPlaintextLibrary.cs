using System.Reflection;
using System.Text.Json;

namespace BingoCipher0.Challenges;

public sealed class EmbeddedPlaintextLibrary
{
    private readonly IReadOnlyDictionary<int, IReadOnlyList<string>> messagesByLength;

    private EmbeddedPlaintextLibrary(IReadOnlyDictionary<int, IReadOnlyList<string>> messagesByLength)
    {
        this.messagesByLength = messagesByLength;
    }

    public IReadOnlyDictionary<int, IReadOnlyList<string>> MessagesByLength => messagesByLength;

    public string Select(int length, Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        if (!messagesByLength.TryGetValue(length, out var messages))
            throw new ArgumentOutOfRangeException(nameof(length), $"No plaintext messages are available for length {length}.");

        return messages[random.Next(messages.Count)];
    }

    public static EmbeddedPlaintextLibrary LoadUk() => Load("BingoCipher0.Challenges.Resources.UkPlaintextLibrary.json", IsUkCandidate, 5, 15);

    public static EmbeddedPlaintextLibrary LoadUs() => Load("BingoCipher0.Challenges.Resources.UsPlaintextLibrary.json", IsUsCandidate, 4, 20);

    private static EmbeddedPlaintextLibrary Load(string resourceName, Func<string, bool> candidateValidator, int minimumLength, int maximumLength)
    {
        using var stream = typeof(EmbeddedPlaintextLibrary).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded plaintext resource not found: {resourceName}.");
        var buckets = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(stream)
            ?? throw new InvalidDataException("Plaintext resource must contain an object of length buckets.");

        var result = new Dictionary<int, IReadOnlyList<string>>();
        for (var length = minimumLength; length <= maximumLength; length++)
        {
            if (!buckets.TryGetValue(length.ToString(), out var candidates))
                throw new InvalidDataException($"Plaintext resource is missing length bucket {length}.");
            if (candidates.Count is < 1 or > 19)
                throw new InvalidDataException($"Plaintext length bucket {length} must contain 1 through 19 candidates.");
            if (candidates.Any(candidate => candidate is null || candidate.Length != length || !candidateValidator(candidate)))
                throw new InvalidDataException($"Plaintext length bucket {length} contains an invalid candidate.");
            if (candidates.Distinct(StringComparer.Ordinal).Count() != candidates.Count)
                throw new InvalidDataException($"Plaintext length bucket {length} contains duplicate candidates.");

            result.Add(length, candidates.AsReadOnly());
        }

        if (buckets.Keys.Any(key => !int.TryParse(key, out var length) || length < minimumLength || length > maximumLength))
            throw new InvalidDataException("Plaintext resource contains an unsupported length bucket.");

        return new EmbeddedPlaintextLibrary(result);
    }

    private static bool IsUkCandidate(string candidate) => candidate.All(character => character is >= (char)32 and <= (char)126);

    private static bool IsUsCandidate(string candidate) => candidate.All(character => character is >= 'A' and <= 'Z' or >= 'a' and <= 'z');
}
