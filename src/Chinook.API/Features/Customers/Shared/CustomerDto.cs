namespace Chinook.API.Features.Customers;

public sealed record CustomerDto(
    int CustomerId,
    string FirstName,
    string LastName,
    string? Company,
    string? Address,
    string? City,
    string? State,
    string? Country,
    string? PostalCode,
    string? Phone,
    string? Fax,
    string Email,
    int? SupportRepId);
