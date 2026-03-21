using FluentResults;
using FluentValidation.Results;

namespace Chinook.API.Common.Results;

/// <summary>
/// Wraps a FluentValidation ValidationFailure as a FluentResults IError.
/// </summary>
public sealed record ValidationError(ValidationFailure Failure) : IError
{
    public string Message => $"{Failure.PropertyName}: {Failure.ErrorMessage}";
    public List<IError> Reasons { get; } = [];
    public Dictionary<string, object> Metadata { get; } = new()
    {
        { "PropertyName", Failure.PropertyName },
        { "AttemptedValue", Failure.AttemptedValue ?? "" }
    };

    public override string ToString() => Message;
}
