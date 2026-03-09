using System.Net;
using System.Text.Json;

namespace MarcketPlace.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
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
                _logger.LogError(ex, "Unhandled exception occurred. TraceId: {TraceId}", context.TraceIdentifier);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                KeyNotFoundException => CreateResponse(
                    HttpStatusCode.NotFound,
                    exception.Message),

                InvalidOperationException => CreateResponse(
                    HttpStatusCode.BadRequest,
                    exception.Message),

                UnauthorizedAccessException => CreateResponse(
                    HttpStatusCode.Unauthorized,
                    exception.Message),

                ArgumentException => CreateResponse(
                    HttpStatusCode.BadRequest,
                    exception.Message),

                _ => CreateResponse(
                    HttpStatusCode.InternalServerError,
                    "حدث خطأ داخلي في السيرفر.")
            };

            context.Response.StatusCode = (int)response.StatusCode;

            var json = JsonSerializer.Serialize(new
            {
                statusCode = (int)response.StatusCode,
                message = response.Message,
                traceId = context.TraceIdentifier
            });

            await context.Response.WriteAsync(json);
        }

        private static ExceptionResponse CreateResponse(HttpStatusCode statusCode, string message)
        {
            return new ExceptionResponse
            {
                StatusCode = statusCode,
                Message = message
            };
        }

        private sealed class ExceptionResponse
        {
            public HttpStatusCode StatusCode { get; set; }
            public string Message { get; set; } = default!;
        }
    }
}