namespace CarStoreManager.Application.Common;

public record Result
{
    public bool Success { get; init; }
    public string? Error { get; init; }

    public static Result Ok()
        => new Result { Success = true };

    public static Result Fail(string error)
        => new Result { Success = false, Error = error };
}

public record Result<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }

    public static Result<T> Ok(T data)
        => new Result<T> { Success = true, Data = data };

    public static Result<T> Fail(string error)
        => new Result<T> { Success = false, Error = error };
}