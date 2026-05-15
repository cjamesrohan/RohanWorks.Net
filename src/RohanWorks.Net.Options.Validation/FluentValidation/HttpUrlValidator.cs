using FluentValidation;
using FluentValidation.Validators;

namespace RohanWorks.Net.Options.Validation.FluentValidation;

public class HttpUrlValidator<T, TProperty> : PropertyValidator<T, TProperty>
{
    public override string Name => "HttpUrlValidator";

    public override bool IsValid(ValidationContext<T> context, TProperty value)
    {
        if (value is null) return true;

        return value is string url &&
               Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    protected override string GetDefaultMessageTemplate(string errorCode)
        => "'{PropertyName}' is not a valid fully-qualified http or https URL.";
}
