using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;

namespace Infrastructure.Services
{
  public class OrderService : IOrderService
  {
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBasketRepository _basketRepo;
    private readonly IPaymentService _paymentService;

    public OrderService(IUnitOfWork unitOfWork, IBasketRepository basketRepo, IPaymentService paymentService)
    {
      _paymentService = paymentService;
      _unitOfWork = unitOfWork;
      _basketRepo = basketRepo;
    }

    public async Task<Order> CreateOrderAsync(string buyerEmail, int deliveryMethodId, string basketId, Address shippingAddress)
    {
      // get basket from the basket repo
      var basket = await _basketRepo.GetBasketAsync(basketId);
      // get items from the product repo
      var items = new List<OrderItem>();
      foreach (var item in basket.Items)
      {
        var productItem = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
        var itemOrdered = new ProductItemOrdered(productItem.Id, productItem.Name, productItem.PictureUrl);
        var orderItem = new OrderItem(itemOrdered, productItem.Price, item.Quantity);
        items.Add(orderItem);
      }
      // get the delivery method from repo
      var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(deliveryMethodId);
      // calculate subtotal
      var subtotal = items.Sum(e => e.Quantity * e.Price);

      // check to see if order exists
      var spec = new OrderByPaymentIntentIdSpecification(basket.PyamentIntentId);
      var existingOrder = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

      if (existingOrder != null)
      {
        _unitOfWork.Repository<Order>().Delete(existingOrder);
        await _paymentService.CreateOrUpdatePaymentIntent(basket.PyamentIntentId);
      }

      // create the order
      var order = new Order(items, buyerEmail, shippingAddress, deliveryMethod, subtotal, basket.PyamentIntentId);

      // save to db
      _unitOfWork.Repository<Order>().Add(order);
      var result = await _unitOfWork.Complete();

      if (result <= 0)
      {
        return null;
      }

      // return the order
      return order;
    }

    public async Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
    {
      return await _unitOfWork.Repository<DeliveryMethod>().ListAllAsync();
    }

    public async Task<Order> GetOrderByIdAsync(int id, string buyerEmail)
    {
      var spec = new OrdersWithItemsAndOrderingSpecification(id, buyerEmail);
      return await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);
    }

    public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
    {
      var spec = new OrdersWithItemsAndOrderingSpecification(buyerEmail);
      return await _unitOfWork.Repository<Order>().ListAsync(spec);
    }
  }
}