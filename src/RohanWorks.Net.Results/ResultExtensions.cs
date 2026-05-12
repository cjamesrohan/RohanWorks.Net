using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RohanWorks.Net.Results;

public static class ResultExtensions
{
    public static ActionResultBuilder<T> OnSuccess<T>(
        this Task<Result<T>> resultTask,
        Func<T, IActionResult> onSuccess)
    {
        try
        {
            var result = resultTask.GetAwaiter().GetResult();
            return new ActionResultBuilder<T>(result, onSuccess);
        }
        catch (Exception ex)
        {
            return new ActionResultBuilder<T>(ex, onSuccess);
        }
    }

    public static ResultBuilder<T> OnSuccess<T>(
        this Task<Result<T>> resultTask,
        Func<T, IResult> onSuccess)
    {
        try
        {
            var result = resultTask.GetAwaiter().GetResult();
            return new ResultBuilder<T>(result, onSuccess);
        }
        catch (Exception ex)
        {
            return new ResultBuilder<T>(ex, onSuccess);
        }
    }
}
