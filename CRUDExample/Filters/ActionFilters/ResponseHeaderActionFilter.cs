using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ActionFilters;

public class ResponseHeaderActionFilter : IActionFilter
{
    private readonly string _key;
    private readonly string _value;
    private readonly ILogger<ResponseHeaderActionFilter> _logger;

    public ResponseHeaderActionFilter(
        ILogger<ResponseHeaderActionFilter> logger,
        string key,
        string value
    )
    {
        _key = key;
        _value = value;
        _logger = logger;
    }

    // before
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInformation("{FilterName}.{MethodName} method",
            nameof(ResponseHeaderActionFilter), nameof(OnActionExecuting));
    }

    // after
    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation("{FilterName}.{MethodName} method",
            nameof(ResponseHeaderActionFilter), nameof(OnActionExecuted));

        context.HttpContext.Response.Headers[_key] = _value;
    }
}
