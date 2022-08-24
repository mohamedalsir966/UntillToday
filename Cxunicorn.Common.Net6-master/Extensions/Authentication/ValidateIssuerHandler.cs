using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Extensions.Authentication
{
    public class ValidateIssuerHandler : AuthorizationHandler<ValidateIssuerRequirement>
    {
        protected override Task HandleRequirementAsync(
           AuthorizationHandlerContext context,
           ValidateIssuerRequirement requirement)
        {

            if (context.User.FindFirst("iss") != null)
            {
                string? issuer = context.User.FindFirst("iss")?.Issuer;
                if (!string.IsNullOrWhiteSpace(issuer) && issuer.StartsWith("https://sts.windows.net/"))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    context.Fail();
                }
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
