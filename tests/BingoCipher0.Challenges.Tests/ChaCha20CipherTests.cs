using System.Text;
using BingoCipher0.Challenges;

namespace BingoCipher0.Challenges.Tests;

public sealed class ChaCha20CipherTests
{
    [Fact]
    public void EncryptMatchesRfc8439Section232Vector()
    {
        var key = Convert.FromHexString("000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f");
        var nonce = Convert.FromHexString("000000090000004a00000000");
        var expected = Convert.FromHexString("10f1e7e4d13b5915500fdd1fa32071c4c7d1f4c733c068030422aa9ac3d46c4ed2826446079faa0914c2d705d98b02a2b5129cd1de164eb9cbd083e8a2503c4e");

        var actual = ChaCha20Cipher.Encrypt(new byte[64], key, nonce, 1);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void BingoCipher0ParametersRoundTripAsciiPayload()
    {
        var key = Convert.FromHexString("23f99a75efdafdd8ac728bdeaf2feda1d6b2fc8fdfa2e9ddfdafdca26a6eb5ea");
        var nonce = Convert.FromHexString("b5e95e7289a6ba789c6ada2b");
        var plaintext = Encoding.ASCII.GetBytes("BingoCipher0 ChaCha20 verification payload");

        var ciphertext = ChaCha20Cipher.Encrypt(plaintext, key, nonce, 1);
        var recovered = ChaCha20Cipher.Decrypt(ciphertext, key, nonce, 1);

        Assert.Equal(plaintext, recovered);
        Assert.Equal(
            "64ac465bfe16081a096cf1bdc007db45a10b5651e561db0bc0b277a2be57391825d3b799b71bd2ef46ce",
            Convert.ToHexString(ciphertext).ToLowerInvariant());
    }

    [Fact]
    public void RejectsInvalidKeyAndNonceLengths()
    {
        Assert.Throws<ArgumentException>(() => ChaCha20Cipher.Encrypt([], new byte[31], new byte[12], 1));
        Assert.Throws<ArgumentException>(() => ChaCha20Cipher.Encrypt([], new byte[32], new byte[11], 1));
    }
}
