namespace RohanWorks.Net.Results;

public readonly struct Result<T>
{
    public readonly bool IsSuccess;
    public readonly T? Value;
    public readonly Exception? Exception;

    [Obsolete("Default constructor disabled. Use implicit conversion or extension methods.", true)]
    public Result() => throw new InvalidOperationException("Default constructor disabled.");

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Exception = null;
    }

    private Result(Exception exception)
    {
        IsSuccess = false;
        Value = default;
        Exception = exception;
    }

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Exception exception) => new(exception);
}
