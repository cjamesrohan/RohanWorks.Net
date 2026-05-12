using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace RohanWorks.Net.Options.Validation;

public static class ConfigResultExtensions
{
    public static ConfigResult<T> Validate<T>(this ConfigResult<T> configResult) where T : class, new()
    {
        if (!configResult.Config.Exists())
            throw new ValidationException($"Missing config section '{configResult.Config.Key}'.");

        if (!configResult.TryValidate(out var errorMessages))
            throw new ValidationException(string.Join(' ', errorMessages));

        return configResult;
    }

    public static bool TryValidate<T>(this ConfigResult<T> configResult, out List<string> errorMessages)
        where T : class, new()
    {
        var errors = new Dictionary<string, List<ValidationResult>>();
        ValidateRecursive(configResult.Options, rootTypeName: null, errors);

        errorMessages = errors
            .Select(kv => $"The {kv.Key} object is invalid: {string.Join(' ', kv.Value.Select(r => r.ErrorMessage))}")
            .ToList();

        return errorMessages.Count == 0;
    }

    private static void ValidateRecursive(object? instance, string? rootTypeName, Dictionary<string, List<ValidationResult>> errors)
    {
        if (instance is null) return;

        var type = instance.GetType();
        var typeName = rootTypeName is null ? type.Name : $"{rootTypeName}.{type.Name}";

        var validationContext = new ValidationContext(instance);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(instance, validationContext, validationResults, validateAllProperties: true);

        if (validationResults.Count > 0)
        {
            if (!errors.ContainsKey(typeName))
                errors[typeName] = new List<ValidationResult>();
            errors[typeName].AddRange(validationResults);
        }

        foreach (var property in type.GetProperties())
        {
            if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                var value = property.GetValue(instance);
                ValidateRecursive(value, typeName, errors);
            }
        }
    }
}
