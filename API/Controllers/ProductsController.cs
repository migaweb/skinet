using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Core.Interfaces;
using Core.Specifications;

namespace API.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class ProductsController : ControllerBase
  {
    private readonly IGenericRepository<Product> _productsRepo;
    private readonly IGenericRepository<ProductType> _productTypeRepo;
    private readonly IGenericRepository<ProductBrand> _productBrandRepo;
    public ProductsController(IGenericRepository<Product> productsRepo,
                              IGenericRepository<ProductBrand> productBrandRepo,
                              IGenericRepository<ProductType> productTypeRepo)
    {
      _productBrandRepo = productBrandRepo;
      _productsRepo = productsRepo;
      _productTypeRepo = productTypeRepo;
    }

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetProducts()
    {
      var products = await _productsRepo.ListAsync(new ProductsWithTypesAndBrandsSpecification());

      return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
      return await _productsRepo.GetEntityWithSpec(new ProductsWithTypesAndBrandsSpecification(id));
    }

    [HttpGet("brands")]
    public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetProductBrands()
    {
      return Ok(await _productBrandRepo.ListAllAsync());
    }

    [HttpGet("types")]
    public async Task<ActionResult<IReadOnlyList<ProductType>>> GetProductTypes()
    {
      return Ok(await _productTypeRepo.ListAllAsync());
    }
  }
}