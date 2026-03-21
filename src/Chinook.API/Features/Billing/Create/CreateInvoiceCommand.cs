using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using Chinook.API.Infrastructure.Persistence.Entities.Billing;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Billing;

public sealed record CreateInvoiceCommand(
    int CustomerId,
    DateTime InvoiceDate,
    string? BillingAddress,
    string? BillingCity,
    string? BillingState,
    string? BillingCountry,
    string? BillingPostalCode) : IResultCommand<InvoiceDto>;

public sealed class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(c => c.CustomerId).GreaterThan(0);
        RuleFor(c => c.InvoiceDate).NotEmpty();
    }
}

public sealed record CreateInvoiceCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<CreateInvoiceCommandHandler> logger) : IResultCommandHandler<CreateInvoiceCommand, InvoiceDto>
{
    public async Task<Result<InvoiceDto>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[CreateInvoiceCommandHandler.Handle] - Handling CreateInvoiceCommand for CustomerId: {CustomerId}", request.CustomerId);

        var customerExists = await dbContext.Customers
            .AsNoTracking()
            .AnyAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

        if (!customerExists)
        {
            logger.LogWarning("[CreateInvoiceCommandHandler.Handle] - Customer not found for CustomerId: {CustomerId}", request.CustomerId);
            return Result.Fail($"Customer with ID {request.CustomerId} not found.");
        }

        var invoice = mapper.Map<Invoice>(request);
        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[CreateInvoiceCommandHandler.Handle] - Successfully created invoice with InvoiceId: {InvoiceId}", invoice.InvoiceId);
        return Result.Ok(mapper.Map<InvoiceDto>(invoice));
    }
}
