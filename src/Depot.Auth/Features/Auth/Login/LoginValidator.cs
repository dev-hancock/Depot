namespace Depot.Auth.Features.Auth.Login;

using FluentValidation;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x)
            .Must(cmd => !string.IsNullOrWhiteSpace(cmd.Email) || !string.IsNullOrWhiteSpace(cmd.Username))
            .WithMessage("Either email or username is required.");

        When(x => !string.IsNullOrWhiteSpace(x.Email),
            () =>
                RuleFor(x => x.Email)
                    .EmailAddress()
                    .WithMessage("Email is not a valid email address."));

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }
}