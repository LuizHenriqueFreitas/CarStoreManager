namespace CarStoreManager.Web.Models;

public record ErrorResponse(
    int StatusCode,
    string Message,
    string? Details
);