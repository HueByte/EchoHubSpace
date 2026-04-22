namespace EchoHub.Core.Models;

/// <summary>
/// Configuration for the claim-token signing subsystem.
/// Bound from the <c>Claim</c> section of configuration.
/// </summary>
public class ClaimOptions
{
    public const string SectionName = "Claim";

    /// <summary>
    /// Base64-encoded secret used to HMAC-sign claim tokens. Must decode to at least 32 bytes.
    /// Must survive API restarts — rotate only when invalidating all existing claims is intended.
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;
}
