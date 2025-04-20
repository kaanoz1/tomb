using FluentValidation;
using tomb.Utility;

namespace tomb.Validation
{
    public class UpdateProfileModel
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Username { get; set; }
        public IFormFile? Image { get; set; }
    }

    public class UpdateProfileValidator : AbstractValidator<UpdateProfileModel>
    {
        private readonly long _maxFileSize = 2 * 1024 * 1024;
        private readonly long _requiredHeight = 1024;
        private readonly long _requiredWidth = 1024;

        public UpdateProfileValidator()
        {
            RuleFor(x => x.Username).NullableUsernameRules();
            RuleFor(x => x.Name).NullableNameRules();
            RuleFor(x => x.Surname).NullableSurnameRules();
            RuleFor(x => x.Image).NullableImageRules(_maxFileSize, _requiredWidth, _requiredHeight);
        }
    }
}
