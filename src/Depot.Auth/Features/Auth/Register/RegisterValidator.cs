namespace Depot.Auth.Features.Auth.Register;

using FluentValidation;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.")
            .Matches("^[ \ta-zA-Z0-9._-]+$")
            .WithMessage("Username can only contain letters, numbers, dots, underscores, or hyphens.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches(@"\p{Lu}").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"\p{Ll}").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"\p{Nd}").WithMessage("Password must contain at least one digit.")
            .Matches(@"[^\p{L}\p{Nd}]").WithMessage("Password must contain at least one special character.");
    }
}