using FluentValidation;
using FluentValidation.Results;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using VRefSolutions.Domain.DTO;

namespace VRefSoltutions.Validators
{
    public class PredictionRequestDTOValidator : AbstractValidator<PredictionRequestDTO>
    {
        public PredictionRequestDTOValidator()
        {
            RuleFor(x => x.File).Custom((file, ctx) =>
            {
                try
                {
                    Image<Rgba32> img = Image.Load<Rgba32>(file);
                }
                catch (UnknownImageFormatException)
                {
                    ctx.AddFailure("The file must be an image.");
                }
            });

            RuleFor(x => x.TimeStamp.Hours).ExclusiveBetween(-1, 24);
            RuleFor(x => x.TimeStamp.Minutes).ExclusiveBetween(-1, 60);
            RuleFor(x => x.TimeStamp.Seconds).ExclusiveBetween(-1, 60);
            RuleFor(x => x.TimeStamp.Miliseconds).ExclusiveBetween(-1, 1000);
        }

        protected override bool PreValidate(ValidationContext<PredictionRequestDTO> context, ValidationResult result)
        {
            if (context.InstanceToValidate == null)
            {
                result.Errors.Add(new ValidationFailure("Missing DTO", "No Valid Json Format Supplied."));
                return false;
            }
            return true;
        }
    }
}