namespace Chinook.API.Features.Employees;

public sealed record EmployeeHierarchyNodeDto(
    int EmployeeId,
    string FirstName,
    string LastName,
    string? Title,
    int? ReportsTo,
    List<EmployeeHierarchyNodeDto> Reports);
