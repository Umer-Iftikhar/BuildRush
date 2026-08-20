using System.ComponentModel.DataAnnotations;

namespace BidRush.CustomAttributes
{
    public class FutureDateTimeAttribute : ValidationAttribute
    {
        public FutureDateTimeAttribute()
        {
            ErrorMessage = "Start time must be in the future.";
        }

        protected override ValidationResult? IsValid(
            object? value,
            ValidationContext validationContext)
        {
            if (value is null)
                return ValidationResult.Success;

            if (value is not DateTime dateTime)
                return new ValidationResult("Invalid date and time.");

            if (dateTime <= DateTime.Now)
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }
    }
}
