using Microsoft.AspNetCore.Mvc;

namespace RohanWorks.Net.Results;

public sealed class ActionResultBuilder<T> : ResultBuilderBase<IActionResult, T>
{
    public ActionResultBuilder(Result<T> instance, Func<T, IActionResult> onSuccess)
        : base(instance, onSuccess) { }

    public ActionResultBuilder(Task<Result<T>> task, Func<T, IActionResult> onSuccess)
        : base(task, onSuccess) { }
}
