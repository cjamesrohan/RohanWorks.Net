using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace RohanWorks.Net.Results.Http;

/// <summary>
/// An <see cref="ObjectResult"/> that produces a 424 Failed Dependency response with a body.
/// Use when a downstream dependency failure caused this request to fail and you have error details to return.
/// </summary>
[DefaultStatusCode(DefaultStatusCode)]
public sealed class FailedDependencyObjectResult : ObjectResult
{
    private const int DefaultStatusCode = StatusCodes.Status424FailedDependency;

    public FailedDependencyObjectResult([ActionResultObjectValue] object? error) : base(error)
    {
        StatusCode = DefaultStatusCode;
    }
}
