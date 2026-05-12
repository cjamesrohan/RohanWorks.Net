using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace RohanWorks.Net.Results;

public abstract class ResultBuilderBase<TResultType, TResponseType>
{
    private readonly Task<Result<TResponseType>> _task;
    private readonly Func<TResponseType, TResultType> _onSuccess;
    private readonly List<(Type ExType, Func<Exception, TResultType> Handler)> _handlers = [];

    protected ResultBuilderBase(Result<TResponseType> instance, Func<TResponseType, TResultType> onSuccess)
        : this(Task.FromResult(instance), onSuccess) { }

    protected ResultBuilderBase(Task<Result<TResponseType>> task, Func<TResponseType, TResultType> onSuccess)
    {
        _task = task;
        _onSuccess = onSuccess;
    }

    public ResultBuilderBase<TResultType, TResponseType> HandleException<TException>(
        Func<TException, TResultType> onFailure) where TException : Exception
    {
        _handlers.Add((typeof(TException), ex => onFailure((TException)ex)));
        return this;
    }

    public TResultType Return() => ReturnAsync().GetAwaiter().GetResult();

    public async Task<TResultType> ReturnAsync()
    {
        Result<TResponseType> instance;
        try { instance = await _task.ConfigureAwait(false); }
        catch (Exception ex) { instance = ex; }
        return Dispatch(instance);
    }

    private TResultType Dispatch(Result<TResponseType> instance)
    {
        if (instance.IsSuccess)
            return _onSuccess(instance.Value!);

        if (instance.Exception is not null)
        {
            foreach (var (exType, handler) in _handlers)
            {
                if (exType.IsInstanceOfType(instance.Exception))
                    return handler(instance.Exception);
            }

            ExceptionDispatchInfo.Capture(instance.Exception).Throw();
            throw null!;
        }

        throw new UnreachableException();
    }
}
