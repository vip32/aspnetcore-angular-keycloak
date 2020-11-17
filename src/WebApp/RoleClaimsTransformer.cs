namespace WebApp
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class RoleClaimsTransformer : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)principal.Identity;

            // flatten realm_access element because Microsoft identity model doesn't support nested claims
            // by map it to Microsoft identity model, because automatic JWT bearer token mapping already processed here
            if (claimsIdentity.IsAuthenticated && claimsIdentity.HasClaim((claim) => claim.Type == "realm_access"))
            {
                var claim = claimsIdentity.FindFirst((claim) => claim.Type == "realm_access");
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(claim.Value);
                if (dictionary["roles"] != null)
                {
                    foreach (var role in dictionary["roles"])
                    {
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                    }
                }
            }

            if (claimsIdentity.IsAuthenticated && claimsIdentity.HasClaim((claim) => claim.Type == "resource_access"))
            {
                var claim = claimsIdentity.FindFirst((claim) => claim.Type.Equals("resource_access", System.StringComparison.OrdinalIgnoreCase));

                if (claim != null && !string.IsNullOrEmpty(claim.Value))
                {
                    foreach (var roles in
                        JObject.Parse(claim.Value).SelectTokens("$..roles"))
                    {
                        if (roles?.Any() == true)
                        {
                            foreach (var role in roles.Values<string>())
                            {
                                if (!string.IsNullOrEmpty(role)
                                    && !claimsIdentity.Claims.Any(c => c.Value.Equals(role, System.StringComparison.OrdinalIgnoreCase)))
                                {
                                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                                }
                            }
                        }
                    }
                }
            }

            // flatten roles element because Microsoft identity model doesn't support nested claims
            // by map it to Microsoft identity model, because automatic JWT bearer token mapping already processed here
            //if (claimsIdentity.IsAuthenticated && claimsIdentity.HasClaim((claim) => claim.Type == "roles"))
            //{
            //    var claim = claimsIdentity.FindFirst((claim) => claim.Type == "roles");
            //    var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(claim.Value);
            //    if (dictionary != null)
            //    {
            //        foreach (var role in dictionary)
            //        {
            //            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.Value));
            //        }
            //    }
            //}

            return Task.FromResult(principal);
        }
    }
}
