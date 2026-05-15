using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace RohanWorks.Net.Options.Validation;

public class ConfigOptionsHealthCheck<T> : IHealthCheck where T : class
{
    private readonly IConfiguration _configuration;
    private readonly string _sectionKey;
    private readonly AbstractValidator<T>? _fluentValidator;

    public ConfigOptionsHealthCheck(
        IConfiguration configuration,
        string? sectionKey = null,
        AbstractValidator<T>? fluentValidator = null)
    {
        _configuration = configuration;
        _sectionKey = sectionKey ?? typeof(T).Name;
        _fluentValidator = fluentValidator;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var section = _configuration.GetSection(_sectionKey);
        if (!section.Exists())
            return Task.FromResult(HealthCheckResult.Unhealthy($"Configuration section '{_sectionKey}' is missing."));

        var instance = section.Get<T>();
        if (instance is null)
            return Task.FromResult(HealthCheckResult.Unhealthy($"Failed to bind configuration section '{_sectionKey}' to {typeof(T).Name}."));

        List<string> errorMessages;

        if (_fluentValidator is not null)
        {
            var result = _fluentValidator.Validate(instance);
            errorMessages = result.IsValid
                ? []
                : result.Errors.Select(e => e.ErrorMessage).ToList();
        }
        else
        {
            errorMessages = GetValidationErrors(instance);
        }

        return Task.FromResult(errorMessages.Count > 0
            ? HealthCheckResult.Unhealthy($"Configuration section '{_sectionKey}' is invalid. {string.Join(", ", errorMessages)}")
            : HealthCheckResult.Healthy($"Configuration section '{_sectionKey}' is correctly configured."));
    }

    private static List<string> GetValidationErrors(object instance)
    {
        var errorDict = new Dictionary<string, List<ValidationResult>>();
        ValidateRecursive(instance, null, errorDict, new HashSet<object>(ReferenceEqualityComparer.Instance));

        return errorDict
            .Select(kv => $"The {kv.Key} object is invalid: {string.Join(' ', kv.Value.Select(r => r.ErrorMessage))}")
            .ToList();
    }

    private static void ValidateRecursive(
        object? instance,
        string? rootTypeName,
        Dictionary<string, List<ValidationResult>> errors,
        HashSet<object> visited)
    {
        if (instance is null) return;
        if (!visited.Add(instance)) return;

        var type = instance.GetType();
        var typeName = rootTypeName is null ? type.Name : $"{rootTypeName}.{type.Name}";

        var validationContext = new ValidationContext(instance);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(instance, validationContext, validationResults, validateAllProperties: true);

        if (validationResults.Count > 0)
        {
            if (!errors.TryGetValue(typeName, out var list))
                errors[typeName] = list = [];
            list.AddRange(validationResults);
        }

        foreach (var property in type.GetProperties())
        {
            if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                var value = property.GetValue(instance);
                ValidateRecursive(value, typeName, errors, visited);
            }
        }
    }
}
