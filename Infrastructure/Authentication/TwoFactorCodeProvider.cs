using System.Security.Cryptography;
using System.Text;
using Domain.Interfaces;

namespace Infrastructure.Authentication;

public class TwoFactorCodeProvider : ITwoFactorCodeProvider
{
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
    private const int SecretByteLength = 20;
    private const int CodeLength = 6;
    private const long StepSeconds = 30;
    private const string RecoveryCodeAlphabet = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";

    public string GenerateSecretBase32()
    {
        return EncodeBase32(RandomNumberGenerator.GetBytes(SecretByteLength));
    }

    public string BuildOtpAuthUri(string issuerName, string accountName, string secretBase32)
    {
        return $"otpauth://totp/{Uri.EscapeDataString($"{issuerName}:{accountName}")}" +
               $"?secret={secretBase32}" +
               $"&issuer={Uri.EscapeDataString(issuerName)}" +
               $"&algorithm=SHA1&digits={CodeLength}&period={StepSeconds}";
    }

    public string GenerateRecoveryCode()
    {
        // Формат "XXXX-XXXX"; в алфавите нет похожих символов (0/O, 1/I)
        var chars = new char[9];
        chars[4] = '-';

        for (var index = 0; index < chars.Length; index++)
        {
            if (chars[index] == '-')
            {
                continue;
            }

            chars[index] = RecoveryCodeAlphabet[RandomNumberGenerator.GetInt32(RecoveryCodeAlphabet.Length)];
        }

        return new string(chars);
    }

    public bool VerifyCode(string secretBase32, string code, long lastUsedStep, out long usedStep, long? currentDateStep = null)
    {
        usedStep = 0;
        code = code?.Trim() ?? string.Empty;

        if (code.Length != CodeLength || !code.All(char.IsAsciiDigit))
        {
            return false;
        }

        if (!TryDecodeBase32(secretBase32, out var secret))
        {
            return false;
        }

        var currentStep = currentDateStep ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds() / StepSeconds;

        for (var step = currentStep - 1; step <= currentStep + 1; step++)
        {
            // Один шаг не может обеспечить два входа (анти-replay)
            if (step <= lastUsedStep)
            {
                continue;
            }

            var expectedCode = ComputeCode(secret, step);

            if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.ASCII.GetBytes(expectedCode),
                    Encoding.ASCII.GetBytes(code)))
            {
                continue;
            }

            usedStep = step;
            return true;
        }

        return false;
    }

    private static string ComputeCode(byte[] secret, long step)
    {
        Span<byte> counter = stackalloc byte[8];
        var value = step;

        for (var index = 7; index >= 0; index--)
        {
            counter[index] = (byte)(value & 0xFF);
            value >>= 8;
        }

        using var hmac = new HMACSHA1(secret);
        var hash = hmac.ComputeHash(counter.ToArray());

        var offset = hash[^1] & 0x0F;
        var truncated = ((hash[offset] & 0x7F) << 24)
            | (hash[offset + 1] << 16)
            | (hash[offset + 2] << 8)
            | hash[offset + 3];

        return (truncated % 1_000_000).ToString($"D{CodeLength}");
    }

    private static string EncodeBase32(ReadOnlySpan<byte> data)
    {
        var output = new StringBuilder((data.Length * 8 + 4) / 5);
        int buffer = 0;
        int bitCount = 0;

        foreach (var byteValue in data)
        {
            buffer = (buffer << 8) | byteValue;
            bitCount += 8;

            while (bitCount >= 5)
            {
                output.Append(Base32Alphabet[(buffer >> (bitCount - 5)) & 0x1F]);
                bitCount -= 5;
            }
        }

        if (bitCount > 0)
        {
            output.Append(Base32Alphabet[(buffer << (5 - bitCount)) & 0x1F]);
        }

        return output.ToString();
    }

    private static bool TryDecodeBase32(string base32, out byte[] data)
    {
        data = Array.Empty<byte>();
        base32 = base32?.TrimEnd('=')?.Replace(" ", string.Empty).ToUpperInvariant() ?? string.Empty;

        if (base32.Length == 0)
        {
            return false;
        }

        int buffer = 0;
        int bitCount = 0;
        var output = new List<byte>(base32.Length * 5 / 8 + 1);

        foreach (var character in base32)
        {
            var index = Base32Alphabet.IndexOf(character);

            if (index < 0)
            {
                return false;
            }

            buffer = (buffer << 5) | index;
            bitCount += 5;

            if (bitCount >= 8)
            {
                output.Add((byte)((buffer >> (bitCount - 8)) & 0xFF));
                bitCount -= 8;
            }
        }

        if (output.Count != SecretByteLength)
        {
            return false;
        }

        data = output.ToArray();
        return true;
    }
}
