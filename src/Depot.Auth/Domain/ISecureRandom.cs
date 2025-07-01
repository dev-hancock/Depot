namespace Depot.Auth.Domain;

public interface ISecureRandom
{
    string Next(int length);
}