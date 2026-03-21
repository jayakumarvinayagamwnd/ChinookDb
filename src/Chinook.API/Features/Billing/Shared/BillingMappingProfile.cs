using AutoMapper;
using Chinook.API.Infrastructure.Persistence.Entities.Billing;

namespace Chinook.API.Features.Billing;

public sealed class BillingMappingProfile : Profile
{
    public BillingMappingProfile()
    {
        CreateMap<Invoice, InvoiceDto>()
            .ForCtorParam(nameof(InvoiceDto.InvoiceId), opt => opt.MapFrom(src => src.InvoiceId))
            .ForCtorParam(nameof(InvoiceDto.CustomerId), opt => opt.MapFrom(src => src.CustomerId))
            .ForCtorParam(nameof(InvoiceDto.InvoiceDate), opt => opt.MapFrom(src => src.InvoiceDate))
            .ForCtorParam(nameof(InvoiceDto.BillingAddress), opt => opt.MapFrom(src => src.BillingAddress))
            .ForCtorParam(nameof(InvoiceDto.BillingCity), opt => opt.MapFrom(src => src.BillingCity))
            .ForCtorParam(nameof(InvoiceDto.BillingState), opt => opt.MapFrom(src => src.BillingState))
            .ForCtorParam(nameof(InvoiceDto.BillingCountry), opt => opt.MapFrom(src => src.BillingCountry))
            .ForCtorParam(nameof(InvoiceDto.BillingPostalCode), opt => opt.MapFrom(src => src.BillingPostalCode))
            .ForCtorParam(nameof(InvoiceDto.Total), opt => opt.MapFrom(src => src.Total));

        CreateMap<InvoiceLine, InvoiceLineDto>()
            .ForCtorParam(nameof(InvoiceLineDto.InvoiceLineId), opt => opt.MapFrom(src => src.InvoiceLineId))
            .ForCtorParam(nameof(InvoiceLineDto.InvoiceId), opt => opt.MapFrom(src => src.InvoiceId))
            .ForCtorParam(nameof(InvoiceLineDto.TrackId), opt => opt.MapFrom(src => src.TrackId))
            .ForCtorParam(nameof(InvoiceLineDto.UnitPrice), opt => opt.MapFrom(src => src.UnitPrice))
            .ForCtorParam(nameof(InvoiceLineDto.Quantity), opt => opt.MapFrom(src => src.Quantity));

        CreateMap<CreateInvoiceCommand, Invoice>()
            .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(dest => dest.InvoiceDate, opt => opt.MapFrom(src => src.InvoiceDate))
            .ForMember(dest => dest.BillingAddress, opt => opt.MapFrom(src => src.BillingAddress))
            .ForMember(dest => dest.BillingCity, opt => opt.MapFrom(src => src.BillingCity))
            .ForMember(dest => dest.BillingState, opt => opt.MapFrom(src => src.BillingState))
            .ForMember(dest => dest.BillingCountry, opt => opt.MapFrom(src => src.BillingCountry))
            .ForMember(dest => dest.BillingPostalCode, opt => opt.MapFrom(src => src.BillingPostalCode))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(_ => 0m))
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.InvoiceLines, opt => opt.Ignore());
    }
}
