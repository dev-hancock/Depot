namespace Depot.Auth.Services;

using Mestra.Abstractions;

public class Mediator
{
    private static IMediator? _mediator;

    public static IMediator Instance
    {
        get => _mediator ?? throw new InvalidOperationException("Mediator not initialized");
        set => _mediator = value;
    }
}