namespace POSMT.API.Services;

public enum ResultStatus
{
    Success,
    NotFound,
    Conflict,
    Invalid,
    Unauthorized
}

public class ServiceResult<T>
{
    public ResultStatus Status { get; private init; }
    public T? Data { get; private init; }
    public string? Error { get; private init; }

    public static ServiceResult<T> Ok(T data) => new() { Status = ResultStatus.Success, Data = data };
    public static ServiceResult<T> NotFound(string error) => new() { Status = ResultStatus.NotFound, Error = error };
    public static ServiceResult<T> Conflict(string error) => new() { Status = ResultStatus.Conflict, Error = error };
    public static ServiceResult<T> Invalid(string error) => new() { Status = ResultStatus.Invalid, Error = error };
    public static ServiceResult<T> Unauthorized(string error) => new() { Status = ResultStatus.Unauthorized, Error = error };
}
