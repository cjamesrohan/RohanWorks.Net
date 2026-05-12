using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RohanWorks.Net.Results.Http;

namespace RohanWorks.Net.Results.Tests;

public class FailedDependencyTests
{
    [Fact]
    public void FailedDependencyResult_HasStatus424()
    {
        var result = new FailedDependencyResult();

        result.StatusCode.Should().Be(StatusCodes.Status424FailedDependency);
    }

    [Fact]
    public void FailedDependencyObjectResult_HasStatus424AndBody()
    {
        var body = new { Message = "upstream failed" };
        var result = new FailedDependencyObjectResult(body);

        result.StatusCode.Should().Be(StatusCodes.Status424FailedDependency);
        result.Value.Should().BeEquivalentTo(body);
    }

    [Fact]
    public void FailedDependencyObjectResult_AcceptsNullBody()
    {
        var result = new FailedDependencyObjectResult(null);

        result.StatusCode.Should().Be(StatusCodes.Status424FailedDependency);
        result.Value.Should().BeNull();
    }
}
