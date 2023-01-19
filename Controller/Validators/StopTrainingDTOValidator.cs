using FluentValidation;
using FluentValidation.Results;
using VRefSolutions.Domain.DTO;

namespace VRefSoltutions.Validators
{
    public class StopTrainingDTOValidator : AbstractValidator<StopTrainingDTO>
    {

        public StopTrainingDTOValidator()
        {
            // NotNull only allows True or False for Boolean.
            RuleFor(x => x.EndTrainingSession).NotNull().WithName("EndTrainingSession").WithMessage("No valid boolean value.");
        }
        protected override bool PreValidate(ValidationContext<StopTrainingDTO> context, ValidationResult result)
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