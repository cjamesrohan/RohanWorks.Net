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

        var errorMessages = new List<string>();

        if (_fluentValidator is not null)
        {
            var result = _fluentValidator.Validate(instance);
            if (!result.IsValid)
                errorMessages.AddRange(result.Errors.Select(e => e.ErrorMessage));
        }
        else
        {
            var validationContext = new ValidationContext(instance);
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(instance, validationContext, results, validateAllProperties: true))
                errorMessages.AddRange(results.Select(r => r.ErrorMessage!));
        }

        return Task.FromResult(errorMessages.Count > 0
            ? HealthCheckResult.Unhealthy($"Configuration section '{_sectionKey}' is invalid. {string.Join(", ", errorMessages)}")
            : HealthCheckResult.Healthy($"Configuration section '{_sectionKey}' is correctly configured."));
    }
}
