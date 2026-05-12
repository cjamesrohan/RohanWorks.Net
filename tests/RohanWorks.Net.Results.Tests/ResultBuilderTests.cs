using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RohanWorks.Net.Results;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace RohanWorks.Net.Results.Tests;

public class ResultBuilderTests
{
    private static Task<Result<string>> SuccessTask(string value) =>
        Task.FromResult<Result<string>>(value);

    private static Task<Result<string>> FailureTask(Exception ex) =>
        Task.FromResult<Result<string>>(ex);

    private static Task<Result<string>> ThrowingTask() =>
        throw new ArgumentOutOfRangeException("thrown from task");

    // ActionResultBuilder tests

    [Fact]
    public async Task ActionResult_OnSuccess_MapsToOk()
    {
        var result = await SuccessTask("data")
            .OnSuccess(v => (IActionResult)new OkObjectResult(v))
            .ReturnAsync();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be("data");
    }

    [Fact]
    public async Task ActionResult_HandledException_MapsToRegisteredHandler()
    {
        var result = await FailureTask(new ArgumentException("bad"))
            .OnSuccess(v => (IActionResult)new OkObjectResult(v))
            .HandleException<ArgumentException>(ex => new BadRequestObjectResult(ex.Message))
            .ReturnAsync();

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("bad");
    }

    [Fact]
    public async Task ActionResult_UnhandledException_Rethrows()
    {
        var act = async () => await FailureTask(new InvalidOperationException("unhandled"))
            .OnSuccess(v => (IActionResult)new OkObjectResult(v))
            .HandleException<ArgumentException>(ex => new BadRequestObjectResult(ex.Message))
            .ReturnAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("unhandled");
    }

    [Fact]
    public async Task ActionResult_ThrownFromTask_CapturedAndHandled()
    {
        var result = await Task.Run(ThrowingTask)
            .OnSuccess(v => (IActionResult)new OkObjectResult(v))
            .HandleException<ArgumentOutOfRangeException>(ex => new BadRequestObjectResult(ex.Message))
            .ReturnAsync();

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ActionResult_MultipleHandlers_OnlyFirstMatchFires()
    {
        var ex = new ArgumentException("arg");
        var result = await FailureTask(ex)
            .OnSuccess(v => (IActionResult)new OkObjectResult(v))
            .HandleException<ArgumentException>(e => new BadRequestObjectResult("first"))
            .HandleException<ArgumentException>(e => new BadRequestObjectResult("second"))
            .ReturnAsync();

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("first");
    }

    // ResultBuilder (minimal API IResult) tests

    [Fact]
    public async Task IResult_OnSuccess_MapsToOk()
    {
        var result = await SuccessTask("data")
            .OnSuccess(v => HttpResults.Ok(v))
            .ReturnAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task IResult_HandledException_MapsToHandler()
    {
        var result = await FailureTask(new ArgumentException("bad"))
            .OnSuccess(v => HttpResults.Ok(v))
            .HandleException<ArgumentException>(ex => HttpResults.BadRequest(ex.Message))
            .ReturnAsync();

        result.Should().NotBeNull();
    }
}
