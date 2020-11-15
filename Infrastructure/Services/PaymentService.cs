using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.Extensions.Configuration;
using Stripe;
using Order = Core.Entities.OrderAggregate.Order;
using Product = Core.Entities.Product;

namespace Infrastructure.Services
{
  public class PaymentService : IPaymentService
  {
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBasketRepository _basketRepository;
    public PaymentService(IBasketRepository basketRepository, IUnitOfWork unitOfWork, IConfiguration configuration)
    {
      _basketRepository = basketRepository;
      _unitOfWork = unitOfWork;
      _configuration = configuration;
    }

    public async Task<CustomerBasket> CreateOrUpdatePaymentIntent(string basketId)
    {
      StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];
      var basket = await _basketRepository.GetBasketAsync(basketId);

      if (basket == null)
      {
        return null;
      }

      var shippingPrice = 0m;

      if (basket.DeliveryMethodId.HasValue)
      {
        var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(basket.DeliveryMethodId.Value);
        shippingPrice = deliveryMethod.Price;
      }

      foreach (var item in basket.Items)
      {
        var productItem = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
        if (item.Price != productItem.Price)
        {
          item.Price = productItem.Price;
        }
      }

      var service = new PaymentIntentService();
      PaymentIntent intent;
      if (string.IsNullOrEmpty(basket.PyamentIntentId))
      {
        var options = new PaymentIntentCreateOptions
        {
          Amount = (long)basket.Items.Sum(i => (i.Price * 100) * i.Quantity) + (long)shippingPrice,
          Currency = "usd",
          PaymentMethodTypes = new List<string>() { "card" }
        };

        intent = await service.CreateAsync(options);
        basket.PyamentIntentId = intent.Id;
        basket.ClientSecret = intent.ClientSecret;

      }
      else
      {
        var options = new PaymentIntentUpdateOptions
        {
          Amount = (long)basket.Items.Sum(i => (i.Price * 100) * i.Quantity) + ((long)shippingPrice * 100)
        };

        await service.UpdateAsync(basket.PyamentIntentId, options);
      }

      await _basketRepository.UpdateBasketAsync(basket);

      return basket;
    }

    public async Task<Order> UpdateOrderPaymentFailed(string paymentIntentId)
    {
      var spec = new OrderByPaymentIntentIdSpecification(paymentIntentId);
      var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

      if (order == null)
      {
        return null;
      }

      order.Status = OrderStatus.PaymentFailed;
      //_unitOfWork.Repository<Order>().Update(order);

      await _unitOfWork.Complete();

      return order;
    }

    public async Task<Order> UpdateOrderPaymentSucceeded(string paymentIntentId)
    {
      var spec = new OrderByPaymentIntentIdSpecification(paymentIntentId);
      var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

      if (order == null)
      {
        return null;
      }

      order.Status = OrderStatus.PaymentReceived;
      _unitOfWork.Repository<Order>().Update(order);

      await _unitOfWork.Complete();

      return order;
    }
  }
}
