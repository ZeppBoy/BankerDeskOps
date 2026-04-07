using System.Net;
using System.Text.Json;

namespace BankerDeskOps.Api.Middleware
{
    /// <summary>
    /// Middleware for handling global exceptions and returning consistent error responses.
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse();

            switch (exception)
            {
                case ArgumentNullException argNull:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Invalid argument: " + argNull.ParamName;
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    break;

                case ArgumentException arg:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = arg.Message;
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    break;

                case InvalidOperationException invOp:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = invOp.Message;
                    response.StatusCode = StatusCodes.Status404NotFound;
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "An unexpected error occurred. Please try again later.";
                    response.StatusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            return context.Response.WriteAsJsonAsync(response);
        }

        /// <summary>
        /// Standard error response model.
        /// </summary>
        private class ErrorResponse
        {
            public int StatusCode { get; set; }
            public string Message { get; set; } = string.Empty;
        }
    }
}
