using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace RohanWorks.Net.Results.Http;

/// <summary>
/// A <see cref="StatusCodeResult"/> that produces a 424 Failed Dependency response.
/// Use when a downstream dependency failure caused this request to fail.
/// </summary>
[DefaultStatusCode(DefaultStatusCode)]
public sealed class FailedDependencyResult : StatusCodeResult
{
    private const int DefaultStatusCode = StatusCodes.Status424FailedDependency;

    public FailedDependencyResult() : base(DefaultStatusCode) { }
}
