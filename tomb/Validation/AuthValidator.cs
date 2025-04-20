using FluentValidation;
using tomb.Utility;

namespace tomb.Validation
{
    public class RegisterModel
    {
        public required string Username { get; set; }

        public required string Email { get; set; }

        public required string Password { get; set; }

        public required string Name { get; set; }

        public string? Surname { get; set; }

        public IFormFile? Image { get; set; }
    }
    public class AuthValidator : AbstractValidator<RegisterModel>
    {
        private readonly long _maxFileSize = 2 * 1024 * 1024; //2MB
        private readonly long _requiredHeight = 1024;
        private readonly long _requiredWidth = 1024;

        public AuthValidator()
        {
            RuleFor(r => r.Username).AuthenticationUsernameRules();
            RuleFor(r => r.Email).AuthenticationEmailRules();
            RuleFor(r => r.Password).AuthenticationPasswordRules();
            RuleFor(r => r.Name).AuthenticationNameRules();
            RuleFor(r => r.Surname).AuthenticationSurnameRules();
            RuleFor(r => r.Image).AuthenticationImageRules(_maxFileSize, _requiredWidth, _requiredHeight);
        }

    }

    public class LoginModel
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginValidator : AbstractValidator<LoginModel>
    {
        public LoginValidator()
        {
            RuleFor(r => r.Email).AuthenticationEmailRules();

            RuleFor(l => l.Password).AuthenticationPasswordRules();
        }
    }
}
