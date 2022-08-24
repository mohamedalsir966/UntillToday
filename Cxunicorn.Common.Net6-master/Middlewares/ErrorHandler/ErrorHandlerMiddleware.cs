using Cxunicorn.Common.Middlewares.ErrorHandler.Exceptions;
using Cxunicorn.Common.Middlewares.ErrorHandler.Response;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Middlewares.ErrorHandler
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                var currentBody = context.Response.Body;

                using var memoryStream = new MemoryStream();
                //set the current response to the memorystream.
                context.Response.Body = memoryStream;

                await _next(context);

                //reset the body 
                context.Response.Body = currentBody;
                memoryStream.Seek(0, SeekOrigin.Begin);

                var readToEnd = new StreamReader(memoryStream).ReadToEnd();

                if(!string.IsNullOrWhiteSpace(context.Response.ContentType))
                {
                    if (context.Response.ContentType.Contains("application/json"))
                    {
                        object? objResult;
                        try
                        {
                            objResult = JsonConvert.DeserializeObject(readToEnd);
                        }
                        catch (Exception)
                        {
                            throw new FailedResponse
                            {
                                Code = 500,
                                Message = "Cannot parse the response from the controller.",
                            };
                        }

                        var result = SuccessResponseContent.Create(objResult);
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(result));
                    }
                    else if (context.Response.ContentType.Contains("text/plain"))
                    {
                        throw new FailedResponse
                        {
                            Code = 500,
                            Message = "Plain text response from controller is not supported.",
                        };
                    }
                    else await _next(context);
                }
                else await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                string messageBody = string.Empty;  
                switch (error)
                {
                    case FailedResponse e:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        messageBody = System.Text.Json.JsonSerializer.Serialize(new FailedResponseContent
                        {
                            Message = e.Message,
                            Code = e.Code,
                        });
                        break;

                    case NotFoundResponse e:
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        messageBody = System.Text.Json.JsonSerializer.Serialize(new FailedResponseContent
                        {
                            Message = e.Message,
                            Code = e.Code,
                        });
                        break;

                    default:
                        // unhandled error
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        messageBody = System.Text.Json.JsonSerializer.Serialize(new FailedResponseContent
                        {
                            Message = $"UnhandledException: {error.Message}",
                            Code = 10000,
                        });
                        break;
                }

                var result = messageBody;
                await response.WriteAsync(result);
            }
        }
    }
}
