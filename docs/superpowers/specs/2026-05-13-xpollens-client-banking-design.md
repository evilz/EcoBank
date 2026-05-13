# EcoBank Xpollens Client Banking Design

Date: 2026-05-13
Status: design approved for implementation planning

## Goal

EcoBank must become a complete client-facing banking application backed by the available Xpollens APIs. The app targets an existing Xpollens user. It must not implement onboarding or user creation flows.

The product must let the client consult and operate their daily banking services: accounts, balances, IBANs, statements, payments, cards, security status, and existing documents when Xpollens exposes them.

## Non-Goals

- No onboarding funnel.
- No back-office or operations/admin console.
- No compliance analyst workflow.
- No document upload by default.
- No user creation flow.
- No direct webhook management UI for the client.

Webhook and callback events can still feed client-facing status, alerts, and refresh states through infrastructure or future backend mediation.

## Current Application Context

The repository is an Avalonia multi-platform application with a clean split:

- `src/App`: shared UI, ViewModels, navigation, styles.
- `src/Core`: domain models, ports, use cases.
- `src/Infrastructure.Xpollens`: HTTP adapters for Xpollens.
- Platform heads: Desktop, Android, iOS, Browser.

Existing capabilities include authentication, user selection, accounts, operations, cards, bank statements, profile, and a responsive shell.

The current shell labels are not fully aligned with banking expectations. The target shell should expose five stable client tabs:

- Accueil
- Comptes
- Paiements
- Cartes
- Profil

## Xpollens Capability Map

The available Xpollens specs cover these client-relevant areas:

- Accounts: account detail, balances, debt balance, additional balance, account limits, virtual IBANs.
- Statements: available bank statements and statement retrieval.
- Operations: account operations and card operations.
- Payments: beneficiaries, SEPA credit transfers, instant payments, SEPA direct debits, mandates, verification of payee where available.
- Cards: physical cards, virtual cards, card detail, card self-care options, card limits, card operations, PIN display, renewals where available.
- Identity and security: existing user profile data, strong customer authentication request.
- Documents: existing KYC and FATCA attachments retrievable by key.

Compliance, AML, PEP, FATCA administration, TIN validation, and webhook registration are platform capabilities but should not surface as admin workflows in the client app.

## Product Structure

### Accueil

The home screen is the daily dashboard.

It should show:

- Total visible balance with hide/show control.
- Main account summaries.
- Useful alerts: pending payment, card status, missing document availability, failed transaction, SCA required.
- Quick actions: transfer, instant transfer, card top-up if supported, create virtual card, view documents.
- Recent account and card operations.

The screen should avoid marketing content and favor dense, scannable banking information.

### Comptes

The accounts area should show a list and account detail view.

List view:

- Account label, account identifier, balance, currency, status.
- Search/filter when multiple accounts exist.
- Quick access to statements and IBANs.

Detail view:

- Main balance.
- Additional balance and debt balance if available.
- IBAN details and virtual IBAN list.
- Account limits.
- Account operations filtered by account.
- Available bank statements with view/download action.

Virtual IBAN creation can be included only if the current user/product allows it. Otherwise the UI should expose read-only virtual IBANs.

### Paiements

The payments area should group money movement and payment recipients.

It should include:

- Beneficiary list.
- Beneficiary detail.
- Add, edit, and delete beneficiary when supported.
- SEPA credit transfer creation.
- Instant payment creation.
- Transfer status tracking.
- Verification of payee before transfer when available.
- SEPA direct debit list.
- Mandate list and mandate authorization when available.

Payment creation must use a step-based flow:

1. Select source account.
2. Select or create beneficiary.
3. Enter amount, currency, label, execution mode.
4. Verify beneficiary/payee when available.
5. Confirm through SCA if required.
6. Show final status and receipt.

### Cartes

The cards area should remain card-centered but become complete.

List view:

- Physical and virtual cards.
- Card visual, masked PAN, status, holder, type.
- Primary actions: lock/unlock, details, limits, operations.

Detail view:

- Card details.
- Self-care options.
- Payment and withdrawal limits.
- Card operations.
- PIN display behind SCA.
- Virtual card creation when available.
- Physical card order/renewal when available.

Sensitive card data must be masked by default and require explicit user intent plus SCA where Xpollens requires it.

### Profil

The profile area should stay client-facing.

It should include:

- Personal information from the selected Xpollens user.
- Security/SCA status and security actions.
- Preferences that are local to the app.
- Documents.
- Help and support.
- Logout.

Documents must be read-only by default. The app should show KYC/FATCA files when existing document keys are available through Xpollens. If no files exist or no keys are exposed, the UI should show an empty state explaining that no document is available for this profile.

## Domain and Architecture

Keep the existing layering:

- Core owns domain models, ports, and use cases.
- Infrastructure.Xpollens owns endpoint-specific HTTP DTOs and mapping.
- App owns ViewModels, navigation state, and Avalonia views.

New modules should be added by capability:

- `Payments`
- `Beneficiaries`
- `Documents`
- `Security`
- `VirtualIbans`
- `CardManagement`

The app should prefer small use cases such as `GetBeneficiariesUseCase`, `CreateSepaTransferUseCase`, `GetUserDocumentsUseCase`, and `RequestStrongAuthenticationUseCase` rather than letting ViewModels call repositories directly.

## Error Handling

Every remote flow must expose:

- Loading state.
- Empty state.
- Recoverable error message.
- Retry action when safe.
- Clear terminal state for submitted payments or card actions.

Payments and card actions must distinguish:

- Validation error.
- Authentication/SCA required.
- Insufficient funds or limit exceeded.
- Pending processing.
- Rejected or failed by provider.
- Unknown technical failure.

## UX Principles

- Mobile-first, responsive to desktop/tablet through the existing Avalonia shell.
- Banking information first, decorative content second.
- Stable navigation with predictable tab names.
- Sensitive data masked by default.
- Explicit confirmation for money movement and card security actions.
- No admin terminology in client screens.
- Documents are visible from Profil and can be linked from Accueil alerts if relevant.

## Testing Strategy

Unit tests:

- Use cases for new domain flows.
- ViewModel state transitions for loading, empty, success, and error states.
- Payment wizard validation.
- Card sensitive-data visibility rules.

Integration-adjacent tests:

- Xpollens adapter mapping from API DTOs to domain models.
- HTTP request construction for key endpoints.
- SCA-required handling.

UI checks:

- Build shared app and desktop head.
- Verify shell navigation labels.
- Verify mobile and desktop layouts for the five main tabs.
- Verify no onboarding UI appears.

## Implementation Phases

1. Shell and navigation alignment.
2. Domain models and ports for missing client capabilities.
3. Account enhancements: balances, limits, virtual IBANs, statements.
4. Payments: beneficiaries, transfer wizard, status tracking.
5. Cards: virtual cards, limits, self-care, PIN/SCA, operations.
6. Documents and profile security.
7. Dashboard aggregation and alerts.
8. Verification, tests, and visual QA.

## Acceptance Criteria

- The app exposes the five client tabs: Accueil, Comptes, Paiements, Cartes, Profil.
- No onboarding or user creation flow is added.
- Existing user documents can be listed and opened when Xpollens provides document keys.
- Accounts include detailed balances, IBANs, limits, statements, and account operations.
- Payments include beneficiaries, SEPA transfer, instant transfer, transfer status, and SCA-aware confirmation.
- Cards include physical/virtual card management, limits, operations, lock/unlock, and PIN access behind SCA when available.
- Client-facing error and empty states are present for every new remote capability.
- Existing architecture boundaries remain intact.
