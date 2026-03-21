using AutoMapper;
using Chinook.API.Infrastructure.Persistence.Entities.Customers;
using Chinook.API.Infrastructure.Persistence.Entities.Employees;

namespace Chinook.API.Features.Customers;

public sealed class CustomerMappingProfile : Profile
{
    public CustomerMappingProfile()
    {
        CreateMap<Customer, CustomerDto>()
            .ForCtorParam(nameof(CustomerDto.CustomerId), opt => opt.MapFrom(src => src.CustomerId))
            .ForCtorParam(nameof(CustomerDto.FirstName), opt => opt.MapFrom(src => src.FirstName))
            .ForCtorParam(nameof(CustomerDto.LastName), opt => opt.MapFrom(src => src.LastName))
            .ForCtorParam(nameof(CustomerDto.Company), opt => opt.MapFrom(src => src.Company))
            .ForCtorParam(nameof(CustomerDto.Address), opt => opt.MapFrom(src => src.Address))
            .ForCtorParam(nameof(CustomerDto.City), opt => opt.MapFrom(src => src.City))
            .ForCtorParam(nameof(CustomerDto.State), opt => opt.MapFrom(src => src.State))
            .ForCtorParam(nameof(CustomerDto.Country), opt => opt.MapFrom(src => src.Country))
            .ForCtorParam(nameof(CustomerDto.PostalCode), opt => opt.MapFrom(src => src.PostalCode))
            .ForCtorParam(nameof(CustomerDto.Phone), opt => opt.MapFrom(src => src.Phone))
            .ForCtorParam(nameof(CustomerDto.Fax), opt => opt.MapFrom(src => src.Fax))
            .ForCtorParam(nameof(CustomerDto.Email), opt => opt.MapFrom(src => src.Email))
            .ForCtorParam(nameof(CustomerDto.SupportRepId), opt => opt.MapFrom(src => src.SupportRepId));

        CreateMap<Employee, SupportRepDto>()
            .ForCtorParam(nameof(SupportRepDto.EmployeeId), opt => opt.MapFrom(src => src.EmployeeId))
            .ForCtorParam(nameof(SupportRepDto.FirstName), opt => opt.MapFrom(src => src.FirstName))
            .ForCtorParam(nameof(SupportRepDto.LastName), opt => opt.MapFrom(src => src.LastName))
            .ForCtorParam(nameof(SupportRepDto.Title), opt => opt.MapFrom(src => src.Title))
            .ForCtorParam(nameof(SupportRepDto.Email), opt => opt.MapFrom(src => src.Email))
            .ForCtorParam(nameof(SupportRepDto.Phone), opt => opt.MapFrom(src => src.Phone));

        CreateMap<CreateCustomerCommand, Customer>()
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Company))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
            .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.PostalCode))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.Fax, opt => opt.MapFrom(src => src.Fax))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.SupportRepId, opt => opt.MapFrom(src => src.SupportRepId))
            .ForMember(dest => dest.SupportRep, opt => opt.Ignore())
            .ForMember(dest => dest.Invoices, opt => opt.Ignore());

        CreateMap<UpdateCustomerCommand, Customer>()
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Company))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
            .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.PostalCode))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.Fax, opt => opt.MapFrom(src => src.Fax))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.SupportRepId, opt => opt.MapFrom(src => src.SupportRepId))
            .ForMember(dest => dest.SupportRep, opt => opt.Ignore())
            .ForMember(dest => dest.Invoices, opt => opt.Ignore());
    }
}
