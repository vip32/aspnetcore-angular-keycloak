namespace WebApp.Controllers
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize] // (Roles = "role1")
    [ApiController]
    [Route("[controller]")]
    public class UserProfileController : ControllerBase
    {
        [HttpGet]
        public Dictionary<string, string> Get()
        {
            var result = new Dictionary<string, string>
            {
                ["IsAuthenticated"] = this.HttpContext.User?.Identity?.IsAuthenticated.ToString(),
                ["Name"] = this.HttpContext.User?.Identity?.Name,
            };

            if (this.HttpContext.User?.Claims != null)
            {
                foreach (var claim in this.HttpContext.User?.Claims)
                {
                    if (!result.ContainsKey(claim.Type))
                    {
                        result.Add(claim.Type, claim.Value);
                    }
                    else
                    {
                        result[claim.Type] += $"; {claim.Value}";
                    }
                }
            }

            return result;
        }
    }
}
