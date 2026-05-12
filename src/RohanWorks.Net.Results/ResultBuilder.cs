using Microsoft.AspNetCore.Http;

namespace RohanWorks.Net.Results;

public sealed class ResultBuilder<T> : ResultBuilderBase<IResult, T>
{
    public ResultBuilder(Result<T> instance, Func<T, IResult> onSuccess)
        : base(instance, onSuccess) { }

    public ResultBuilder(Task<Result<T>> task, Func<T, IResult> onSuccess)
        : base(task, onSuccess) { }
}
