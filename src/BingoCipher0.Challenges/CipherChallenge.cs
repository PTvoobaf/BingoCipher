using System.Text.Json;
using System.Text.Json.Serialization;

namespace BingoCipher0.Challenges;

public enum CiphertextEncoding
{
    Hex,
    Base64,
    Alphabetic
}

public sealed class CipherChallenge
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [JsonConstructor]
    public CipherChallenge(
        int schemaVersion,
        string challengeType, // UK or US
        string mode,          // decode or encode
        string challengeId,
        long seed,
        string plaintext,
        string ciphertext,
        IReadOnlyList<int> calledNumbers,
        IReadOnlyList<int> winningTicket,
        CiphertextEncoding ciphertextEncoding)
    {
        SchemaVersion = schemaVersion;
        ChallengeType = challengeType ?? throw new ArgumentNullException(nameof(challengeType));
        Mode = mode ?? throw new ArgumentNullException(nameof(mode));
        ChallengeId = challengeId ?? throw new ArgumentNullException(nameof(challengeId));
        Plaintext = plaintext ?? throw new ArgumentNullException(nameof(plaintext));
        Ciphertext = ciphertext ?? throw new ArgumentNullException(nameof(ciphertext));
        CalledNumbers = calledNumbers?.ToArray() ?? throw new ArgumentNullException(nameof(calledNumbers));
        WinningTicket = winningTicket?.ToArray() ?? throw new ArgumentNullException(nameof(winningTicket));
        CiphertextEncoding = ciphertextEncoding;
        Seed = seed;

        Validate();
    }

    public int SchemaVersion { get; }
    public string ChallengeType { get; }
    public string Mode { get; }
    public string ChallengeId { get; }
    public long Seed { get; }
    public string Plaintext { get; }
    public string Ciphertext { get; }
    public IReadOnlyList<int> CalledNumbers { get; }
    public IReadOnlyList<int> WinningTicket { get; }
    public CiphertextEncoding CiphertextEncoding { get; }

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    public static CipherChallenge FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<CipherChallenge>(json, JsonOptions)
            ?? throw new JsonException("Challenge JSON must contain an object.");
    }

    private void Validate()
    {
        if (SchemaVersion != 1) throw new ArgumentException("Unsupported challenge schema version.", nameof(SchemaVersion));
        if (ChallengeType is not ("UK" or "US")) throw new ArgumentException("Challenge type must be UK or US.", nameof(ChallengeType));
        if (Mode is not ("encode" or "decode")) throw new ArgumentException("Challenge mode must be encode or decode.", nameof(Mode));
        if (string.IsNullOrWhiteSpace(ChallengeId)) throw new ArgumentException("Challenge ID must not be empty.", nameof(ChallengeId));
        if (CalledNumbers.Count == 0) throw new ArgumentException("Called numbers must not be empty.", nameof(CalledNumbers));

        var expectedTicketSize = ChallengeType == "UK" ? 27 : 25;
        if (WinningTicket.Count != expectedTicketSize)
        {
            throw new ArgumentException($"{ChallengeType} winningTicket must contain {expectedTicketSize} values.", nameof(WinningTicket));
        }

        var markedCount = WinningTicket.Count(value => value < 0);
        if (Plaintext.Length != markedCount)
        {
            throw new ArgumentException("Plaintext length must equal the number of marked numbered spaces.", nameof(Plaintext));
        }

        if (ChallengeType == "UK")
        {
            if (CiphertextEncoding is not (CiphertextEncoding.Hex or CiphertextEncoding.Base64))
                throw new ArgumentException("UK ciphertext encoding must be Hex or Base64.", nameof(CiphertextEncoding));
            if (Plaintext.Any(character => character is < (char)32 or > (char)126))
                throw new ArgumentException("UK plaintext must contain ASCII printable characters.", nameof(Plaintext));
        }
        else
        {
            if (CiphertextEncoding != CiphertextEncoding.Alphabetic)
                throw new ArgumentException("US ciphertext encoding must be Alphabetic.", nameof(CiphertextEncoding));
            if (Plaintext.Any(character => character is < 'A' or > 'Z' and < 'a' or > 'z'))
                throw new ArgumentException("US plaintext must contain alphabetic ASCII characters.", nameof(Plaintext));
        }
    }
}
