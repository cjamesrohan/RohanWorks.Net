using global::FluentValidation;

namespace RohanWorks.Net.Options.Validation.FluentValidation;

public static class RuleBuilderExtensions
{
    public static IRuleBuilderOptions<T, TProperty> HttpUrl<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        string propertyName)
        => ruleBuilder.SetValidator(new HttpUrlValidator<T, TProperty>(propertyName));
}
