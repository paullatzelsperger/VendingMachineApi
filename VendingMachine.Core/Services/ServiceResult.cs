namespace VendingMachine.Core.Services;

/// <summary>
/// Generic wrapper to indicate the result of something. Whenever the result of an operation is
/// not exceptional, in which case an exception should be thrown, the ServiceResult is the better approach
/// as it allows for conditional code execution and collection of several operations.
/// </summary>
/// <typeparam name="T">The type of the content</typeparam>
public record ServiceResult<T>
{
    public bool Succeeded => FailureMessage == null;
    public T? Content { get; }
    public string? FailureMessage { get; set; }
    public bool Failed => !Succeeded;

    public static ServiceResult<T> Success(T content)
    {
        return new ServiceResult<T>(content);
    }

    public static ServiceResult<T> Success()
    {
        return new ServiceResult<T>();
    }

    public static ServiceResult<T> Failure(string message)
    {
        return new ServiceResult<T> { FailureMessage = message };
    }

    private ServiceResult(T content)
    {
        Content = content;
    }

    private ServiceResult()
    {
    }
}