# Development Plan: User Entity, Authentication & User Management

> **Status**: Draft  
> **Date**: 2025-04-17  
> **Author**: Architect  
> **Scope**: Domain · Infrastructure · Application · API · WPF · Avalonia · Angular

---

## Table of Contents

1. [Step 1 — User Entity](#step-1--user-entity)
2. [Step 2 — Credentials / Login Form](#step-2--credentials--login-form)
3. [Step 3 — User Management (CRUD) Forms](#step-3--user-management-crud-forms)
4. [Cross-Cutting Concerns](#cross-cutting-concerns)
5. [Dependency & Execution Order](#dependency--execution-order)

---

## Step 1 — User Entity

### 1.1 Domain Layer (`BankerDeskOps.Domain`)

#### New Enum — `UserRole`

| Value | Description |
|-------|-------------|
| `Operator` (0) | Day-to-day bank clerk |
| `Manager` (1) | Branch / department manager |
| `Admin` (2) | System administrator |

#### New Enum — `UserStatus`

| Value | Description |
|-------|-------------|
| `Active` (0) | Can log in and operate |
| `Inactive` (1) | Account disabled |
| `Locked` (2) | Temporarily locked (e.g., failed login attempts) |

#### New Entity — `User`

| Property | Type | Constraints |
|----------|------|-------------|
| `Id` | `Guid` | PK, generated client-side |
| `Username` | `string` | Required, max 50, unique |
| `Email` | `string` | Required, max 255, unique, valid email |
| `FirstName` | `string` | Required, max 100 |
| `LastName` | `string` | Required, max 100 |
| `PasswordHash` | `string` | Required (stored as bcrypt/PBKDF2 hash) |
| `Role` | `UserRole` | Required, default `Operator` |
| `Status` | `UserStatus` | Required, default `Active` |
| `LastLoginAt` | `DateTime?` | Nullable, UTC |
| `CreatedAt` | `DateTime` | UTC, default `GETUTCDATE()` |
| `UpdatedAt` | `DateTime` | UTC, default `GETUTCDATE()` |

> **Security note**: `PasswordHash` never leaves the server. It must be excluded from all DTOs and gRPC/REST responses.

### 1.2 Infrastructure Layer (`BankerDeskOps.Infrastructure`)

| Artifact | Details |
|----------|---------|
| `UserConfiguration.cs` | Fluent API: table `Users`, unique indexes on `Username` and `Email`, column types, defaults |
| `AppDbContext.cs` | Add `DbSet<User> Users` |
| `UserRepository.cs` | Implements `IUserRepository` — `GetAllAsync`, `GetByIdAsync`, `GetByUsernameAsync`, `GetByEmailAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync` |
| `DependencyInjection.cs` | Register `IUserRepository → UserRepository` (Scoped) |
| EF Migration | `AddUserEntity` migration |

### 1.3 Application Layer (`BankerDeskOps.Application`)

#### DTOs

| DTO | Fields | Purpose |
|-----|--------|---------|
| `UserDto` | Id, Username, Email, FirstName, LastName, FullName (computed), Role, Status, LastLoginAt, CreatedAt, UpdatedAt | Response DTO — **no PasswordHash** |
| `CreateUserRequest` | Username, Email, FirstName, LastName, Password, Role | Create operation |
| `UpdateUserRequest` | Email, FirstName, LastName, Role | Update operation (username immutable) |
| `LoginRequest` | Username, Password | Authentication |
| `LoginResponse` | Success (bool), Token (string?), UserDto?, ErrorMessage? | Authentication result |

#### Interface — `IUserService`

```
GetAllAsync() → IEnumerable<UserDto>
GetByIdAsync(Guid id) → UserDto?
CreateAsync(CreateUserRequest) → UserDto
UpdateAsync(Guid id, UpdateUserRequest) → UserDto
DeleteAsync(Guid id) → void
ActivateAsync(Guid id) → UserDto
DeactivateAsync(Guid id) → UserDto
AuthenticateAsync(LoginRequest) → LoginResponse
```

#### Service — `UserService`

- Password hashing via `Microsoft.AspNetCore.Identity` hasher or `BCrypt.Net`
- Validation: unique username, unique email, name length, email format
- Authentication: verify hash, update `LastLoginAt`, check `UserStatus`
- Mapping helper: `MapToDto()` (excludes `PasswordHash`)

### 1.4 API Layer (`BankerDeskOps.Api`)

#### REST Controller — `UsersController`

| Method | Route | Action |
|--------|-------|--------|
| `GET` | `/api/users` | List all users |
| `GET` | `/api/users/{id:guid}` | Get user by ID |
| `POST` | `/api/users` | Create user |
| `PUT` | `/api/users/{id:guid}` | Update user |
| `PUT` | `/api/users/{id:guid}/activate` | Activate user |
| `PUT` | `/api/users/{id:guid}/deactivate` | Deactivate user |
| `DELETE` | `/api/users/{id:guid}` | Delete user |
| `POST` | `/api/users/login` | Authenticate |

#### gRPC — `user.proto` + `UserServiceImpl`

- Define `UserMessage`, `CreateUserRequest`, `UpdateUserRequest`, `LoginRequest`, `LoginResponse`
- RPC methods mirroring the REST surface
- Register in `Program.cs`

### 1.5 Tests

| Project | Tests |
|---------|-------|
| `BankerDeskOps.Application.Tests` | `UserServiceTests` — CRUD, authentication, duplicate checks, status transitions |
| `BankerDeskOps.Api.Tests` | `UsersControllerTests` — HTTP status codes, request/response validation |

---

## Step 2 — Credentials / Login Form

> **Goal**: Gate access to WPF, Avalonia, and Angular clients behind a login screen.  
> **Temporary feature**: An **"Anonymous"** checkbox to bypass authentication during development.

### 2.1 API Changes

- `POST /api/users/login` endpoint (already defined above)
- Anonymous mode: When the client sends `{ "username": "anonymous", "password": "" }`, the API returns a success response with a limited/read-only token or a marker DTO (`IsAnonymous = true`)
- Keep it simple: no JWT at this stage — return `LoginResponse` with `Success` flag; clients store the logged-in `UserDto` in memory

### 2.2 WPF Client (`BankerDeskOps.Wpf`)

#### New Artifacts

| Artifact | Details |
|----------|---------|
| `LoginView.xaml` | Username field, Password field, **"Anonymous" CheckBox**, Login button, error message label |
| `LoginView.xaml.cs` | Code-behind (minimal) |
| `LoginViewModel.cs` | `Username`, `Password`, `IsAnonymous` (observable properties); `LoginCommand` (RelayCommand); calls gRPC `UserService.Login` or sets anonymous session |
| `GrpcUserApiService.cs` | Wraps gRPC `UserServiceClient` — `LoginAsync(username, password)` |

#### Navigation Flow

1. `App.xaml.cs` → Show `LoginView` as startup window (instead of `MainWindow`)
2. On successful login → store `UserDto` in a singleton `SessionContext` service
3. Navigate to `MainWindow`; close `LoginView`
4. **Anonymous checkbox** checked → skip gRPC call, populate `SessionContext` with anonymous placeholder
5. On logout → clear `SessionContext`, show `LoginView` again

#### New Service — `SessionContext` (Singleton)

| Property | Type |
|----------|------|
| `CurrentUser` | `UserDto?` |
| `IsAnonymous` | `bool` |
| `IsAuthenticated` | `bool` (computed) |

#### DI Registration

- `SessionContext` → Singleton
- `LoginViewModel` → Transient
- `LoginView` → Transient
- `GrpcUserApiService` → Scoped

### 2.3 Avalonia Client (`BankerDeskOps.Avalonia`)

#### New Artifacts

| Artifact | Details |
|----------|---------|
| `LoginView.axaml` / `.axaml.cs` | Same fields: Username, Password, Anonymous checkbox, Login button |
| `LoginViewModel.cs` | Same MVVM pattern as WPF, uses MVVM Toolkit |
| `GrpcUserApiService.cs` | Same gRPC wrapper |
| `SessionContext.cs` | Singleton — same contract as WPF |

#### Navigation Flow

- Same as WPF: `App.axaml.cs` → resolve `LoginView` first → on success swap to `MainWindow`
- Share the same MVVM Toolkit patterns (`ObservableObject`, `RelayCommand`)

### 2.4 Angular Client (`BankerDeskOps.Web`)

#### New Artifacts

| Artifact | Path | Details |
|----------|------|---------|
| `LoginComponent` | `src/app/features/login/` | Template: username, password, **"Anonymous" checkbox**, login button, error message |
| `AuthService` | `src/app/core/services/auth.service.ts` | `login(username, password): Observable<LoginResponse>`, `logout()`, `isAuthenticated$: BehaviorSubject<boolean>`, `currentUser$: BehaviorSubject<UserDto \| null>` |
| `AuthGuard` | `src/app/core/guards/auth.guard.ts` | `canActivate` — redirects to `/login` if not authenticated |
| `UserDto` / `LoginRequest` / `LoginResponse` | `src/app/core/models/` | TypeScript interfaces |

#### API Service Extension

- Add to `api.service.ts`: `login(req: LoginRequest): Observable<LoginResponse>`

#### Routing Changes (`app.routes.ts`)

```typescript
{ path: 'login', component: LoginComponent }
{ path: '', component: HomeComponent, canActivate: [AuthGuard] }
{ path: 'loans', component: LoansComponent, canActivate: [AuthGuard] }
{ path: 'accounts', component: AccountsComponent, canActivate: [AuthGuard] }
{ path: 'clients', component: ClientsComponent, canActivate: [AuthGuard] }
// Future: { path: 'users', ... }
```

#### Anonymous Mode

- Checkbox on login form toggles anonymous access
- `AuthService.loginAnonymous()` → sets `isAuthenticated$ = true`, `currentUser$ = anonymousPlaceholder`
- No HTTP call required

### 2.5 UI/UX Specification (All Clients)

```
┌─────────────────────────────┐
│        BankerDeskOps        │
│                             │
│   Username:  [___________]  │
│   Password:  [___________]  │
│                             │
│   ☐ Anonymous Access        │
│                             │
│        [ Log In ]           │
│                             │
│   (error message area)      │
└─────────────────────────────┘
```

- When **Anonymous** is checked: Username and Password fields are disabled / grayed out
- Login button label changes to **"Enter as Guest"** when Anonymous is checked
- Error messages: "Invalid credentials", "Account is locked", "Account is inactive"

---

## Step 3 — User Management (CRUD) Forms

> **Goal**: Provide UI for creating, editing, and deleting users in all three clients.

### 3.1 API Requirements

- All endpoints from Step 1 (`GET /api/users`, `POST`, `PUT`, `DELETE`)
- gRPC `UserService` RPC methods
- **Authorization consideration**: Only `Admin` role users (or Anonymous in dev mode) can manage users

### 3.2 WPF Client (`BankerDeskOps.Wpf`)

#### New Artifacts

| Artifact | Details |
|----------|---------|
| `UsersView.xaml` / `.xaml.cs` | DataGrid listing users; toolbar with Add / Edit / Delete buttons |
| `UsersViewModel.cs` | `Users` (ObservableCollection), `SelectedUser`, `LoadUsersCommand`, `AddUserCommand`, `EditUserCommand`, `DeleteUserCommand` |
| `UserDialogView.xaml` / `.xaml.cs` | Modal dialog for Create/Edit: fields for Username, Email, FirstName, LastName, Password (create only), Role dropdown, Status display |
| `UserDialogViewModel.cs` | `IsEditMode`, field properties, `SaveCommand`, validation |

#### DataGrid Columns

| Column | Binding |
|--------|---------|
| Username | `Username` |
| Full Name | `FullName` (FirstName + LastName) |
| Email | `Email` |
| Role | `Role` (enum → display string) |
| Status | `Status` (with color indicator) |
| Last Login | `LastLoginAt` (formatted) |

#### Navigation Integration

- Add `NavigateToUsers()` command in `MainViewModel`
- Add "Users" button in `MainWindow.xaml` sidebar/navigation

#### DI Registration

- `UsersViewModel` → Singleton
- `UsersView` → Singleton
- `UserDialogViewModel` → Transient

### 3.3 Avalonia Client (`BankerDeskOps.Avalonia`)

#### New Artifacts

| Artifact | Details |
|----------|---------|
| `UsersView.axaml` / `.axaml.cs` | DataGrid listing users; same toolbar pattern |
| `UsersViewModel.cs` | Same pattern as WPF — ObservableCollection, CRUD commands |
| `UserDialogView.axaml` / `.axaml.cs` | Modal dialog (Avalonia `Window`) for Create/Edit |
| `UserDialogViewModel.cs` | Same pattern as WPF |

#### Navigation Integration

- Add `NavigateToUsers()` in `MainViewModel`
- Add navigation button in `MainWindow.axaml`

#### DI Registration

- Same pattern as WPF

### 3.4 Angular Client (`BankerDeskOps.Web`)

#### New Artifacts

| Artifact | Path | Details |
|----------|------|---------|
| `UsersComponent` | `src/app/features/users/` | Users list page with Bootstrap table, Add/Edit/Delete actions |
| `UserFormComponent` | `src/app/features/users/user-form/` | Reusable form component for Create and Edit modes |
| `UserService` | `src/app/core/services/user.service.ts` | Wraps `ApiService` user endpoints |
| `UserDto`, `CreateUserRequest`, `UpdateUserRequest` | `src/app/core/models/` | TypeScript interfaces (extend existing barrel export) |

#### API Service Extension

- Add to `api.service.ts`:
  - `getUsers(): Observable<UserDto[]>`
  - `getUserById(id: string): Observable<UserDto>`
  - `createUser(req: CreateUserRequest): Observable<UserDto>`
  - `updateUser(id: string, req: UpdateUserRequest): Observable<UserDto>`
  - `deleteUser(id: string): Observable<void>`
  - `activateUser(id: string): Observable<UserDto>`
  - `deactivateUser(id: string): Observable<UserDto>`

#### Routing

```typescript
{ path: 'users', component: UsersComponent, canActivate: [AuthGuard] }
```

#### UI Components

**Users List (`UsersComponent`)**:
- Bootstrap table with columns: Username, Full Name, Email, Role, Status, Last Login, Actions
- Action buttons per row: Edit (icon), Delete (icon with confirmation)
- "Add User" button at top
- Status badge (green=Active, gray=Inactive, red=Locked)

**User Form (`UserFormComponent`)**:
- Reactive form (`FormGroup`) with validators
- Fields: Username (disabled on edit), Email, First Name, Last Name, Password (required on create, optional on edit), Role (dropdown), Status display
- Save / Cancel buttons
- Inline validation messages

#### Navigation Integration

- Add "Users" link in `app.html` navigation bar

### 3.5 Delete Confirmation (All Clients)

- WPF: `MessageBox.Show()` with Yes/No
- Avalonia: `MessageBoxManager` or custom dialog
- Angular: Bootstrap modal or `window.confirm()` with upgrade path to ng-bootstrap modal

---

## Cross-Cutting Concerns

### Password Security

| Concern | Approach |
|---------|----------|
| Hashing | BCrypt or PBKDF2 via `Microsoft.AspNetCore.Identity.PasswordHasher<User>` |
| Storage | Only `PasswordHash` stored in DB; raw password never persisted |
| Transport | Password sent over HTTPS / gRPC TLS only; excluded from all response DTOs |
| Validation | Minimum 8 characters; at minimum one letter and one digit |

### Anonymous Mode (Temporary)

| Concern | Approach |
|---------|----------|
| Scope | Development / demo only — controlled by a UI checkbox |
| Behavior | Grants read-only or full access (configurable) without real credentials |
| Cleanup | Remove or feature-flag before production deployment |
| Tracking | Logged as anonymous activity for auditing |

### Seed Data

- Create a default `Admin` user on first migration:
  - Username: `admin`
  - Password: `Admin@123` (hashed)
  - Role: `Admin`
  - Status: `Active`

---

## Dependency & Execution Order

```
Step 1 ──► Step 2 ──► Step 3

Step 1: User Entity
  1.1  Domain (Entity + Enums)
  1.2  Infrastructure (Config, DbContext, Repository, Migration)
  1.3  Application (DTOs, Interface, Service)
  1.4  API (Controller, Proto, gRPC Impl, Program.cs registration)
  1.5  Tests

Step 2: Login Form
  2.1  API login endpoint (depends on 1.4)
  2.2  WPF LoginView + SessionContext
  2.3  Avalonia LoginView + SessionContext
  2.4  Angular LoginComponent + AuthService + AuthGuard
  (2.2, 2.3, 2.4 are independent — can be parallelized)

Step 3: User Management CRUD
  3.1  WPF UsersView + UserDialogView
  3.2  Avalonia UsersView + UserDialogView
  3.3  Angular UsersComponent + UserFormComponent
  (3.1, 3.2, 3.3 are independent — can be parallelized)
```

---

> **Next action**: Review and approve this plan, then proceed with Step 1.1 implementation.
