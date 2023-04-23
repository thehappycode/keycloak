using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using MyWebApi.Constants;
using Newtonsoft.Json.Linq;

namespace MyWebApi.Authentication;

public class ClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var claimsIdentity = (ClaimsIdentity)principal.Identity;

        if(claimsIdentity.IsAuthenticated && claimsIdentity.HasClaim(claim => claim.Type == JWTConstant.RESOURCE_ACCESS)){
            
            var userRole = claimsIdentity.FindFirst(claim => claim.Type == JWTConstant.RESOURCE_ACCESS);
            var content = JObject.Parse(userRole.Value);
            foreach(var role in content[JWTConstant.MY_APP][JWTConstant.ROLES]){
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.ToString()));
            }
        }
        return Task.FromResult(principal);
    }
}