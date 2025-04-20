using FluentValidation;
using SixLabors.ImageSharp;

namespace tomb.Utility
{

    public static class Utility
    {
        public const string DBTypeDateTime = "datetime";

        public const string DBType8bitInteger = "tinyint";

        public const string DBType16bitInteger = "smallint";

        public const string DBType32bitInteger = "int";

        public const string DBType64bitInteger = "bigint";

        public const string DBTypeVARBINARYMAX = "varbinary(max)";

        public const string DBTypeNVARCHARMAX = "NVARCHAR(MAX)";

        public const string DBTypeNVARCHAR1000 = "NVARCHAR(1000)";

        public const string DBTypeNVARCHAR255 = "NVARCHAR(255)";

        public const string DBTypeNVARCHAR250 = "NVARCHAR(250)";

        public const string DBTypeNVARCHAR100 = "NVARCHAR(100)";

        public const string DBTypeNVARCHAR5 = "NVARCHAR(5)";

        public const string DBTypeVARCHARMAX = "varchar(max)";

        public const string DBTypeVARCHAR500 = "VARCHAR(500)";

        public const string DBTypeVARCHAR250 = "VARCHAR(250)";

        public const string DBTypeVARCHAR126 = "VARCHAR(126)";

        public const string DBTypeVARCHAR100 = "VARCHAR(100)";

        public const string DBTypeVARCHAR72 = "VARCHAR(72)";

        public const string DBTypeNVARCHAR50 = "NVARCHAR(50)";

        public const string DBTypeVARCHAR50 = "VARCHAR(50)";

        public const string DBTypeVARCHAR32 = "VARCHAR(32)";

        public const string DBTypeVARCHAR24 = "VARCHAR(24)";

        public const string DBTypeVARCHAR16 = "VARCHAR(16)";

        public const string DBTypeVARCHAR5 = "VARCHAR(5)";

        public const string DBTypeVARCHAR2 = "VARCHAR(2)";

        public const string DBTypeCHAR1 = "CHAR(1)";

        public const string DBTypeUUID = "uniqueidentifier";

        public const string DBDefaultUUIDFunction = "NEWID()";

        public const string DBDefaultDateTimeFunction = "GETUTCDATE()";
    }
    public static class AuthenticationRules
    {
        public static IRuleBuilderOptions<T, string> AuthenticationUsernameRules<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Username is required.")
                .MinimumLength(5).WithMessage("Username must be at least 5 characters long.")
                .MaximumLength(16).WithMessage("Username cannot exceed 16 characters.")
                .Matches("^(?=.*[a-z])[a-z0-9._]+$")
                .WithMessage("Username must contain at least one lowercase letter and can only include lowercase letters, numbers, '.', and '_'.");
        }

        public static IRuleBuilderOptions<T, string> AuthenticationEmailRules<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Email is required.")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.")
                .EmailAddress().WithMessage("Invalid email address format.");
        }

        public static IRuleBuilderOptions<T, string> AuthenticationPasswordRules<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
                .MaximumLength(256).WithMessage("Password cannot exceed 256 characters.");
        }

        public static IRuleBuilderOptions<T, string> AuthenticationNameRules<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(16).WithMessage("Name cannot exceed 16 characters.")
                .Matches("^[A-Za-z]+$").WithMessage("Name must contain only letters (A-Z).");
        }

        public static IRuleBuilderOptions<T, string?> AuthenticationSurnameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .MaximumLength(16).WithMessage("Surname cannot exceed 16 characters.")
                .Matches("^[A-Za-z]*$").WithMessage("Surname must contain only letters (A-Z) if provided.");
        }

        public static IRuleBuilderOptions<T, string?> AuthenticationGenderRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .MaximumLength(1).WithMessage("Gender must be a single character.")
                .Must(g => string.IsNullOrEmpty(g) || g == "M" || g == "F" || g == "U")
                .WithMessage("Invalid gender. Allowed values are 'M', 'F', or 'U'.");
        }

        public static IRuleBuilderOptions<T, IFormFile?> AuthenticationImageRules<T>(this IRuleBuilder<T, IFormFile?> ruleBuilder, long maxFileSize, long requiredWidth, long requiredHeight)
        {
            return ruleBuilder
                .Must(file => file == null || IsAllowedExtension(file)).WithMessage("Only JPEG or JPG files are allowed.")
                .Must(file => file == null || file.Length <= maxFileSize).WithMessage($"Image size must be less than {maxFileSize / (1024 * 1024)} MB.")
                .Must(file => file == null || IsValidImage(file, requiredWidth, requiredHeight)).WithMessage($"Image must be {requiredWidth}x{requiredHeight} pixels and square.");
        }

        public static bool IsAllowedExtension(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg" };
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension);
        }

        public static bool IsValidImage(IFormFile file, long requiredWidth, long requiredHeight)
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var image = Image.Load(stream);
                return image.Width == image.Height && image.Width == requiredWidth && image.Height == requiredHeight;
            }
            catch
            {
                return false;
            }
        }


    }

    public static class NullableUserRules
    {

        public static IRuleBuilderOptions<T, string?> NullableUsernameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .MinimumLength(5).WithMessage("Username must be at least 5 characters long.")
                .MaximumLength(16).WithMessage("Username cannot exceed 16 characters.")
                .Matches("^(?=.*[a-z])[a-z0-9._]+$")
                .WithMessage("Username must contain at least one lowercase letter and can only include lowercase letters, numbers, '.', and '_'.")
                .When(x => ruleBuilder != null); // Applied only if not null
        }

        public static IRuleBuilderOptions<T, string?> NullableNameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .MaximumLength(16).WithMessage("Name cannot exceed 16 characters.")
                .Matches("^[A-Za-z]+$").WithMessage("Name must contain only letters (A-Z).")
                .When(x => ruleBuilder != null);
        }

        public static IRuleBuilderOptions<T, string?> NullableSurnameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .MaximumLength(16).WithMessage("Surname cannot exceed 16 characters.")
                .Matches("^[A-Za-z]*$").WithMessage("Surname must contain only letters (A-Z).")
                .When(x => ruleBuilder != null);
        }

        public static IRuleBuilderOptions<T, IFormFile?> NullableImageRules<T>(this IRuleBuilder<T, IFormFile?> ruleBuilder, long maxFileSize, long requiredWidth, long requiredHeight)
        {
            return ruleBuilder
                .Must(file => file == null || AuthenticationRules.IsAllowedExtension(file)).WithMessage("Only JPEG or JPG files are allowed.")
                .Must(file => file == null || file.Length <= maxFileSize).WithMessage($"Image size must be less than {maxFileSize / (1024 * 1024)} MB.")
                .Must(file => file == null || AuthenticationRules.IsValidImage(file, requiredWidth, requiredHeight)).WithMessage($"Image must be {requiredWidth}x{requiredHeight} pixels and square.");
        }
    }

    public static class TombValidationRules
    {
        public static IRuleBuilderOptions<T, string> TombNameRules<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Name is required.")
                .MinimumLength(1).WithMessage("Name must be at least 1 character long.")
                .MaximumLength(50).WithMessage("Name cannot exceed 50 characters.");
        }

        public static IRuleBuilderOptions<T, string?> TombDescriptionRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .MaximumLength(120).WithMessage("Description cannot exceed 120 characters.");
        }

        public static IRuleBuilderOptions<T, double> LatitudeRules<T>(this IRuleBuilder<T, double> ruleBuilder)
        {
            return ruleBuilder
                .InclusiveBetween(-90.0, 90.0).WithMessage("Latitude must be between -90 and 90 degrees.");
        }

        public static IRuleBuilderOptions<T, double> LongitudeRules<T>(this IRuleBuilder<T, double> ruleBuilder)
        {
            return ruleBuilder
                .InclusiveBetween(-180.0, 180.0).WithMessage("Longitude must be between -180 and 180 degrees.");
        }
    }
    public static class TombUpdateValidationRules
    {
        public static IRuleBuilderOptions<T, long> TombIdRules<T>(this IRuleBuilder<T, long> ruleBuilder)
        {
            return ruleBuilder
                .GreaterThan(0).WithMessage("Tomb ID must be greater than 0.");
        }

        public static IRuleBuilderOptions<T, string?> NullableTombNameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .MinimumLength(1).WithMessage("Name must be at least 1 character long.")
                .MaximumLength(50).WithMessage("Name cannot exceed 50 characters.")
                .When(name => name != null);
        }

        public static IRuleBuilderOptions<T, string?> NullableTombDescriptionRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .MaximumLength(120).WithMessage("Description cannot exceed 120 characters.")
                .When(desc => desc != null);
        }
    }

}
