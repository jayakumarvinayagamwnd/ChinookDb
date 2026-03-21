using AutoMapper;
using Chinook.API.Infrastructure.Persistence.Entities.Customers;
using Chinook.API.Infrastructure.Persistence.Entities.Employees;

namespace Chinook.API.Features.Employees;

public sealed class EmployeeMappingProfile : Profile
{
    public EmployeeMappingProfile()
    {
        CreateMap<Employee, EmployeeDto>()
            .ForCtorParam(nameof(EmployeeDto.EmployeeId), opt => opt.MapFrom(src => src.EmployeeId))
            .ForCtorParam(nameof(EmployeeDto.FirstName), opt => opt.MapFrom(src => src.FirstName))
            .ForCtorParam(nameof(EmployeeDto.LastName), opt => opt.MapFrom(src => src.LastName))
            .ForCtorParam(nameof(EmployeeDto.Title), opt => opt.MapFrom(src => src.Title))
            .ForCtorParam(nameof(EmployeeDto.ReportsTo), opt => opt.MapFrom(src => src.ReportsTo))
            .ForCtorParam(nameof(EmployeeDto.BirthDate), opt => opt.MapFrom(src => src.BirthDate))
            .ForCtorParam(nameof(EmployeeDto.HireDate), opt => opt.MapFrom(src => src.HireDate))
            .ForCtorParam(nameof(EmployeeDto.Address), opt => opt.MapFrom(src => src.Address))
            .ForCtorParam(nameof(EmployeeDto.City), opt => opt.MapFrom(src => src.City))
            .ForCtorParam(nameof(EmployeeDto.State), opt => opt.MapFrom(src => src.State))
            .ForCtorParam(nameof(EmployeeDto.Country), opt => opt.MapFrom(src => src.Country))
            .ForCtorParam(nameof(EmployeeDto.PostalCode), opt => opt.MapFrom(src => src.PostalCode))
            .ForCtorParam(nameof(EmployeeDto.Phone), opt => opt.MapFrom(src => src.Phone))
            .ForCtorParam(nameof(EmployeeDto.Fax), opt => opt.MapFrom(src => src.Fax))
            .ForCtorParam(nameof(EmployeeDto.Email), opt => opt.MapFrom(src => src.Email));

        CreateMap<Customer, EmployeeCustomerDto>()
            .ForCtorParam(nameof(EmployeeCustomerDto.CustomerId), opt => opt.MapFrom(src => src.CustomerId))
            .ForCtorParam(nameof(EmployeeCustomerDto.FirstName), opt => opt.MapFrom(src => src.FirstName))
            .ForCtorParam(nameof(EmployeeCustomerDto.LastName), opt => opt.MapFrom(src => src.LastName))
            .ForCtorParam(nameof(EmployeeCustomerDto.Email), opt => opt.MapFrom(src => src.Email))
            .ForCtorParam(nameof(EmployeeCustomerDto.Company), opt => opt.MapFrom(src => src.Company))
            .ForCtorParam(nameof(EmployeeCustomerDto.Country), opt => opt.MapFrom(src => src.Country))
            .ForCtorParam(nameof(EmployeeCustomerDto.SupportRepId), opt => opt.MapFrom(src => src.SupportRepId));

        CreateMap<CreateEmployeeCommand, Employee>()
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.ReportsTo, opt => opt.MapFrom(src => src.ReportsTo))
            .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
            .ForMember(dest => dest.HireDate, opt => opt.MapFrom(src => src.HireDate))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
            .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.PostalCode))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.Fax, opt => opt.MapFrom(src => src.Fax))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Manager, opt => opt.Ignore())
            .ForMember(dest => dest.DirectReports, opt => opt.Ignore())
            .ForMember(dest => dest.SupportedCustomers, opt => opt.Ignore());

        CreateMap<UpdateEmployeeCommand, Employee>()
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.ReportsTo, opt => opt.MapFrom(src => src.ReportsTo))
            .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
            .ForMember(dest => dest.HireDate, opt => opt.MapFrom(src => src.HireDate))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
            .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.PostalCode))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.Fax, opt => opt.MapFrom(src => src.Fax))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Manager, opt => opt.Ignore())
            .ForMember(dest => dest.DirectReports, opt => opt.Ignore())
            .ForMember(dest => dest.SupportedCustomers, opt => opt.Ignore());
    }
}
