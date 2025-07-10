namespace Depot.Auth.Domain.Users;

using ErrorOr;

public class Email
{
    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ErrorOr<Email> New(string value)
    {
        return new Email(value);
    }

    public static Email Create(string value)
    {
        return new Email(value);
    }

    public static implicit operator string(Email email)
    {
        return email.Value;
    }

    public static implicit operator Email(string value)
    {
        return Create(value);
    }
}