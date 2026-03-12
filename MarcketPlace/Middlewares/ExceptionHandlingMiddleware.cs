using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
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

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
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

                DbUpdateException dbEx => CreateResponse(
                    HttpStatusCode.BadRequest,
                    dbEx.InnerException?.Message ?? dbEx.Message),

                _ => CreateResponse(
                    HttpStatusCode.InternalServerError,
                    _env.IsDevelopment()
                        ? $"{exception.GetType().Name}: {exception.Message}"
                        : "حدث خطأ داخلي في السيرفر.")
            };

            context.Response.StatusCode = (int)response.StatusCode;

            var payload = new Dictionary<string, object>
            {
                ["statusCode"] = (int)response.StatusCode,
                ["message"] = response.Message,
                ["traceId"] = context.TraceIdentifier
            };
            if (exception is DbUpdateException dbUpdateEx && dbUpdateEx.InnerException != null && _env.IsDevelopment())
            {
                payload["innerException"] = dbUpdateEx.InnerException.Message;
            }
            if (_env.IsDevelopment() && response.StatusCode == HttpStatusCode.InternalServerError)
            {
                payload["exception"] = exception.GetType().FullName ?? exception.GetType().Name;
                payload["stackTrace"] = exception.StackTrace ?? "";
                if (exception is DbUpdateException dex && dex.InnerException != null)
                {
                    payload["innerStackTrace"] = dex.InnerException.StackTrace ?? "";
                }
            }

            var json = JsonSerializer.Serialize(payload);
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