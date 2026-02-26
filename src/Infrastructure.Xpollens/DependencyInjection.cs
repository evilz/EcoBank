using EcoBank.Core.Application;
using EcoBank.Core.Ports;
using EcoBank.Infrastructure.Xpollens.Accounts;
using EcoBank.Infrastructure.Xpollens.Auth;
using EcoBank.Infrastructure.Xpollens.Cards;
using EcoBank.Infrastructure.Xpollens.Http;
using EcoBank.Infrastructure.Xpollens.Operations;
using EcoBank.Infrastructure.Xpollens.Users;
using Microsoft.Extensions.DependencyInjection;

namespace EcoBank.Infrastructure.Xpollens;

public static class DependencyInjection
{
    public static IServiceCollection AddXpollensInfrastructure(
        this IServiceCollection services,
        string baseUrl = "https://api.xpollens.com/") // TODO: confirm base URL from docs
    {
        services.AddSingleton<UserContext>();
        services.AddTransient<AuthenticatedHandler>();
        services.AddTransient<CorrelationIdHandler>();
        services.AddTransient<LoggingHandler>();

        services.AddHttpClient<IAuthService, XpollensAuthService>(c => c.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<CorrelationIdHandler>()
            .AddHttpMessageHandler<LoggingHandler>();

        services.AddHttpClient<IUserRepository, XpollensUserRepository>(c => c.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<AuthenticatedHandler>()
            .AddHttpMessageHandler<CorrelationIdHandler>()
            .AddHttpMessageHandler<LoggingHandler>();

        services.AddHttpClient<IAccountRepository, XpollensAccountRepository>(c => c.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<AuthenticatedHandler>()
            .AddHttpMessageHandler<CorrelationIdHandler>()
            .AddHttpMessageHandler<LoggingHandler>();

        services.AddHttpClient<IOperationRepository, XpollensOperationRepository>(c => c.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<AuthenticatedHandler>()
            .AddHttpMessageHandler<CorrelationIdHandler>()
            .AddHttpMessageHandler<LoggingHandler>();

        services.AddHttpClient<ICardRepository, XpollensCardRepository>(c => c.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<AuthenticatedHandler>()
            .AddHttpMessageHandler<CorrelationIdHandler>()
            .AddHttpMessageHandler<LoggingHandler>();

        return services;
    }
}
