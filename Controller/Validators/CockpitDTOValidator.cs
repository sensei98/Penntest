using FluentValidation;
using FluentValidation.Results;
using VRefSolutions.Domain.DTO;

namespace VRefSoltutions.Validators
{
    public class CockpitDTOValidator : AbstractValidator<CockpitDTO>
    {

        public CockpitDTOValidator()
        {
            // Check for duplicate camera names
            RuleFor(x => x.Cameras.GroupBy(x => x.Name)
                   .Where(x => x.Skip(1).Any()).Any()).Equal(false).WithMessage("Camera names are not unique.").WithName("Cameras");
            RuleForEach(x => x.Cameras).SetValidator(new CameraDTOValidator());
        }
        protected override bool PreValidate(ValidationContext<CockpitDTO> context, ValidationResult result)
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