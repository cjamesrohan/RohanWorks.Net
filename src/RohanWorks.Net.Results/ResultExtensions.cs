using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RohanWorks.Net.Results;

public static class ResultExtensions
{
    public static ActionResultBuilder<T> OnSuccess<T>(
        this Result<T> result,
        Func<T, IActionResult> onSuccess)
        => new(result, onSuccess);

    public static ActionResultBuilder<T> OnSuccess<T>(
        this Task<Result<T>> resultTask,
        Func<T, IActionResult> onSuccess)
        => new(resultTask, onSuccess);

    public static ResultBuilder<T> OnSuccess<T>(
        this Result<T> result,
        Func<T, IResult> onSuccess)
        => new(result, onSuccess);

    public static ResultBuilder<T> OnSuccess<T>(
        this Task<Result<T>> resultTask,
        Func<T, IResult> onSuccess)
        => new(resultTask, onSuccess);
}
