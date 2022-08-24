using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Extensions.Authentication
{
    public class BearerTokenAcquisitionFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var assertion = context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            context.HttpContext.Request.Headers.Add("BearerToken", assertion);
            await next.Invoke();
        }
    }
}
