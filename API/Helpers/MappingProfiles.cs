using API.Dtos;
using AutoMapper;
using Core.Entities;
using Core.Entities.Identity;

namespace API.Helpers
{
  public class MappingProfiles : Profile
  {
    public MappingProfiles()
    {
      CreateMap<Product, ProductToReturnDto>()
        .ForMember(dto => dto.ProductBrand, options => options.MapFrom(product => product.ProductBrand.Name))
        .ForMember(dto => dto.ProductType, options => options.MapFrom(product => product.ProductType.Name))
        .ForMember(dto => dto.PictureUrl, options => options.MapFrom<ProductUrlResolver>());

      CreateMap<Address, AddressDto>().ReverseMap();
    }
  }
}