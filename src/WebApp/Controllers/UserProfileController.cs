namespace WebApp.Controllers
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [Authorize] // (Roles = "admin")
    [ApiController]
    [ApiVersion("1.0")]
    [Route("/api/v{version:apiVersion}/userprofile")]
    public class UserProfileController : ControllerBase
    {
        private readonly ILogger<UserProfileController> logger;

        public UserProfileController(ILogger<UserProfileController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public Dictionary<string, string> Get()
        {
            this.logger.LogInformation("userprofile request");

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
