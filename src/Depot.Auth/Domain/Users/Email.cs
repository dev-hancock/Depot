namespace Depot.Auth.Domain.Users;

using ErrorOr;

public record Email
{
    internal Email(string value)
    {
        Value = value;
        Normalized = value.ToLowerInvariant().Trim();
    }

    public string Value { get; }

    public string Normalized { get; }

    public static Email Create(string value)
    {
        return new Email(value);
    }

    public static ErrorOr<Email> TryCreate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation();
        }

        return Create(value);
    }

    public static implicit operator string(Email email)
    {
        return email.Value;
    }

    public override string ToString()
    {
        return Value;
    }
}