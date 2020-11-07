using System.Linq;
using System.Security.Claims;

namespace API.Extensions
{
  public static class ClaimsPrincipleExtensions
  {
    public static string RetrieveEmailFromPrinciple(this ClaimsPrincipal user)
    {
      return user?.Claims?.FirstOrDefault(e => e.Type == ClaimTypes.Email)?.Value;
    }
  }
}