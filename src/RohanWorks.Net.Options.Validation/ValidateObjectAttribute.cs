using System.ComponentModel.DataAnnotations;

namespace RohanWorks.Net.Options.Validation;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ValidateObjectAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null) return ValidationResult.Success;

        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(value, new ValidationContext(value), results, validateAllProperties: true);
        if (isValid) return ValidationResult.Success;

        var errors = results.Select(r => r.ErrorMessage).ToList();
        return new ValidationResult($"The {validationContext.DisplayName} object is invalid: {string.Join(' ', errors)}");
    }
}
