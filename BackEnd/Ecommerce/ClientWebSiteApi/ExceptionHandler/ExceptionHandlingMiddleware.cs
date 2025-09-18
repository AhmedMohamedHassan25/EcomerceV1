using DTOs.SharedData;
using System.Net;
using System.Text.Json;

namespace ClientWebSiteApi.ExceptionHandler
{
    public class ExceptionHandlingMiddleware
    {
        
            private readonly RequestDelegate _next;
            private readonly ILogger<ExceptionHandlingMiddleware> _logger;

            public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
            {
                _next = next;
                _logger = logger;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                try
                {
                    await _next(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unhandled exception occurred");
                    await HandleExceptionAsync(context, ex);
                }
            }

            private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
            {
                context.Response.ContentType = "application/json";
                Result<object> response;

                switch (exception)
                {
                    case UnauthorizedAccessException:
                        response = Result<object>.Failure(exception.Message);
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        break;

                    case ArgumentNullException:
                    case ArgumentException:
                        response = Result<object>.Failure(exception.Message);
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;

                    case KeyNotFoundException:
                        response = Result<object>.Failure("Resource not found");
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;

                    case InvalidOperationException:
                        response = Result<object>.Failure(exception.Message);
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;

                    default:
                        response = Result<object>.Failure("An internal server error occurred");
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await context.Response.WriteAsync(jsonResponse);
            }
        }
}
