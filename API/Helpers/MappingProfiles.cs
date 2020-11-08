using API.Dtos;
using AutoMapper;
using Core.Entities;
using Core.Entities.Identity;
using Core.Entities.OrderAggregate;

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

      CreateMap<Core.Entities.Identity.Address, AddressDto>().ReverseMap();
      CreateMap<BasketItemDto, BasketItem>();
      CreateMap<CustomerBasketDto, CustomerBasket>();
      CreateMap<AddressDto, Core.Entities.OrderAggregate.Address>();
      CreateMap<Order, OrderToReturnDto>()
        .ForMember(dto => dto.DeliveryMethod, options => options.MapFrom(order => order.DeliveryMethod.ShortName))
        .ForMember(dto => dto.ShippingPrice, options => options.MapFrom(order => order.DeliveryMethod.Price));
      CreateMap<OrderItem, OrderItemDto>()
        .ForMember(dto => dto.ProductId, options => options.MapFrom(orderItem => orderItem.ItemOrdered.ProductItemId))
        .ForMember(dto => dto.ProductName, options => options.MapFrom(orderItem => orderItem.ItemOrdered.ProductName))
        .ForMember(dto => dto.PictureUrl, options => options.MapFrom<OrderItemUrlResolver>());
    }
  }
}