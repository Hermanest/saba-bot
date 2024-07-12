namespace SabaBot;

public record struct ApiResponse<T>(
    T? Value,
    bool Succeed,
    string? Reason
);

public record struct ApiResponse(
    bool Succeed,
    string? Reason
);