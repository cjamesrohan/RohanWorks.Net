using Microsoft.AspNetCore.Http;

namespace RohanWorks.Net.Results;

public sealed class ResultBuilder<T>(Result<T> instance, Func<T, IResult> onSuccess)
    : ResultBuilderBase<IResult, T>(instance, onSuccess);
