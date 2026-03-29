namespace Plannify.Application.Common;

/// <summary>
/// Represents the result of an operation with success/failure handling
/// Eliminates exceptions for business logic errors - only use exceptions for true exceptions
/// </summary>
public abstract record Result
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }

    protected Result(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new SuccessResult();
    public static Result Failure(string errorMessage) => new FailureResult(errorMessage);

    public static Result<T> Success<T>(T value) => new Result<T>.SuccessResult(value);
    public static Result<T> Failure<T>(string errorMessage) => new Result<T>.FailureResult(errorMessage);

    private sealed record SuccessResult : Result
    {
        public SuccessResult() : base(true, null) { }
    }

    private sealed record FailureResult : Result
    {
        public FailureResult(string errorMessage) : base(false, errorMessage) { }
    }
}

public abstract record Result<T> : Result
{
    public T? Value { get; }

    protected Result(bool isSuccess, string? errorMessage, T? value) : base(isSuccess, errorMessage)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new SuccessResult(value);
    public static new Result<T> Failure(string errorMessage) => new FailureResult(errorMessage);

    public sealed record SuccessResult : Result<T>
    {
        public SuccessResult(T value) : base(true, null, value) { }
    }

    public sealed record FailureResult : Result<T>
    {
        public FailureResult(string errorMessage) : base(false, errorMessage, default) { }
    }
}
