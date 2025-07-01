namespace Depot.Auth.Domain;

public readonly struct Unit : IEquatable<Unit>
{
    public static readonly Unit Default = new();

    public override string ToString()
    {
        return "()";
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public override bool Equals(object? obj)
    {
        return obj is Unit;
    }

    public bool Equals(Unit other)
    {
        return true;
    }

    public static bool operator ==(Unit first, Unit second)
    {
        return true;
    }

    public static bool operator !=(Unit first, Unit second)
    {
        return false;
    }
}