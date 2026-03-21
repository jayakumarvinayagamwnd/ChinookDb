using Chinook.API.Common.Results;
using MediatR;
using Serilog;

namespace Chinook.API.Features.Customers;

public static class CustomerEndpointExtensions
{
    public static WebApplication MapCustomerEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/customers").WithTags("Customers");

        group.MapGet("/", GetCustomersAsync)
            .WithName("GetCustomers")
            .WithSummary("Retrieves a list of customers.")
            .WithDescription("Returns a list of all customers.")
            .Produces<List<CustomerDto>>(StatusCodes.Status200OK);

        group.MapGet("/{customerId:int}", GetCustomerByIdAsync)
            .WithName("GetCustomerById")
            .WithSummary("Retrieves a single customer by id.")
            .WithDescription("Returns one customer by customer id.")
            .Produces<CustomerDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateCustomerAsync)
            .WithName("CreateCustomer")
            .WithSummary("Creates a new customer.")
            .WithDescription("Adds a new customer and returns the created resource.")
            .Produces<CustomerDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapPatch("/{customerId:int}", UpdateCustomerAsync)
            .WithName("UpdateCustomer")
            .WithSummary("Updates an existing customer.")
            .WithDescription("Applies partial updates to a customer.")
            .Produces<CustomerDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{customerId:int}", DeleteCustomerAsync)
            .WithName("DeleteCustomer")
            .WithSummary("Deletes a customer by id.")
            .WithDescription("Permanently removes a customer. Fails if the customer has existing invoices.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{customerId:int}/support-rep", GetCustomerSupportRepAsync)
            .WithName("GetCustomerSupportRep")
            .WithSummary("Retrieves the support rep assigned to a customer.")
            .WithDescription("Returns the employee assigned as support representative for the specified customer.")
            .Produces<SupportRepDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{customerId:int}/support-rep", UpdateCustomerSupportRepAsync)
            .WithName("UpdateCustomerSupportRep")
            .WithSummary("Assigns or removes a support rep for a customer.")
            .WithDescription("Updates the support rep assignment for the specified customer. Pass null to remove.")
            .Produces<CustomerDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{customerId:int}/address", UpdateCustomerAddressAsync)
            .WithName("UpdateCustomerAddress")
            .WithSummary("Updates a customer's address.")
            .WithDescription("Replaces all address fields for the specified customer.")
            .Produces<CustomerDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{customerId:int}/contact-preferences", UpdateCustomerContactPreferencesAsync)
            .WithName("UpdateCustomerContactPreferences")
            .WithSummary("Updates a customer's contact preferences.")
            .WithDescription("Updates phone, fax, and email for the specified customer.")
            .Produces<CustomerDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{customerId:int}/purchase-history", GetCustomerPurchaseHistoryAsync)
            .WithName("GetCustomerPurchaseHistory")
            .WithSummary("Retrieves the purchase history for a customer.")
            .WithDescription("Returns all invoices for the specified customer, ordered by date descending.")
            .Produces<List<PurchaseHistoryItemDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetCustomersAsync(IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CustomerEndpointExtensions.GetCustomersAsync] - Handling GetCustomersAsync");
        var result = await mediator.Send(new ListCustomersQuery(), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetCustomerByIdAsync(int customerId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CustomerEndpointExtensions.GetCustomerByIdAsync] - Handling GetCustomerByIdAsync for CustomerId: {CustomerId}", customerId);
        var result = await mediator.Send(new GetCustomerByIdQuery(customerId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateCustomerAsync(CreateCustomerCommand command, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CustomerEndpointExtensions.CreateCustomerAsync] - Handling CreateCustomerAsync for Email: {Email}", command.Email);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToCreatedResult($"/api/v1/customers/{result.Value?.CustomerId}");
    }

    private static async Task<IResult> UpdateCustomerAsync(int customerId, UpdateCustomerRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CustomerEndpointExtensions.UpdateCustomerAsync] - Handling UpdateCustomerAsync for CustomerId: {CustomerId}", customerId);
        var result = await mediator.Send(new UpdateCustomerCommand(
            customerId,
            request.FirstName,
            request.LastName,
            request.Company,
            request.Address,
            request.City,
            request.State,
            request.Country,
            request.PostalCode,
            request.Phone,
            request.Fax,
            request.Email,
            request.SupportRepId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> DeleteCustomerAsync(int customerId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CustomerEndpointExtensions.DeleteCustomerAsync] - Handling DeleteCustomerAsync for CustomerId: {CustomerId}", customerId);
        var result = await mediator.Send(new DeleteCustomerCommand(customerId), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { errors = result.Errors.Select(e => e.Message) });
    }

    private static async Task<IResult> GetCustomerSupportRepAsync(int customerId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CustomerEndpointExtensions.GetCustomerSupportRepAsync] - Handling GetCustomerSupportRepAsync for CustomerId: {CustomerId}", customerId);
        var result = await mediator.Send(new GetCustomerSupportRepQuery(customerId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateCustomerSupportRepAsync(int customerId, UpdateCustomerSupportRepRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CustomerEndpointExtensions.UpdateCustomerSupportRepAsync] - Handling UpdateCustomerSupportRepAsync for CustomerId: {CustomerId}", customerId);
        var result = await mediator.Send(new UpdateCustomerSupportRepCommand(customerId, request.SupportRepId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateCustomerAddressAsync(int customerId, UpdateCustomerAddressRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CustomerEndpointExtensions.UpdateCustomerAddressAsync] - Handling UpdateCustomerAddressAsync for CustomerId: {CustomerId}", customerId);
        var result = await mediator.Send(new UpdateCustomerAddressCommand(
            customerId,
            request.Address,
            request.City,
            request.State,
            request.Country,
            request.PostalCode), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateCustomerContactPreferencesAsync(int customerId, UpdateCustomerContactPreferencesRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CustomerEndpointExtensions.UpdateCustomerContactPreferencesAsync] - Handling UpdateCustomerContactPreferencesAsync for CustomerId: {CustomerId}", customerId);
        var result = await mediator.Send(new UpdateCustomerContactPreferencesCommand(customerId, request.Phone, request.Fax, request.Email), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetCustomerPurchaseHistoryAsync(int customerId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CustomerEndpointExtensions.GetCustomerPurchaseHistoryAsync] - Handling GetCustomerPurchaseHistoryAsync for CustomerId: {CustomerId}", customerId);
        var result = await mediator.Send(new GetCustomerPurchaseHistoryQuery(customerId), cancellationToken);
        return result.ToHttpResult();
    }
}
