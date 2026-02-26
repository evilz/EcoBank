# EcoBank

Application bancaire multi-plateformes en **Avalonia .NET 10**, basée sur les APIs **Xpollens**.

![Écran de connexion EcoBank](docs/screenshot-login.png)

## Plateformes cibles

| Plateforme | Projet | TFM |
|---|---|---|
| Windows / macOS / Linux | `src/Desktop` | `net10.0` |
| Android | `src/Android` | `net10.0-android` |
| iOS | `src/iOS` | `net10.0-ios` |
| Browser (WASM) | `src/Browser` | `net10.0-browser` |

## Architecture

```
EcoBank/
├── src/
│   ├── App/                        # Bibliothèque partagée (UI, ViewModels, Services)
│   │   ├── Views/                  # Écrans AXAML (Auth, Home, Accounts, Operations, Cards, Profile)
│   │   ├── ViewModels/             # ViewModels MVVM (CommunityToolkit.Mvvm)
│   │   ├── Services/               # NavigationService, ISecureStorage
│   │   └── Styles/                 # Tokens.axaml, Components.axaml (design system)
│   ├── Core/                       # Domaine métier (sans dépendances UI)
│   │   ├── Domain/                 # Entités : User, Account, Operation, Card
│   │   ├── Ports/                  # Interfaces : IAuthService, IAccountRepository…
│   │   ├── UseCases/               # Cas d'usage : Authenticate, GetAccounts…
│   │   └── Application/            # UserContext (state partagé)
│   ├── Infrastructure.Xpollens/    # Adaptateurs HTTP vers l'API Xpollens
│   │   ├── Auth/                   # XpollensAuthService
│   │   ├── Http/                   # AuthenticatedHandler, CorrelationIdHandler, LoggingHandler
│   │   ├── Accounts/Users/Operations/Cards/
│   │   └── DependencyInjection.cs
│   ├── Shared/                     # Design tokens JSON
│   ├── Desktop/                    # Point d'entrée Desktop (Program.cs)
│   ├── Android/                    # Point d'entrée Android (MainActivity.cs)
│   ├── iOS/                        # Point d'entrée iOS (AppDelegate.cs)
│   └── Browser/                    # Point d'entrée Browser/WASM (Program.cs)
└── EcoBank.slnx
```

## Parcours de navigation

1. **Connexion Xpollens** — Saisie Client ID / Client Secret → authentification OAuth2
2. **Sélection utilisateur** — Liste paginée avec recherche, statut KYC
3. **Shell principal** (responsive)
   - Navigation latérale sur tablette/desktop (≥ 600 px)
   - Barre de navigation basse sur mobile (< 600 px)
4. **Onglets** : Accueil · Comptes · Opérations · Cartes · Profil

## Design System

Les tokens sont définis dans `src/Shared/DesignTokens/tokens.json` et exposés dans
`src/App/Styles/Tokens.axaml` (ResourceDictionary) et `src/App/Styles/Components.axaml` (Styles).

Principaux tokens :
- **Couleurs** : primary `#1A56DB`, success `#0CA678`, error `#E02424`, dark mode complet
- **Espacement** : grille 4dp (xs=4, sm=8, md=12, lg=16, xl=24, xxl=32)
- **Radius** : xs=4, sm=8, md=12, lg=16, xl=24
- **Accessibilité** : touch target minimum 48dp

## Lancer l'application (Desktop)

```bash
dotnet run --project src/Desktop/EcoBank.Desktop.csproj
```

## Construction

```bash
# Desktop
dotnet build src/Desktop/EcoBank.Desktop.csproj

# Android (nécessite le workload Android)
dotnet build src/Android/EcoBank.Android.csproj

# iOS (nécessite le workload iOS + Xcode)
dotnet build src/iOS/EcoBank.iOS.csproj

# Browser/WASM (nécessite le workload wasm-tools)
dotnet build src/Browser/EcoBank.Browser.csproj
```

## Sécurité

- Le `clientSecret` n'est **jamais** persisté en clair ni loggé
- Pipeline HTTP : injection du Bearer token + X-Correlation-ID + logging sans secrets
- Les endpoints Xpollens non documentés sont marqués `// TODO: confirm exact path`
