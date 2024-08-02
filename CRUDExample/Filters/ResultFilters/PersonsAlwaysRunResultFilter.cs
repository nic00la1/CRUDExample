using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ResultFilters;

public class PersonsAlwaysRunResultFilter : IAlwaysRunResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }
}
