namespace VendingMachineApi.Services;

public class ServiceResult<T>
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
        return new ServiceResult<T>() { FailureMessage = message };
    }

    private ServiceResult(T content)
    {
        Content = content;
    }

    private ServiceResult()
    {
    }
}