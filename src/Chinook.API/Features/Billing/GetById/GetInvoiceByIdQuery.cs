using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Billing;

public sealed record GetInvoiceByIdQuery(int InvoiceId) : IResultQuery<InvoiceDto>, ICacheableQuery
{
    public string CacheKey => $"billing:invoices:{InvoiceId}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(5);
}

public sealed class GetInvoiceByIdQueryValidator : AbstractValidator<GetInvoiceByIdQuery>
{
    public GetInvoiceByIdQueryValidator()
    {
        RuleFor(q => q.InvoiceId).GreaterThan(0);
    }
}

public sealed record GetInvoiceByIdQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<GetInvoiceByIdQueryHandler> logger) : IResultQueryHandler<GetInvoiceByIdQuery, InvoiceDto>
{
    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetInvoiceByIdQueryHandler.Handle] - Handling GetInvoiceByIdQuery for InvoiceId: {InvoiceId}", request.InvoiceId);

        var invoice = await dbContext.Invoices
            .AsNoTracking()
            .Where(i => i.InvoiceId == request.InvoiceId)
            .ProjectTo<InvoiceDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);

        if (invoice is null)
        {
            logger.LogWarning("[GetInvoiceByIdQueryHandler.Handle] - Invoice not found for InvoiceId: {InvoiceId}", request.InvoiceId);
            return Result.Fail($"Invoice with ID {request.InvoiceId} not found.");
        }

        logger.LogInformation("[GetInvoiceByIdQueryHandler.Handle] - Successfully retrieved invoice for InvoiceId: {InvoiceId}", request.InvoiceId);
        return Result.Ok(invoice);
    }
}
