using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data
{
  public class StoreContextSeed
  {
    public static async Task SeedAsync(StoreContext context, ILoggerFactory loggerFactory)
    {
      try
      {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (!context.ProductBrands.Any())
        {
          var brandsData = File.ReadAllText($"{path}/Data/SeedData/brands.json");
          var brands = JsonSerializer.Deserialize<List<ProductBrand>>(brandsData);
          await context.ProductBrands.AddRangeAsync(brands);

          await context.SaveChangesAsync();
        }
        if (!context.ProductTypes.Any())
        {
          var typesData = File.ReadAllText($"{path}/Data/SeedData/types.json");
          var types = JsonSerializer.Deserialize<List<ProductType>>(typesData);
          await context.ProductTypes.AddRangeAsync(types);

          await context.SaveChangesAsync();
        }

        if (!context.Products.Any())
        {
          var productData = File.ReadAllText($"{path}/Data/SeedData/products.json");
          var products = JsonSerializer.Deserialize<List<Product>>(productData);
          await context.Products.AddRangeAsync(products);

          await context.SaveChangesAsync();
        }

        if (!context.DeliveryMethods.Any())
        {
          var dmData = File.ReadAllText($"{path}/Data/SeedData/delivery.json");
          var methods = JsonSerializer.Deserialize<List<DeliveryMethod>>(dmData);
          await context.DeliveryMethods.AddRangeAsync(methods);

          await context.SaveChangesAsync();
        }
      }
      catch (Exception ex)
      {
        var logger = loggerFactory.CreateLogger<StoreContextSeed>();
        logger.LogError(ex, ex.Message);
      }
    }
  }
}