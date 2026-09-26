namespace SkillSwapAPI.API.Contracts;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public List<string>? Errors { get; init; }

    private ApiResponse() { }

    public static ApiResponse<T> SuccessResponse(T data, string message = "") =>
        new()
        {
            Success = true,
            Message = message,
            Data = data,
        };

    public static ApiResponse<T> FailResponse(string message, List<string>? errors = null) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = errors ?? [],
        };
}