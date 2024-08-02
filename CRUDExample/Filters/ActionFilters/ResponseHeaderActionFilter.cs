using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ActionFilters;

public class ResponseHeaderFilterFactoryAttribute(
    string key,
    string value,
    int order)
    : Attribute, IFilterFactory
{
    private readonly string Key = key;
    private readonly string Value = value;
    private readonly int Order = order;

    public bool IsReusable => false;

    // Controller -> Filter Factory -> Filter
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        ResponseHeaderActionFilter filter = serviceProvider
            .GetRequiredService<ResponseHeaderActionFilter>();

        filter.Key = Key;
        filter.Value = Value;
        filter.Order = Order;

        // return filter object
        return filter;
    }
}

public class ResponseHeaderActionFilter(
    ILogger<ResponseHeaderActionFilter> logger)
    : IAsyncActionFilter, IOrderedFilter
{
    public string Key { get; set; }
    public string Value { get; set; }
    public int Order { get; set; }


    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next
    )
    {
        logger.LogInformation(
            "Before logic - ResponseHeaderActionFilter");
        await next(); // calls the subsequent filter or action method

        logger.LogInformation(
            "After logic - ResponseHeaderActionFilter");
        context.HttpContext.Response.Headers[Key] = Value;
    }
}
