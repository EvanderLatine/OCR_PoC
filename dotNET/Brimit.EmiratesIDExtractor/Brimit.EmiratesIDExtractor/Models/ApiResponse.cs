namespace Brimit.EmiratesIDExtractor.Models;

/// <summary>
/// Standardized API response model
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// HTTP response code
    /// </summary>
    public int Response { get; set; }
    
    /// <summary>
    /// Error message (empty string if success)
    /// </summary>
    public string ErrMsg { get; set; } = string.Empty;
    
    /// <summary>
    /// Response data payload
    /// </summary>
    public T? Data { get; set; }
    
    /// <summary>
    /// Creates a success response
    /// </summary>
    public static ApiResponse<T> Success(T data)
    {
        return new ApiResponse<T>
        {
            Response = 200,
            ErrMsg = string.Empty,
            Data = data
        };
    }
    
    /// <summary>
    /// Creates an error response
    /// </summary>
    public static ApiResponse<T> Error(int code, string message)
    {
        return new ApiResponse<T>
        {
            Response = code,
            ErrMsg = message,
            Data = default
        };
    }
}