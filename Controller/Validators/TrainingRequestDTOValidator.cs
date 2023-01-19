using FluentValidation;
using FluentValidation.Results;
using VRefSolutions.Domain.DTO;

namespace VRefSoltutions.Validators
{
    public class TrainingRequestDTOValidator : AbstractValidator<TrainingRequestDTO>
    {

        public TrainingRequestDTOValidator()
        {
            RuleFor(x => x.Students.Count).Equal(2);
        }
        protected override bool PreValidate(ValidationContext<TrainingRequestDTO> context, ValidationResult result)
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