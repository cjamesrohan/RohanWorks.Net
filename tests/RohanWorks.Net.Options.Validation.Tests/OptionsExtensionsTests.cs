using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace RohanWorks.Net.Options.Validation.Tests;

public class OptionsExtensionsTests
{
    private static IServiceCollection Services => new ServiceCollection();

    private static IConfiguration BuildConfig(Dictionary<string, string?> values)
        => new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    [Fact]
    public void ConfigureAndGet_ByTypeName_ReturnsBoundOptions()
    {
        var config = BuildConfig(new()
        {
            ["MyOptions:Name"] = "hello",
            ["MyOptions:Count"] = "7"
        });

        var options = Services.ConfigureAndGet<MyOptions>(config);

        options.Name.Should().Be("hello");
        options.Count.Should().Be(7);
    }

    [Fact]
    public void ConfigureAndGet_BySection_ReturnsBoundOptions()
    {
        var config = BuildConfig(new()
        {
            ["MyOptions:Name"] = "world",
            ["MyOptions:Count"] = "3"
        });

        var options = Services.ConfigureAndGet<MyOptions>(config.GetSection("MyOptions"));

        options.Name.Should().Be("world");
        options.Count.Should().Be(3);
    }

    [Fact]
    public void ConfigureAndGet_MissingSection_ReturnsDefault()
    {
        var config = BuildConfig(new());

        var options = Services.ConfigureAndGet<MyOptions>(config.GetSection("DoesNotExist"));

        options.Should().NotBeNull();
        options.Name.Should().BeNull();
    }

    [Fact]
    public void ConfigureAndGet_RegistersIOptionsInDI()
    {
        var config = BuildConfig(new()
        {
            ["MyOptions:Name"] = "di-test"
        });

        var services = new ServiceCollection();
        services.ConfigureAndGet<MyOptions>(config.GetSection("MyOptions"));

        var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<IOptions<MyOptions>>().Value;

        resolved.Name.Should().Be("di-test");
    }

    private class MyOptions
    {
        [Required] public string? Name { get; set; }
        public int Count { get; set; } = 1;
    }
}
