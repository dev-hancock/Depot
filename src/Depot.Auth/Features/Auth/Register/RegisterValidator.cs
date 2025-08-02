namespace Depot.Auth.Features.Auth.Register;

using FluentValidation;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email is required and must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.");
    }
}