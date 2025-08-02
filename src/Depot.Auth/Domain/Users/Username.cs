namespace Depot.Auth.Domain.Users;

public record Username
{
    internal Username(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Username Create(string value)
    {
        return new Username(value.Trim());
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