namespace Depot.Auth.Features.Auth.Login;

using FluentValidation;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Email) || !string.IsNullOrWhiteSpace(x.Username))
            .WithMessage("Either email or username is required.");

        When(x => x.Username is not null && x.Email is not null,
            () =>
                RuleFor(x => x)
                    .Must(x => x.Username is null || x.Email is null)
                    .WithMessage("Only one of email or username should be provided."));

        When(x => x.Username is not null,
            () =>
                RuleFor(x => x.Username)
                    .NotEmpty()
                    .Must(y => !y!.Contains('@'))
                    .WithMessage("Username cannot contain '@' character."));

        When(x => x.Email is not null,
            () =>
                RuleFor(x => x.Email)
                    .EmailAddress()
                    .WithMessage("Email is not a valid email address."));

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }
}