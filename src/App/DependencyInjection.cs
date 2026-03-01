using EcoBank.App.Services;
using EcoBank.App.ViewModels;
using EcoBank.App.ViewModels.Accounts;
using EcoBank.App.ViewModels.Auth;
using EcoBank.App.ViewModels.Cards;
using EcoBank.App.ViewModels.Contact;
using EcoBank.App.ViewModels.Home;
using EcoBank.App.ViewModels.Operations;
using EcoBank.App.ViewModels.Profile;
using EcoBank.App.ViewModels.Shell;
using EcoBank.Core.Ports;
using EcoBank.Core.UseCases.Accounts;
using EcoBank.Core.UseCases.Auth;
using EcoBank.Core.UseCases.Cards;
using EcoBank.Core.UseCases.Operations;
using EcoBank.Core.UseCases.Users;
using EcoBank.Infrastructure.Xpollens;
using Microsoft.Extensions.DependencyInjection;

namespace EcoBank.App;

public static class DependencyInjection
{
    public static IServiceCollection AddApp(this IServiceCollection services)
    {
        // Infrastructure
        services.AddXpollensInfrastructure();
        services.AddSingleton<ISecureStorage, FileSecureStorage>();
        services.AddSingleton<ProfileService>();

        // Use Cases
        services.AddTransient<AuthenticateUseCase>();
        services.AddTransient<GetUsersUseCase>();
        services.AddTransient<GetUserUseCase>();
        services.AddTransient<SelectUserUseCase>();
        services.AddTransient<GetAccountsUseCase>();
        services.AddTransient<GetBankStatementUseCase>();
        services.AddTransient<GetOperationsUseCase>();
        services.AddTransient<GetCardsUseCase>();
        services.AddTransient<GetCardOperationsUseCase>();
        services.AddTransient<ToggleCardLockUseCase>();

        // Navigation
        services.AddSingleton<INavigationService, NavigationService>();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<UserSelectionViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<AccountsViewModel>();
        services.AddTransient<AccountDetailViewModel>();
        services.AddTransient<OperationsViewModel>();
        services.AddTransient<OperationDetailViewModel>();
        services.AddTransient<CardsViewModel>();
        services.AddTransient<CardDetailViewModel>();
        services.AddTransient<ContactViewModel>();
        services.AddTransient<ProfileViewModel>();
        services.AddTransient<MainShellViewModel>();

        return services;
    }
}
