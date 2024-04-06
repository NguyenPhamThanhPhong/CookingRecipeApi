using System.ComponentModel.DataAnnotations;

namespace CookingRecipeApi.Helper
{
    public class TimeSpanModelStateValidation : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is TimeSpan timeSpan)
            {
                return timeSpan.TotalSeconds >= 0 
                    ? ValidationResult.Success 
                    : new ValidationResult("Invalid timespan format");
            }

            return new ValidationResult("Invalid timespan format");
        }
    }
}
