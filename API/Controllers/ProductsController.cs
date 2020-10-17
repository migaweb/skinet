using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Core.Interfaces;
using Core.Specifications;
using API.Dtos;
using System.Linq;
using AutoMapper;

namespace API.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class ProductsController : ControllerBase
  {
    private readonly IGenericRepository<Product> _productsRepo;
    private readonly IGenericRepository<ProductType> _productTypeRepo;
    private readonly IMapper _mapper;
    private readonly IGenericRepository<ProductBrand> _productBrandRepo;
    public ProductsController(IGenericRepository<Product> productsRepo,
                              IGenericRepository<ProductBrand> productBrandRepo,
                              IGenericRepository<ProductType> productTypeRepo,
                              IMapper mapper)
    {
      _productBrandRepo = productBrandRepo;
      _productsRepo = productsRepo;
      _productTypeRepo = productTypeRepo;
      _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductToReturnDto>>> GetProducts()
    {
      var products = await _productsRepo.ListAsync(new ProductsWithTypesAndBrandsSpecification());

      return Ok(
        _mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products)
      );
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductToReturnDto>> GetProduct(int id)
    {
      var product = await _productsRepo.GetEntityWithSpec(new ProductsWithTypesAndBrandsSpecification(id));
      return _mapper.Map<Product, ProductToReturnDto>(product);
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