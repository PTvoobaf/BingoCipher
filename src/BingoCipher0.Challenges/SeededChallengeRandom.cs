using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace BingoCipher0.Challenges;

public static class SeededChallengeRandom
{
    public static Random Create(long seed, string purpose)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);

        Span<byte> seedBytes = stackalloc byte[8];
        BinaryPrimitives.WriteInt64LittleEndian(seedBytes, seed);
        var purposeBytes = Encoding.UTF8.GetBytes(purpose);
        var material = new byte[seedBytes.Length + purposeBytes.Length];
        seedBytes.CopyTo(material);
        purposeBytes.CopyTo(material, seedBytes.Length);
        var digest = SHA256.HashData(material);
        return new Random(BinaryPrimitives.ReadInt32LittleEndian(digest));
    }

    public static Random CreatePlaintextSelection(long seed) => Create(seed, "plaintext-selection");
    public static Random CreateUkFiller(long seed) => Create(seed, "uk-filler");
    public static Random CreateUsFiller(long seed) => Create(seed, "us-filler");
}
