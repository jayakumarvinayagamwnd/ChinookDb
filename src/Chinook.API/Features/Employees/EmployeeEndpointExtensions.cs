using Chinook.API.Common.Results;
using Chinook.API.Common.Pagination;
using MediatR;
using Serilog;

namespace Chinook.API.Features.Employees;

public static class EmployeeEndpointExtensions
{
    public static WebApplication MapEmployeeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/employees").WithTags("Employees");

        group.MapGet("/", GetEmployeesAsync)
            .WithName("GetEmployees")
            .WithSummary("Retrieves a list of employees.")
            .WithDescription("Returns employees using offset pagination.")
            .Produces<OffsetPagedResponse<EmployeeDto>>(StatusCodes.Status200OK);

        group.MapGet("/{employeeId:int}", GetEmployeeByIdAsync)
            .WithName("GetEmployeeById")
            .WithSummary("Retrieves a single employee by id.")
            .WithDescription("Returns one employee by employee id.")
            .Produces<EmployeeDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateEmployeeAsync)
            .WithName("CreateEmployee")
            .WithSummary("Creates a new employee.")
            .WithDescription("Adds a new employee and returns the created resource.")
            .Produces<EmployeeDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapPatch("/{employeeId:int}", UpdateEmployeeAsync)
            .WithName("UpdateEmployee")
            .WithSummary("Updates an existing employee.")
            .WithDescription("Applies updates to an employee.")
            .Produces<EmployeeDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{employeeId:int}/reports", GetEmployeeReportsAsync)
            .WithName("GetEmployeeReports")
            .WithSummary("Retrieves direct reports for an employee.")
            .WithDescription("Returns employees that directly report to the specified employee.")
            .Produces<List<EmployeeDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{employeeId:int}/customers", GetEmployeeCustomersAsync)
            .WithName("GetEmployeeCustomers")
            .WithSummary("Retrieves customers assigned to an employee.")
            .WithDescription("Returns customers for whom the employee is the support representative.")
            .Produces<List<EmployeeCustomerDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{employeeId:int}/manager", UpdateEmployeeManagerAsync)
            .WithName("UpdateEmployeeManager")
            .WithSummary("Updates an employee's manager assignment.")
            .WithDescription("Assigns or removes the direct manager for the specified employee.")
            .Produces<EmployeeDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/hierarchy", GetEmployeeHierarchyAsync)
            .WithName("GetEmployeeHierarchy")
            .WithSummary("Retrieves the employee reporting hierarchy.")
            .WithDescription("Returns employees arranged as a reporting tree from top-level managers down.")
            .Produces<List<EmployeeHierarchyNodeDto>>(StatusCodes.Status200OK);

        return app;
    }

    private static async Task<IResult> GetEmployeesAsync(int? offset, int? limit, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[EmployeeEndpointExtensions.GetEmployeesAsync] - Handling GetEmployeesAsync with Offset: {Offset}, Limit: {Limit}", offset, limit);
        var result = await mediator.Send(new ListEmployeesQuery(offset ?? 0, limit ?? OffsetPaginationDefaults.DefaultLimit), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetEmployeeByIdAsync(int employeeId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[EmployeeEndpointExtensions.GetEmployeeByIdAsync] - Handling GetEmployeeByIdAsync for EmployeeId: {EmployeeId}", employeeId);
        var result = await mediator.Send(new GetEmployeeByIdQuery(employeeId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateEmployeeAsync(CreateEmployeeCommand request, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[EmployeeEndpointExtensions.CreateEmployeeAsync] - Handling CreateEmployeeAsync for Email: {Email}", request.Email);
        var result = await mediator.Send(request, cancellationToken);
        return result.ToCreatedResult($"/api/v1/employees/{result.Value?.EmployeeId}");
    }

    private static async Task<IResult> UpdateEmployeeAsync(int employeeId, UpdateEmployeeRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[EmployeeEndpointExtensions.UpdateEmployeeAsync] - Handling UpdateEmployeeAsync for EmployeeId: {EmployeeId}", employeeId);
        var result = await mediator.Send(new UpdateEmployeeCommand(
            employeeId,
            request.FirstName,
            request.LastName,
            request.Title,
            request.ReportsTo,
            request.BirthDate,
            request.HireDate,
            request.Address,
            request.City,
            request.State,
            request.Country,
            request.PostalCode,
            request.Phone,
            request.Fax,
            request.Email), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetEmployeeReportsAsync(int employeeId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[EmployeeEndpointExtensions.GetEmployeeReportsAsync] - Handling GetEmployeeReportsAsync for EmployeeId: {EmployeeId}", employeeId);
        var result = await mediator.Send(new GetEmployeeReportsQuery(employeeId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetEmployeeCustomersAsync(int employeeId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[EmployeeEndpointExtensions.GetEmployeeCustomersAsync] - Handling GetEmployeeCustomersAsync for EmployeeId: {EmployeeId}", employeeId);
        var result = await mediator.Send(new GetEmployeeCustomersQuery(employeeId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateEmployeeManagerAsync(int employeeId, UpdateEmployeeManagerRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[EmployeeEndpointExtensions.UpdateEmployeeManagerAsync] - Handling UpdateEmployeeManagerAsync for EmployeeId: {EmployeeId}", employeeId);
        var result = await mediator.Send(new UpdateEmployeeManagerCommand(employeeId, request.ManagerId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetEmployeeHierarchyAsync(IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[EmployeeEndpointExtensions.GetEmployeeHierarchyAsync] - Handling GetEmployeeHierarchyAsync");
        var result = await mediator.Send(new GetEmployeeHierarchyQuery(), cancellationToken);
        return result.ToHttpResult();
    }
}
