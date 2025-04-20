using FluentValidation;
using tomb.Utility;

namespace tomb.Validation
{
    public class TombCreateValidatedModel
    {
        public required string Name { get; set; }

        public string? Description { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

    }



    public class TombCreateValidator : AbstractValidator<TombCreateValidatedModel>
    {
        public TombCreateValidator()
        {
            RuleFor(t => t.Name).TombNameRules();
            RuleFor(t => t.Description).TombDescriptionRules();
            RuleFor(t => t.Latitude).LatitudeRules();
            RuleFor(t => t.Longitude).LongitudeRules();
        }
    }

    public class TombUpdateValidatedModel
    {

        public required long Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

    }

    public class TombUpdateValidator : AbstractValidator<TombUpdateValidatedModel>
    {
        public TombUpdateValidator()
        {
            RuleFor(t => t.Id).TombIdRules();
            RuleFor(t => t.Name).NullableTombNameRules();
            RuleFor(t => t.Description).NullableTombDescriptionRules();
        }
    }


    public class TombIdentifierModel
    {
        public required long Id { get; set; }
    }

    public class TombIdentifierValidator : AbstractValidator<TombIdentifierModel>
    {
        public TombIdentifierValidator()
        {
            RuleFor(t => t.Id).TombIdRules();
        }
    }
}
