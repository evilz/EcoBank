# Xpollens Client Banking Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the complete client-facing Xpollens banking surface in EcoBank without onboarding or admin workflows.

**Architecture:** Keep the existing Avalonia MVVM split: `Core` owns domain and use cases, `Infrastructure.Xpollens` owns HTTP adapters, and `App` owns ViewModels and XAML. Add client modules for payments, beneficiaries, documents, security, virtual IBANs, and richer card/account management.

**Tech Stack:** .NET 10, Avalonia, CommunityToolkit.Mvvm, Microsoft.Extensions.DependencyInjection, Xpollens HTTP APIs, xUnit v3.

---

### Task 1: Shell and Plan Tracking

**Files:**
- Modify: `src/App/ViewModels/Shell/MainShellViewModel.cs`
- Modify: `src/App/ViewModels/Shell/NavConverters.cs`
- Create: `tests/EcoBank.App.Tests/ViewModels/MainShellViewModelTests.cs`

- [ ] Write a failing test asserting shell labels are `Accueil`, `Comptes`, `Paiements`, `Cartes`, `Profil`.
- [ ] Run `dotnet test tests/EcoBank.App.Tests/EcoBank.App.Tests.csproj --filter MainShellViewModelTests` and verify it fails.
- [ ] Update shell tab order and icons.
- [ ] Run the shell test and verify it passes.

### Task 2: Core Domain and Use Cases

**Files:**
- Create: `src/Core/Domain/Payments/Beneficiary.cs`
- Create: `src/Core/Domain/Payments/PaymentOrder.cs`
- Create: `src/Core/Domain/Payments/PaymentResult.cs`
- Create: `src/Core/Domain/Payments/Mandate.cs`
- Create: `src/Core/Domain/Documents/UserDocument.cs`
- Create: `src/Core/Domain/Security/StrongAuthenticationRequest.cs`
- Create: `src/Core/Domain/Accounts/VirtualIban.cs`
- Create: `src/Core/Ports/IPaymentRepository.cs`
- Create: `src/Core/Ports/IBeneficiaryRepository.cs`
- Create: `src/Core/Ports/IDocumentRepository.cs`
- Create: `src/Core/Ports/ISecurityRepository.cs`
- Create: `src/Core/Ports/IVirtualIbanRepository.cs`
- Create use cases under `src/Core/UseCases/*`.
- Create tests under `tests/EcoBank.App.Tests/UseCases`.

- [ ] Write failing use-case tests for payment validation, document empty state, and SCA request forwarding.
- [ ] Run targeted tests and verify they fail because types do not exist.
- [ ] Add minimal domain records, ports, and use cases.
- [ ] Run targeted tests and verify they pass.

### Task 3: Xpollens Infrastructure

**Files:**
- Create: `src/Infrastructure.Xpollens/Payments/*`
- Create: `src/Infrastructure.Xpollens/Documents/*`
- Create: `src/Infrastructure.Xpollens/Security/*`
- Create: `src/Infrastructure.Xpollens/Accounts/XpollensVirtualIbanRepository.cs`
- Modify: `src/Infrastructure.Xpollens/DependencyInjection.cs`

- [ ] Add repositories for beneficiaries, SEPA transfers, instant payments, mandates, documents, SCA, and virtual IBANs.
- [ ] Use documented Xpollens paths from the specs discovered during design.
- [ ] Keep DTOs internal to infrastructure and map to Core records.
- [ ] Register all repositories in DI.

### Task 4: Payments UI

**Files:**
- Rename conceptually: `ContactViewModel` becomes the payments ViewModel role without breaking existing view registration.
- Modify: `src/App/ViewModels/Contact/ContactViewModel.cs`
- Modify: `src/App/Views/Contact/ContactView.axaml`
- Modify: `src/App/DependencyInjection.cs`

- [ ] Expose beneficiaries, payment modes, form fields, validation messages, submitted payment status, SDD/mandate list.
- [ ] Bind UI to client payment actions.
- [ ] Keep actions disabled when required data is missing.

### Task 5: Accounts, Cards, Documents, and Dashboard

**Files:**
- Modify: `src/App/ViewModels/Accounts/AccountsViewModel.cs`
- Modify: `src/App/Views/Accounts/AccountsView.axaml`
- Modify: `src/App/ViewModels/Cards/CardsViewModel.cs`
- Modify: `src/App/Views/Cards/CardsView.axaml`
- Modify: `src/App/ViewModels/Profile/ProfileViewModel.cs`
- Modify: `src/App/Views/Profile/ProfileView.axaml`
- Modify: `src/App/ViewModels/Home/HomeViewModel.cs`
- Modify: `src/App/Views/Home/HomeView.axaml`

- [ ] Add virtual IBANs, richer balances, limits, statements links, card actions, SCA-aware PIN placeholder, documents list, and dashboard alerts.
- [ ] Preserve current mobile-first layout and existing style resources.

### Task 6: Verification

**Files:**
- Update or create focused tests for ViewModels and use cases.

- [ ] Run `dotnet test tests/EcoBank.App.Tests/EcoBank.App.Tests.csproj`.
- [ ] Run `dotnet build src/Desktop/EcoBank.Desktop.csproj`.
- [ ] Fix compile, binding, or test failures.
- [ ] Report any endpoint that remains best-effort because Xpollens specs expose retrieval by key but no list endpoint.

### Best-effort endpoint notes (implementation)

- Bank statements are currently integrated in retrieval mode (`GetBankStatementAsync(bankStatementId)`), because the client adapters in this implementation do not have a dedicated list flow wired to surface statement identifiers per account in the UI.
- Document download is key-based (`GetDocumentContentAsync(appUserId, key, kind)`), and document listing remains dependent on provider-exposed keys from KYC demand payloads; if no keys are exposed, the app intentionally falls back to empty-state behavior.
