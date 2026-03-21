using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Billing;

public sealed record GetRevenueQuery(DateTime From, DateTime To) : IResultQuery<RevenueDto>, ICacheableQuery
{
    public string CacheKey => $"billing:revenue:{From:yyyyMMdd}:{To:yyyyMMdd}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed class GetRevenueQueryValidator : AbstractValidator<GetRevenueQuery>
{
    public GetRevenueQueryValidator()
    {
        RuleFor(q => q.From).NotEmpty();
        RuleFor(q => q.To).NotEmpty().GreaterThanOrEqualTo(q => q.From).WithMessage("'To' must be greater than or equal to 'From'.");
    }
}

public sealed record GetRevenueQueryHandler(
    ChinookDbContext dbContext,
    ILogger<GetRevenueQueryHandler> logger) : IResultQueryHandler<GetRevenueQuery, RevenueDto>
{
    public async Task<Result<RevenueDto>> Handle(GetRevenueQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetRevenueQueryHandler.Handle] - Handling GetRevenueQuery from {From} to {To}", request.From, request.To);

        var invoices = await dbContext.Invoices
            .AsNoTracking()
            .Where(i => i.InvoiceDate >= request.From && i.InvoiceDate <= request.To)
            .ToListAsync(cancellationToken);

        var totalRevenue = invoices.Sum(i => i.Total);
        var invoiceCount = invoices.Count;

        logger.LogInformation("[GetRevenueQueryHandler.Handle] - Revenue from {From} to {To}: {TotalRevenue} across {InvoiceCount} invoices", request.From, request.To, totalRevenue, invoiceCount);
        return Result.Ok(new RevenueDto(request.From, request.To, totalRevenue, invoiceCount));
    }
}
