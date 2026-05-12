using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RohanWorks.Net.Options.Validation.Tests;

public class ConfigResultExtensionsTests
{
    private static IServiceCollection Services => new ServiceCollection();

    private static IConfiguration BuildConfig(Dictionary<string, string?> values)
        => new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    [Fact]
    public void TryValidate_ValidOptions_ReturnsTrue()
    {
        var config = BuildConfig(new()
        {
            ["MyOptions:Name"] = "test",
            ["MyOptions:Count"] = "3"
        });

        var result = Services.ConfigureAndGet<MyOptions>(config.GetSection("MyOptions"));
        var isValid = result.TryValidate(out var errors);

        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void TryValidate_InvalidOptions_ReturnsFalseWithMessages()
    {
        var config = BuildConfig(new()
        {
            ["MyOptions:Name"] = null,
            ["MyOptions:Count"] = "10"
        });

        var result = Services.ConfigureAndGet<MyOptions>(config.GetSection("MyOptions"));
        var isValid = result.TryValidate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().NotBeEmpty();
    }

    [Fact]
    public void TryValidate_IsIdempotent_SeparateCallsDontAccumulateErrors()
    {
        var config = BuildConfig(new() { ["MyOptions:Name"] = null });
        var result = Services.ConfigureAndGet<MyOptions>(config.GetSection("MyOptions"));

        result.TryValidate(out var errors1);
        result.TryValidate(out var errors2);

        errors1.Count.Should().Be(errors2.Count);
    }

    [Fact]
    public void Validate_MissingSection_Throws()
    {
        var config = BuildConfig(new());
        var result = Services.ConfigureAndGet<MyOptions>(config.GetSection("DoesNotExist"));

        var act = () => result.Validate();
        act.Should().Throw<ValidationException>().WithMessage("*DoesNotExist*");
    }

    [Fact]
    public void Validate_InvalidOptions_Throws()
    {
        var config = BuildConfig(new()
        {
            ["MyOptions:Name"] = null
        });

        var result = Services.ConfigureAndGet<MyOptions>(config.GetSection("MyOptions"));
        var act = () => result.Validate();

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void TryValidate_NestedObject_ValidatesChildProperties()
    {
        var config = BuildConfig(new()
        {
            ["MyOptions:Name"] = "ok",
            ["MyOptions:Count"] = "1",
            ["MyOptions:Sub:Value"] = null
        });

        var result = Services.ConfigureAndGet<MyOptions>(config.GetSection("MyOptions"));
        var isValid = result.TryValidate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().ContainMatch("*Sub*");
    }

    private class MyOptions
    {
        [Required] public string? Name { get; set; }
        [Range(1, 5)] public int Count { get; set; } = 1;
        public SubOptions? Sub { get; set; }
    }

    private class SubOptions
    {
        [Required] public string? Value { get; set; }
    }
}
