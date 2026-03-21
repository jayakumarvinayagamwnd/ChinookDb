namespace Chinook.API.Features.Employees;

public sealed record EmployeeCustomerDto(
    int CustomerId,
    string FirstName,
    string LastName,
    string Email,
    string? Company,
    string? Country,
    int? SupportRepId);
