namespace Depot.Auth.Validation;

using System.Reactive.Linq;
using FluentValidation;
using Mestra.Abstractions;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public IObservable<TResponse> Handle(TRequest request, IObservable<TResponse> next)
    {
        var context = new ValidationContext<TRequest>(request);

        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .ToList();

        if (failures.Count != 0)
        {
            return Observable.Throw<TResponse>(new ValidationException(failures));
        }

        return next;
    }
}