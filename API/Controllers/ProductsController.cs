using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Core.Interfaces;
using Core.Specifications;
using API.Dtos;
using System.Linq;
using AutoMapper;
using API.Errors;
using Microsoft.AspNetCore.Http;
using API.Helpers;

namespace API.Controllers
{
  public class ProductsController : BaseApiController
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

    [Cache(600)]
    [HttpGet]
    public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts(
      [FromQuery] ProductSpecParams productparams)
    {
      var products = await _productsRepo.ListAsync(
        new ProductsWithTypesAndBrandsSpecification(productparams)
      );

      var totalItems = await _productsRepo.CountAsync(
        new ProductWithFiltersForCountSpecification(productparams)
      );

      var data = _mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products);

      return Ok(
        new Pagination<ProductToReturnDto>(productparams.PageIndex, productparams.PageSize, totalItems, data)
      );
    }

    [Cache(600)]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductToReturnDto>> GetProduct(int id)
    {
      var product = await _productsRepo.GetEntityWithSpec(new ProductsWithTypesAndBrandsSpecification(id));

      if (product == null)
      {
        return NotFound(new ApiResponse(404));
      }

      return _mapper.Map<Product, ProductToReturnDto>(product);
    }

    [Cache(600)]
    [HttpGet("brands")]
    public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetProductBrands()
    {
      return Ok(await _productBrandRepo.ListAllAsync());
    }

    [Cache(600)]
    [HttpGet("types")]
    public async Task<ActionResult<IReadOnlyList<ProductType>>> GetProductTypes()
    {
      return Ok(await _productTypeRepo.ListAllAsync());
    }
  }
}