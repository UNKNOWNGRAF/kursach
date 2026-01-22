using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Models;
//объясните пжпж
namespace WebApplication1.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            // Пропускаем запросы аутентификации
            if (context.Request.Path.StartsWithSegments("/api/Auth"))
            {
                await _next(context);
                return;
            }

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int userIdInt))
            {
                var request = context.Request;
                var originalBodyStream = context.Response.Body;

                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                try
                {
                    await _next(context);

                    responseBody.Seek(0, SeekOrigin.Begin);
                    var responseText = await new StreamReader(responseBody).ReadToEndAsync();
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);

                    // Сохраняем историю запроса
                    var history = new RequestHistory
                    {
                        UserId = userIdInt,
                        Endpoint = request.Path,
                        Method = request.Method,
                        RequestData = $"{request.QueryString}",
                        ResponseData = responseText.Length > 500 ? responseText.Substring(0, 500) + "..." : responseText,
                        CreatedAt = DateTime.UtcNow
                    };

                    dbContext.RequestHistories.Add(history);
                    await dbContext.SaveChangesAsync();
                }
                finally
                {
                    context.Response.Body = originalBodyStream;
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}