namespace EchoHub.Core.Interfaces;

/// <summary>
/// Issues and verifies HMAC-signed claim tokens. Stateless: no per-token server state is kept;
/// the token itself carries the server identity, the signing key carries trust.
/// </summary>
public interface IClaimTokenService
{
    /// <summary>
    /// Mints a fresh signed token that binds <paramref name="serverId"/>.
    /// Opaque to callers — clients must treat the returned string as bytes.
    /// </summary>
    string Issue(Guid serverId);

    /// <summary>
    /// Verifies the signature of <paramref name="token"/> and, if valid, extracts the embedded server id.
    /// </summary>
    /// <param name="token">The opaque claim token presented by the client.</param>
    /// <param name="serverId">The embedded server id when the signature is valid; otherwise <see cref="Guid.Empty"/>.</param>
    /// <returns><c>true</c> when the token's signature matches; otherwise <c>false</c>.</returns>
    bool TryVerify(string token, out Guid serverId);
}
