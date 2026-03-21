namespace Chinook.API.Features.Employees;

public sealed record EmployeeDto(
    int EmployeeId,
    string FirstName,
    string LastName,
    string? Title,
    int? ReportsTo,
    DateTime? BirthDate,
    DateTime? HireDate,
    string? Address,
    string? City,
    string? State,
    string? Country,
    string? PostalCode,
    string? Phone,
    string? Fax,
    string? Email);
