using FluentValidation;
using FluentValidation.Results;
using VRefSolutions.Domain.DTO;

namespace VRefSoltutions.Validators
{
    public class EventRequestDTOValidator : AbstractValidator<EventRequestDTO>
    {

        public EventRequestDTOValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Name).MinimumLength(1).MaximumLength(1000);
            RuleFor(x => x.Message).NotEmpty();
            RuleFor(x => x.Message).MinimumLength(1).MaximumLength(1000);
            RuleFor(x => x.Symbol).NotEmpty();
            RuleFor(x => x.Symbol).MinimumLength(1).MaximumLength(1000);

            RuleFor(x => x.TimeStamp.Hours).ExclusiveBetween(-1, 24);
            RuleFor(x => x.TimeStamp.Minutes).ExclusiveBetween(-1, 60);
            RuleFor(x => x.TimeStamp.Seconds).ExclusiveBetween(-1, 60);
            RuleFor(x => x.TimeStamp.Miliseconds).ExclusiveBetween(-1, 1000);
        }
        protected override bool PreValidate(ValidationContext<EventRequestDTO> context, ValidationResult result)
        {
            // In combination with a try catch serialization method, the Validator can check for a null object and bad values.
            if (context.InstanceToValidate == null)
            {
                result.Errors.Add(new ValidationFailure("Missing DTO", "No Valid Json Format Supplied."));
                return false;
            }
            return true;
        }
    }
}