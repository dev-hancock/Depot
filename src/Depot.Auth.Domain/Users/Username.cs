using System.Text;
using ErrorOr;

namespace Depot.Auth.Domain.Users;

public record Username
{
    internal Username(string value)
    {
        Value = value.Trim();
        Normalized = Value
            .Normalize(NormalizationForm.FormKC)
            .ToLowerInvariant();
    }

    public string Value { get; } = null!;

    public string Normalized { get; } = null!;

    public static Username Create(string username)
    {
        return new Username(username);
    }

    public static implicit operator string(Username email)
    {
        return email.Value;
    }

    public static ErrorOr<Username> TryCreate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation();
        }

        return Create(value);
    }

    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(Normalized);
    }

    public override string ToString()
    {
        return Value;
    }

    public virtual bool Equals(Username? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(Normalized, other.Normalized, StringComparison.Ordinal);
    }
}