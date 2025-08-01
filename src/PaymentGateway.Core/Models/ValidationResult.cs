namespace PaymentGateway.Core.Models;

public class ValidationResult
{
    public bool IsValid { get; private set; } = true;
    public List<string> Errors { get; private set; } = new();

    public void AddError(string message)
    {
        Errors.Add(message);
        IsValid = false;
    }

    public static ValidationResult Success() => new() { IsValid = true };

    public static ValidationResult Failure(params string[] errors)
    {
        var result = new ValidationResult();
        foreach (var error in errors)
        {
            result.AddError(error);
        }
        return result;
    }
}