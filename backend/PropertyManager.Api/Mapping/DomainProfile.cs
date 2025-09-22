using AutoMapper;
using PropertyManager.Api.Contracts.Requests;
using PropertyManager.Api.Contracts.Responses;
using PropertyManager.Api.Domain.Entities;

namespace PropertyManager.Api.Mapping;

public class DomainProfile : Profile
{
    public DomainProfile()
    {
        CreateMap<Tenant, TenantResponse>();
        CreateMap<CreateRentalUnitRequest, RentalUnit>();
        CreateMap<UpdateRentalUnitRequest, RentalUnit>();
        CreateMap<RentalUnit, RentalUnitResponse>();

        CreateMap<RentalContract, RentalContractResponse>()
            .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => $"{src.Tenant.FirstName} {src.Tenant.LastName}"))
            .ForMember(dest => dest.RentalUnitName, opt => opt.MapFrom(src => src.RentalUnit.Name))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency));

        CreateMap<RentPayment, RentPaymentResponse>()
            .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => $"{src.RentalContract.Tenant.FirstName} {src.RentalContract.Tenant.LastName}"))
            .ForMember(dest => dest.RentalUnitName, opt => opt.MapFrom(src => src.RentalContract.RentalUnit.Name));

    CreateMap<CreateRentPaymentRequest, RentPayment>().ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency));

        CreateMap<PaymentAlert, PaymentAlertResponse>();
    }
}
