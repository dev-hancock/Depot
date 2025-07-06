namespace Depot.Auth.Domain.Interfaces;

public interface ISecureRandom
{
    string Next(int length);
}