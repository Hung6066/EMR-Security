// API/Middleware/DlpMiddleware.cs
public class DlpMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDlpService _dlpService;

    public DlpMiddleware(RequestDelegate next, IDlpService dlpService)
    {
        _next = next;
        _dlpService = dlpService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        if (context.Response.ContentType?.Contains("application/json") == true)
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(responseBody).ReadToEndAsync();
            responseBody.Seek(0, SeekOrigin.Begin);

            var (isBlocked, modifiedContent) = await _dlpService.ScanAndApplyPolicyAsync(responseText, "API_Response", context.Request.Path);

            if (isBlocked)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentLength = 0;
                await context.Response.WriteAsync("Sensitive data detected. Request blocked.");
            }
            else
            {
                var newBody = Encoding.UTF8.GetBytes(modifiedContent);
                context.Response.Body = originalBodyStream;
                await context.Response.Body.WriteAsync(newBody, 0, newBody.Length);
            }
        }
        else
        {
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}