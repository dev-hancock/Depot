namespace Depot.Auth.Domain.Users;

public record Email
{
    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Email Create(string value)
    {
        return new Email(value);
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