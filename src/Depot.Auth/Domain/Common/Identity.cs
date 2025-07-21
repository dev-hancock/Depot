namespace Depot.Auth.Domain.Common;

using ErrorOr;

public abstract record Identity<T> where T : Identity<T>, new()
{
    private readonly Guid _id;

    public Guid Value
    {
        get => _id;
        private init
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("Value cannot be empty");
            }

            _id = value;
        }
    }

    public static ErrorOr<T> TryCreate(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Error.Validation();
        }

        return new T
        {
            Value = value
        };
    }

    public static T Create(Guid value)
    {
        return new T
        {
            Value = value
        };
    }

    public static implicit operator Guid(Identity<T> identity)
    {
        return identity.Value;
    }

    public static implicit operator string(Identity<T> identity)
    {
        return identity.Value.ToString();
    }

    public static T Next()
    {
        return new T
        {
            Value = Guid.NewGuid()
        };
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}