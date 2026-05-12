using FluentValidation;
using FluentValidation.Validators;

namespace RohanWorks.Net.Options.Validation.FluentValidation;

public class HttpUrlValidator<T, TProperty> : PropertyValidator<T, TProperty>
{
    private readonly string _propertyName;
    public override string Name => "HttpUrlValidator";

    public HttpUrlValidator(string propertyName)
    {
        _propertyName = propertyName;
    }

    public override bool IsValid(ValidationContext<T> context, TProperty value)
    {
        if (value is null) return true;

        return value is string url &&
               Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    protected override string GetDefaultMessageTemplate(string errorCode)
        => $"The {_propertyName} field is not a valid fully-qualified http or https URL.";
}
