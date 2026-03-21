namespace Chinook.API.Features.Customers;

public sealed record SupportRepDto(
    int EmployeeId,
    string FirstName,
    string LastName,
    string? Title,
    string? Email,
    string? Phone);
