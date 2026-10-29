using CSChaCha20;

namespace BingoCipher0.Challenges;

public static class ChaCha20Cipher
{
    public static byte[] Encrypt(ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, uint initialCounter)
        => Transform(plaintext, key, nonce, initialCounter);

    public static byte[] Decrypt(ReadOnlySpan<byte> ciphertext, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, uint initialCounter)
        => Transform(ciphertext, key, nonce, initialCounter);

    private static byte[] Transform(ReadOnlySpan<byte> input, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, uint initialCounter)
    {
        if (key.Length != ChaCha20.allowedKeyLength)
        {
            throw new ArgumentException("ChaCha20 requires a 32-byte key.", nameof(key));
        }

        if (nonce.Length != ChaCha20.allowedNonceLength)
        {
            throw new ArgumentException("RFC 8439 ChaCha20 requires a 12-byte nonce.", nameof(nonce));
        }

        var output = new byte[input.Length];
        using var cipher = new ChaCha20(key, nonce, initialCounter);
        cipher.EncryptBytes(output, input.ToArray());
        return output;
    }
}
