using System.Linq;
using API.Errors;
using API.Helpers;
using API.Middleware;
using AutoMapper;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace API
{
  public class Startup
  {
    private readonly IConfiguration _config;
    public Startup(IConfiguration config)
    {
      _config = config;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    // Dependency injection container
    // Order is not important, except related to controllers.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
      services.AddScoped<IProductRepository, ProductRepository>();
      services.AddAutoMapper(typeof(MappingProfiles));
      services.AddControllers();
      services.AddDbContext<StoreContext>(x => x.UseSqlite(_config.GetConnectionString("DefaultConnection")));

      // Order important here
      services.Configure<ApiBehaviorOptions>(options =>
      {
        options.InvalidModelStateResponseFactory = actionContext =>
        {
          var errors = actionContext.ModelState.Where(e => e.Value.Errors.Count > 0).SelectMany(x => x.Value.Errors)
          .Select(x => x.ErrorMessage).ToArray();

          var errorResponse = new ApiValidationErrorResponse()
          {
            Errors = errors
          };

          return new BadRequestObjectResult(errorResponse);
        };
      });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    // Add middleware to the pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseMiddleware<ExceptionMiddleware>();

      app.UseStatusCodePagesWithReExecute("/errors/{0}");

      app.UseHttpsRedirection();

      app.UseRouting();

      app.UseStaticFiles();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
    }
  }
}
