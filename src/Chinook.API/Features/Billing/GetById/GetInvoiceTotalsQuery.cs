using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Billing;

public sealed record GetInvoiceTotalsQuery(int InvoiceId) : IResultQuery<InvoiceTotalsDto>, ICacheableQuery
{
    public string CacheKey => $"billing:invoices:{InvoiceId}:totals";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(5);
}

public sealed class GetInvoiceTotalsQueryValidator : AbstractValidator<GetInvoiceTotalsQuery>
{
    public GetInvoiceTotalsQueryValidator()
    {
        RuleFor(q => q.InvoiceId).GreaterThan(0);
    }
}

public sealed record GetInvoiceTotalsQueryHandler(
    ChinookDbContext dbContext,
    ILogger<GetInvoiceTotalsQueryHandler> logger) : IResultQueryHandler<GetInvoiceTotalsQuery, InvoiceTotalsDto>
{
    public async Task<Result<InvoiceTotalsDto>> Handle(GetInvoiceTotalsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetInvoiceTotalsQueryHandler.Handle] - Handling GetInvoiceTotalsQuery for InvoiceId: {InvoiceId}", request.InvoiceId);

        var totals = await dbContext.Invoices
            .AsNoTracking()
            .Where(i => i.InvoiceId == request.InvoiceId)
            .Select(i => new InvoiceTotalsDto(
                i.InvoiceId,
                i.InvoiceLines.Count,
                i.InvoiceLines.Sum(l => l.UnitPrice * l.Quantity),
                i.Total))
            .SingleOrDefaultAsync(cancellationToken);

        if (totals is null)
        {
            logger.LogWarning("[GetInvoiceTotalsQueryHandler.Handle] - Invoice not found for InvoiceId: {InvoiceId}", request.InvoiceId);
            return Result.Fail($"Invoice with ID {request.InvoiceId} not found.");
        }

        logger.LogInformation("[GetInvoiceTotalsQueryHandler.Handle] - Successfully retrieved totals for InvoiceId: {InvoiceId}", request.InvoiceId);
        return Result.Ok(totals);
    }
}
