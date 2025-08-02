namespace Depot.Auth.Domain.Users;

using ErrorOr;

public record Username
{
    internal Username(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Username Create(string value)
    {
        return new Username(value.ToLowerInvariant().Trim());
    }

    public static ErrorOr<Username> TryCreate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation();
        }

        return Create(value);
    }

    public static implicit operator string(Username email)
    {
        return email.Value;
    }

    public override string ToString()
    {
        return Value;
    }
}