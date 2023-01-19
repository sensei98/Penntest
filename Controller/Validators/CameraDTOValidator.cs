using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using VRefSolutions.Domain.DTO;

namespace VRefSoltutions.Validators
{
    public class CameraDTOValidator : AbstractValidator<CameraDTO>
    {

        public CameraDTOValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Name).Matches("^[a-zA-Z0-9]{2,20}$").WithMessage("Camera Name must be alphanumeric with a length between 2 and 20");
        }
        protected override bool PreValidate(ValidationContext<CameraDTO> context, ValidationResult result)
        {
            // In combination with a try catch serialization method, the Validator can check for a null object and bad values.
            if (context.InstanceToValidate == null)
            {
                result.Errors.Add(new ValidationFailure("Missing DTO", "No Valid Json Format Supplied."));
                return false;
            }
            return true;
        }
        private Boolean IsValidFileName(string name)
        {
            Regex containsABadCharacter = new Regex("["+ Regex.Escape(System.IO.Path.GetInvalidFileNameChars().ToString()) + "]");
            if (containsABadCharacter.IsMatch(name))  
                return false;
            return true;
        }
    }
}