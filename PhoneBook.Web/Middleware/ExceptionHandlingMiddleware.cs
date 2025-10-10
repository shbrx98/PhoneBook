using PhoneBook.Application.Exceptions;

namespace PhoneBook.Web.Middleware
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
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Business exception occurred");
                context.Response.Redirect($"/Error?message={Uri.EscapeDataString(ex.Message)}");
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Not found exception occurred");
                context.Response.Redirect($"/Error?message={Uri.EscapeDataString(ex.Message)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                context.Response.Redirect("/Error?message=خطای غیرمنتظره‌ای رخ داده است");
            }
        }
    }
}
