namespace RohanWorks.Net.Results;

public abstract class ResultBuilderBase<TResultType, TResponseType>
{
    private readonly Result<TResponseType> _instance;
    private TResultType? _result;

    protected ResultBuilderBase(Result<TResponseType> instance, Func<TResponseType, TResultType> onSuccess)
    {
        _instance = instance;
        if (instance is { IsSuccess: true, Value: not null })
        {
            _result = onSuccess(instance.Value);
        }
    }

    public ResultBuilderBase<TResultType, TResponseType> HandleException<TException>(
        Func<TException, TResultType> onFailure) where TException : Exception
    {
        if (_instance is { IsSuccess: false, Exception: TException ex } && _result is null)
        {
            _result = onFailure(ex);
        }
        return this;
    }

    public Task<TResultType> ReturnAsync()
    {
        if (_result is not null)
            return Task.FromResult(_result);

        if (_instance.Exception is not null)
            throw _instance.Exception;

        throw new InvalidOperationException("Result is in an unexpected state.");
    }
}
