using Cxunicorn.Common.Middlewares.Logging.RequestLogging.RequestLoggingRepository;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Middlewares.Logging.RequestLogging
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRequestLoggingRepository _logRepository;

        public RequestLoggingMiddleware(RequestDelegate next, IRequestLoggingRepository loggingDataRepository)
        {
            _next = next;
            _logRepository = loggingDataRepository;
        }
        public async Task Invoke(HttpContext context)
        {
            var correlationId = Activity.Current?.Id;
            RequestLogEntity logEntity = new()
            {
                //CorrelationId = context.TraceIdentifier,
                CorrelationId = correlationId,
                IsHttps = context.Request.IsHttps,
                QueryString = context.Request.QueryString.ToString(),
                RequestMethod = context.Request.Method,
                RequestedOn = DateTime.UtcNow,
                RequestorId = context.User?.FindFirst(ClaimTypes.Email)?.Value,
                RequestorIP = context.Connection?.RemoteIpAddress?.ToString() ?? string.Empty,
                RequestUrl = context.Request.Path,
                StatusCode = context.Response.StatusCode,
            };

            context.Request.EnableBuffering();
            var bodyReader = new StreamReader(context.Request.Body, true);

            logEntity.RequestBody = await bodyReader.ReadToEndAsync();
            context.Request.Body.Seek(0, SeekOrigin.Begin);

            await _next(context);
            logEntity.StatusCode = context.Response.StatusCode;
            logEntity.RespondedOn = DateTime.UtcNow;

            await _logRepository.TableClient.AddEntityAsync(logEntity);
        }
    }
}
