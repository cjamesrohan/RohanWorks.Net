# RohanWorks.Net

A collection of focused .NET libraries for ASP.NET Core applications.

| Package | Description |
|---|---|
| [`RohanWorks.Net.Results`](src/RohanWorks.Net.Results) | `Result<T>` pattern with fluent builder for HTTP response mapping |
| [`RohanWorks.Net.Options.Validation`](src/RohanWorks.Net.Options.Validation) | Startup configuration validation with DataAnnotations and health check integration |
| [`RohanWorks.Net.Testing`](src/RohanWorks.Net.Testing) | Fluent `ILogger<T>` assertion helpers over Moq |

## Sample

[`samples/RohanWorks.Net.Sample.Api`](samples/RohanWorks.Net.Sample.Api) is a working ASP.NET Core API that demonstrates all three packages together — `Result<T>` in both controller and minimal API endpoints, startup options validation, and health check integration.

## License

MIT
