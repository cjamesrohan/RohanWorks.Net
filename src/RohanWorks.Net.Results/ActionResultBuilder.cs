using Microsoft.AspNetCore.Mvc;

namespace RohanWorks.Net.Results;

public sealed class ActionResultBuilder<T>(Result<T> instance, Func<T, IActionResult> onSuccess)
    : ResultBuilderBase<IActionResult, T>(instance, onSuccess);
