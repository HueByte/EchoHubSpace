using System.Buffers.Binary;
using System.Security.Cryptography;
using EchoHub.Core.Interfaces;
using EchoHub.Core.Models;
using Microsoft.Extensions.Options;

namespace EchoHub.Core.Services;

/// <summary>
/// HMAC-SHA256 implementation of <see cref="IClaimTokenService"/>.
/// Token wire format: <c>base64url(payload).base64url(signature)</c> where
/// <c>payload = serverId (16 bytes) || issuedAtUnixSeconds (8 bytes, big-endian)</c>
/// and <c>signature = HMAC-SHA256(signingKey, payload)</c> (32 bytes).
/// </summary>
public class ClaimTokenService : IClaimTokenService
{
    private const int PayloadLength = 24; // 16 (Guid) + 8 (long)
    private const int SignatureLength = 32; // HMAC-SHA256

    private readonly byte[] _signingKey;

    public ClaimTokenService(IOptions<ClaimOptions> options)
    {
        _signingKey = Convert.FromBase64String(options.Value.SigningKey);
    }

    /// <inheritdoc />
    public string Issue(Guid serverId)
    {
        // SECURITY: the raw token leaves this method exactly once, returned to the caller.
        // Do not log, cache, or copy it anywhere else.
        var payload = new byte[PayloadLength];
        if (!serverId.TryWriteBytes(payload.AsSpan(0, 16)))
            throw new InvalidOperationException("Failed to write server id bytes.");
        BinaryPrimitives.WriteInt64BigEndian(payload.AsSpan(16, 8), DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        var signature = HMACSHA256.HashData(_signingKey, payload);
        return $"{Base64UrlEncode(payload)}.{Base64UrlEncode(signature)}";
    }

    /// <inheritdoc />
    public bool TryVerify(string token, out Guid serverId)
    {
        serverId = Guid.Empty;
        if (string.IsNullOrWhiteSpace(token)) return false;

        var dotIndex = token.IndexOf('.');
        if (dotIndex <= 0 || dotIndex >= token.Length - 1) return false;

        byte[] payload, signature;
        try
        {
            payload = Base64UrlDecode(token[..dotIndex]);
            signature = Base64UrlDecode(token[(dotIndex + 1)..]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (payload.Length != PayloadLength || signature.Length != SignatureLength)
            return false;

        var expected = HMACSHA256.HashData(_signingKey, payload);
        if (!CryptographicOperations.FixedTimeEquals(expected, signature))
            return false;

        serverId = new Guid(payload.AsSpan(0, 16));
        return true;
    }

    private static string Base64UrlEncode(byte[] data) =>
        Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(ReadOnlySpan<char> input)
    {
        var padded = new string(input).Replace('-', '+').Replace('_', '/');
        var padLen = (4 - padded.Length % 4) % 4;
        if (padLen > 0) padded += new string('=', padLen);
        return Convert.FromBase64String(padded);
    }
}
