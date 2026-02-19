namespace EchoHub.App.DTOs;

public record ServerDto(
    Guid Id,
    string Name,
    string? Description,
    string Host,
    int Port,
    int UserCount,
    bool IsOnline,
    DateTime CreatedAt
);

public record CreateServerDto(
    string Name,
    string? Description,
    string Host,
    int Port
);

public record RegisterServerDto(
    string Name,
    string? Description,
    string Host,
    int Port,
    int UserCount
);
