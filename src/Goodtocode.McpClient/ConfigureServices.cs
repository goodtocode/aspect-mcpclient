using Goodtocode.SecuredHttpClient.Middleware;
using Goodtocode.SecuredHttpClient.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Goodtocode.SecuredHttpClient;

public static class ConfigureServices
{
    public static IServiceCollection AddClientCredentialHttpClient(this IServiceCollection services,
        IConfiguration configuration,
        string clientName,
        Uri baseAddress)
    {
        services.AddSingleton<IValidateOptions<ClientCredentialOptions>, ClientCredentialOptionsValidation>();
        services.AddOptions<ClientCredentialOptions>()
        .Bind(configuration.GetSection(nameof(ClientCredentialOptions)))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddScoped<IAccessTokenProvider, ClientCredentialTokenProvider>();
        services.AddTransient<TokenHandler>();

        services.AddHttpClient(clientName, options =>
        {
            options.DefaultRequestHeaders.Clear();
            options.BaseAddress = baseAddress;
        })
            .AddHttpMessageHandler<TokenHandler>();

        return services;
    }
}