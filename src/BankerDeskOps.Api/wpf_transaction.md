# Development Plan: Transaction Functionality for WPF Client

## Overview

This document outlines the phased development plan for implementing **Transaction management** in the BankerDeskOps WPF client. The backend already has full support for `Transaction` and `Entry` entities via gRPC (proto file, service implementation, repository, and application service). The WPF client currently manages Loans, Retail Accounts, Bank Clients, and Users – but has **no Transaction UI or gRPC client** yet.

### Current State Analysis

| Layer | Status | Details |
|-------|--------|---------|
| Domain Entities | ✅ Done | `Transaction`, `Entry` with enums (`TransactionType`, `TransactionStatus`, `EntryType`) |
| EF Configurations | ✅ Done | `TransactionConfiguration`, `EntryConfiguration` |
| Repository | ✅ Done | `ITransactionRepository`, `IEntryRepository` registered in DI |
| Application Service | ✅ Done | `ITransactionService`, `TransactionService` with `TransferFundsAsync()` |
| gRPC Proto | ❌ Missing | No `transaction.proto` file |
| gRPC Server Impl | ❌ Missing | No `TransactionServiceImpl` mapped in `Program.cs` |
| WPF gRPC Client | ❌ Missing | No `GrpcTransactionApiService` |
| WPF ViewModel | ❌ Missing | No `TransactionsViewModel` |
| WPF View | ❌ Missing | No `TransactionsView.xaml` |
| Navigation | ❌ Missing | No "Transactions" button in MainWindow |

### Architecture Pattern (Follow Existing Convention)

```
┌─────────────────────────────────────────────────────────────┐
│  WPF Client                                                 │
│  ┌──────────┐   ┌──────────────┐   ┌──────────────────┐    │
│  │ View     │◄══│ ViewModel    │◄══│ GrpcClient       │    │
│  │ (XAML)   │   │ (CommunityTk)│   │ (GrpcChannelMgr) │    │
│  └──────────┘   └──────────────┘   └──────────────────┘    │
└──────────────────────────┬──────────────────────────────────┘
                           │ gRPC (Protobuf)
┌──────────────────────────▼──────────────────────────────────┐
│  API Server                                                 │
│  ┌──────────────┐   ┌──────────────┐   ┌─────────────────┐ │
│  │ gRPC Service │◄══│ App Service  │◄══│ Repository      │ │
│  │ Impl         │   │ (ITransaction)│   │ (EF Core)       │ │
│  └──────────────┘   └──────────────┘   └─────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

---

## Phase 1: gRPC Proto Definition and Server-Side Service

**Goal:** Define the protobuf contract for Transaction operations and implement the server-side gRPC service.

### Task 1.1: Create `transaction.proto`

**File:** `src/BankerDeskOps.Api/Protos/transaction.proto`

**Instructions for AI Agent:**
- Create a new proto file following the exact same pattern as `retailaccount.proto`.
- Define these messages and service methods:

```protobuf
syntax = "proto3";
option csharp_namespace = "BankerDeskOps.Api.Protos";
package banker_desk_ops;

import "google/protobuf/empty.proto";

enum TransactionType {
  TRANSACTION_TYPE_UNSPECIFIED = 0;
  TRANSFER = 1;
}

enum TransactionStatus {
  STATUS_UNSPECIFIED = 0;
  PENDING = 1;
  COMPLETED = 2;
  FAILED = 3;
  CANCELLED = 4;
}

message TransactionDto {
  string id = 1;
  int32 transaction_type = 2;
  int32 status = 3;
  string reference_id = 4;
  repeated EntryDto entries = 5;
  string created_at = 6;
}

message EntryDto {
  string id = 1;
  string account_id = 2;
  string account_iban = 10;
  double amount = 3;
  int32 entry_type = 4;
  double balance_after = 5;
  string description = 6;
  string created_at = 7;
}

message TransferFundsRequest {
  string source_account_id = 1;
  string destination_account_id = 2;
  double amount = 3;
  string description = 4;
}

message TransferFundsResponse {
  TransactionDto transaction = 1;
}

message GetAllTransactionsResponse {
  repeated TransactionDto transactions = 1;
}

message GetTransactionByIdRequest {
  string id = 1;
}

message GetTransactionByIdResponse {
  TransactionDto transaction = 1;
}

service TransactionService {
  rpc GetAllTransactions(google.protobuf.Empty) returns (GetAllTransactionsResponse);
  rpc GetTransactionById(GetTransactionByIdRequest) returns (GetTransactionByIdResponse);
  rpc TransferFunds(TransferFundsRequest) returns (TransferFundsResponse);
}
```

**Validation Checklist:**
- [ ] Proto file compiles without errors (`dotnet build`)
- [ ] Enums match the existing `Domain.Enums.TransactionType`, `TransactionStatus`, `EntryType` values
- [ ] Namespace is `BankerDeskOps.Api.Protos`
- [ ] Message field numbers are sequential starting from 1

---

### Task 1.2: Create DTOs for Transaction and Entry (if not already present)

**Files to check:**
- `src/BankerDeskOps.Application/DTOs/TransactionDto.cs`
- `src/BankerDeskOps.Application/DTOs/EntryDto.cs`

**Instructions for AI Agent:**
- Check if these DTOs already exist in the Application layer. If they do, skip this task.
- If missing, create them following the same pattern as `RetailAccountDto`:

```csharp
// TransactionDto.cs
namespace BankerDeskOps.Application.DTOs;

public class TransactionDto
{
    public Guid Id { get; set; }
    public Domain.Enums.TransactionType TransactionType { get; set; }
    public Domain.Enums.TransactionStatus Status { get; set; }
    public string? ReferenceId { get; set; }
    public List<EntryDto> Entries { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

// EntryDto.cs
namespace BankerDeskOps.Application.DTOs;

public class EntryDto
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public Guid AccountId { get; set; }
    public string? AccountIban { get; set; }
    public decimal Amount { get; set; }
    public Domain.Enums.EntryType EntryType { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Validation Checklist:**
- [ ] DTOs are in `BankerDeskOps.Application.DTOs` namespace
- [ ] Properties match the domain entity structure
- [ ] Navigation properties use collections, not raw entities

---

### Task 1.3: Implement `TransactionServiceImpl` (gRPC Server)

**File:** `src/BankerDeskOps.Api/Services/TransactionServiceImpl.cs`

**Instructions for AI Agent:**
- Create a new gRPC service implementation class following the exact pattern of `RetailAccountServiceImpl`.
- Key methods to implement:
  1. `GetAllTransactions()` → calls `_transactionService.GetAllAsync()`
  2. `GetTransactionById()` → calls `_transactionService.GetByIdAsync(id)` with NotFound handling
  3. `TransferFunds()` → calls `_transactionService.TransferFundsAsync(request)` with validation

**Mapping Requirements:**
- Create two private static mapping methods:
  - `MapTransactionToProto(TransactionDto) → Protos.TransactionDto`
  - `MapEntryToProto(EntryDto) → Protos.EntryDto`
- Handle enum conversions between domain enums and proto enums
- Convert `decimal` ↔ `double` for amounts
- Map `EntryDto.AccountIban` to `Protos.EntryDto.account_iban` — the IBAN for the entry's account (looked up from the account entity when building the DTO)

**Error Handling:**
- Wrap `InvalidOperationException` → `StatusCode.NotFound`
- Wrap `ArgumentException` → `StatusCode.InvalidArgument`
- Log all operations with `_logger`

**Validation Checklist:**
- [ ] Class inherits from `TransactionService.TransactionServiceBase` (auto-generated from proto)
- [ ] Constructor takes `ITransactionService` and `ILogger<TransactionServiceImpl>` via DI
- [ ] All three RPC methods are implemented
- [ ] Mapping methods handle null checks for entries list

---

### Task 1.4: Register gRPC Service in API Server

**File:** `src/BankerDeskOps.Api/Program.cs`

**Instructions for AI Agent:**
- Add these two lines to the existing registration block:

```csharp
// In service registration section (after other Scoped registrations):
builder.Services.AddScoped<TransactionServiceImpl>();

// In gRPC mapping section (after other MapGrpcService calls):
app.MapGrpcService<TransactionServiceImpl>();
```

**Validation Checklist:**
- [ ] `TransactionServiceImpl` is registered as `Scoped`
- [ ] `MapGrpcService<TransactionServiceImpl>()` is called before `app.Run()`
- [ ] API server builds and starts without errors

---

## Phase 2: WPF gRPC Client Service

**Goal:** Create the client-side gRPC service that communicates with the Transaction gRPC endpoint.

### Task 2.1: Create `GrpcTransactionApiService`

**File:** `src/BankerDeskOps.Wpf/Services/GrpcTransactionApiService.cs`

**Instructions for AI Agent:**
- Create a new class following the exact pattern of `GrpcRetailAccountApiService`.
- Use `GrpcChannelManager` to get the channel.
- Implement these public methods:

```csharp
public async Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync()
public async Task<TransactionDto?> GetTransactionByIdAsync(Guid id)
public async Task<TransactionDto?> TransferFundsAsync(TransferFundsRequest request)
```

**Mapping Requirements:**
- Create a private static method `MapProtoToDto(Api.Protos.TransactionDto)` that converts proto messages to WPF DTOs.
- Also create `MapEntryProtoToDto(Api.Protos.EntryDto)` for nested entries.
- Handle enum conversions: proto enums → domain enums.

**DTO Definition (WPF-specific if needed):**
If the Application DTOs are not accessible from WPF, define local DTOs in `BankerDeskOps.Wpf.DTOs`:

**Validation Checklist:**
- [ ] Class is in `BankerDeskOps.Wpf.Services` namespace
- [ ] Constructor takes `GrpcChannelManager` and `ILogger<GrpcTransactionApiService>`
- [ ] All methods are async with proper error logging
- [ ] Mapping handles null entries gracefully

---

### Task 2.2: Register gRPC Client in WPF DI Container

**File:** `src/BankerDeskOps.Wpf/App.xaml.cs`

**Instructions for AI Agent:**
- Add this line to the `ConfigureServices` method, alongside other gRPC service registrations:

```csharp
services.AddScoped<GrpcTransactionApiService>();
```

**Validation Checklist:**
- [ ] Registration is placed with other `Grpc*ApiService` registrations
- [ ] Lifetime is `Scoped` (consistent with other services)

---

## Phase 3: WPF ViewModel for Transactions

**Goal:** Create the MVVM ViewModel that manages transaction state, commands, and data binding.

### Task 3.1: Create `TransactionsViewModel`

**File:** `src/BankerDeskOps.Wpf/ViewModels/TransactionsViewModel.cs`

**Instructions for AI Agent:**
- Create a new ViewModel inheriting from `ObservableObject` (CommunityToolkit.Mvvm).
- Follow the exact same pattern as `RetailAccountsViewModel`.

**Required Properties:**

```csharp
[ObservableProperty] private ObservableCollection<TransactionDto> _transactions = new();
[ObservableProperty] private TransactionDto? _selectedTransaction;
[ObservableProperty] private Guid _sourceAccountId = Guid.Empty;
[ObservableProperty] private Guid _destinationAccountId = Guid.Empty;
[ObservableProperty] private decimal _transferAmount;
[ObservableProperty] private string _description = string.Empty;
[ObservableProperty] private bool _isLoading;
[ObservableProperty] private string? _errorMessage;
```

**Required Commands:**

| Command | Purpose | Validation Rules |
|---------|---------|-----------------|
| `LoadTransactionsCommand` | Fetch all transactions from gRPC | None |
| `TransferFundsCommand` | Execute a fund transfer | Source ≠ Destination, Amount > 0, both accounts selected |
| `ClearFormCommand` | Reset form fields | None |

**Constructor:**
- Takes `GrpcTransactionApiService` and `ILogger<TransactionsViewModel>` via DI.
- Also takes `RetailAccountsViewModel` or a shared account list to populate dropdowns for source/destination accounts. **Alternative approach:** inject `GrpcRetailAccountApiService` directly to load available accounts into two separate ObservableCollections for the dropdowns.

**Command Implementation Details:**

1. **LoadTransactionsAsync():**
   - Call `_grpcClient.GetAllTransactionsAsync()`
   - Clear and repopulate `_transactions` ObservableCollection
   - Set `IsLoading = true/false` in try/finally

2. **TransferFundsAsync():**
   - Validate: source account selected, destination account selected, source ≠ destination, amount > 0
   - Call `_grpcClient.TransferFundsAsync(request)`
   - On success: clear form, reload transactions list, show success message
   - On error: set `ErrorMessage`, log exception

3. **LoadAccountsForDropdowns():**
   - Called in constructor or on first navigation
   - Fetches all retail accounts via `GrpcRetailAccountApiService`
   - Populates two separate lists for source/destination dropdowns

**Validation Checklist:**
- [ ] Inherits from `ObservableObject`
- [ ] Uses `[ObservableProperty]` and `[RelayCommand]` attributes
- [ ] All async commands use try/catch/finally with IsLoading state
- [ ] Error messages are user-friendly (not raw exception text)
- [ ] Input validation prevents invalid transfers

---

## Phase 4: WPF View for Transactions

**Goal:** Create the XAML view for transaction management UI.

### Task 4.1: Create `TransactionsView.xaml` and Code-Behind

**Files:**
- `src/BankerDeskOps.Wpf/Views/TransactionsView.xaml`
- `src/BankerDeskOps.Wpf/Views/TransactionsView.xaml.cs`

**Instructions for AI Agent:**
- Create a new WPF UserControl following the exact same layout pattern as `RetailAccountsView.xaml`.

**UI Layout Structure (Grid-based):**

```
┌─────────────────────────────────────────────────────┐
│  Transactions Management                             │
├─────────────────────────────────────────────────────┤
│  Transfer Form:                                     │
│  Source IBAN [Dropdown ▼] Dest IBAN [Dropdown▼]     │
│  Amount: [_______] Description: [_____________]     │
├─────────────────────────────────────────────────────┤
│  [Transfer Funds] [Refresh]                          │
├─────────────────────────────────────────────────────┤
│  Transactions DataGrid:                              │
│  ┌──────┬──────────┬──────────┬──────────┬────────┬──────────┐ │
│  │ ID   │ Type     │ Status   │ Src IBAN │ Dst IBAN│ Created  │ │
│  ├──────┼──────────┼──────────┼──────────┼────────┼──────────┤ │
│  │ ...  │ Transfer │ Completed│ US12...  │ GB29...│ 2024-... │ │
│  └──────┴──────────┴──────────┴──────────┴────────┴──────────┘ │
├─────────────────────────────────────────────────────┤
│  Error Message (if any)                              │
└─────────────────────────────────────────────────────┘