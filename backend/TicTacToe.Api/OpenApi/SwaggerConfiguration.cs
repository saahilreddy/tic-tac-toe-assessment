using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TicTacToe.Api.OpenApi;

public sealed class SwaggerConfiguration : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Tic Tac Toe API",
            Version = "v1",
            Description = "REST API for the Tic Tac Toe technical assessment."
        });

        options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
    }
}
