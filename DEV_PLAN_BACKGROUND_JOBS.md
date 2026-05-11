# Development Plan — `BankerDeskOps.Jobs`

Structured for an AI coding agent: each phase = atomic, verifiable, ≤1 PR. Every task lists **inputs**, **deliverables**, **acceptance check** (a command the agent must run green before claiming done).

**Global rules for the agent**
- Branch per phase: `feat/jobs-phase-{N}-{slug}`.
- Never modify `Domain` entity shapes in a Jobs PR — propose a Domain PR first.
- Every job class implements `IJob` (defined in Phase 1) and ships with at least one xUnit test.
- Run `dotnet build BankerDeskOps.slnx` and `dotnet test` at the end of every task. Don't claim done on red.
- No `Task.Delay` polling in tests — use `IBusinessDateProvider` fakes.

---

## Phase 0 — Pre-flight (no code)

**Goal:** lock the contract before writing anything.

- 0.1 Read `src/BankerDeskOps.Domain/Entities/{Loan,RepaymentSchedule,Entry,Transaction,Contract}.cs`. Produce a short note: which properties are missing for accruals (e.g., `AccruedInterest`, `LastAccrualDate`, `DPD`, `Status` enum values).
- 0.2 Confirm whether `Entry` is a GL-style immutable ledger row. If not, flag it — accrual jobs depend on this.
- 0.3 Confirm MSSQL connection works from Api (`dotnet run --project src/BankerDeskOps.Api`).

**Acceptance:** a `docs/jobs-preflight.md` note listing each gap with file:line refs. Stop and ask the user before Phase 1 if any Domain change is required.

---

## Phase 1 — Foundations (worker host + abstractions)

**Goal:** empty worker running; no business logic yet.

| # | Task | Deliverable | Acceptance |
|---|---|---|---|
| 1.1 | Create `src/BankerDeskOps.Jobs/BankerDeskOps.Jobs.csproj` (`Microsoft.NET.Sdk.Worker`, net8.0). Reference `Application`, `Infrastructure`. | csproj + `Program.cs` with `Host.CreateDefaultBuilder` | `dotnet build` green |
| 1.2 | Add to `BankerDeskOps.slnx`. | slnx updated | `dotnet sln list` shows it |
| 1.3 | Install Hangfire (`Hangfire.AspNetCore`, `Hangfire.SqlServer`). Configure SQL storage on the existing DB, separate schema `hangfire`. | DI wiring in `Program.cs` | Worker starts, Hangfire creates schema tables |
| 1.4 | Define abstractions in `BankerDeskOps.Application/Jobs/`: `IJob`, `IBusinessDateProvider`, `IJobExecutionStore`, `JobRunContext { Guid RunId, DateOnly BusinessDate, bool DryRun }`. | 4 interfaces, 1 record | Compiles, no implementation yet |
| 1.5 | Implement `SystemBusinessDateProvider` (wraps `DateOnly.FromDateTime(DateTime.UtcNow)`) + `FakeBusinessDateProvider` in test project. | 2 classes | Unit test: fake returns set date |
| 1.6 | Create `JobExecution` entity + EF migration (`Id`, `JobName`, `BusinessDate`, `StartedAt`, `FinishedAt`, `Status`, `ErrorMessage`, `IdempotencyKey UNIQUE`). | Entity, Configuration, migration | `dotnet ef database update` succeeds |
| 1.7 | Implement `SqlJobExecutionStore` using `AppDbContext`. Method `TryClaim(key) → bool` using insert-on-conflict semantics. | Class + xUnit test hitting LocalDB | Test green |
| 1.8 | Add Hangfire dashboard endpoint to `BankerDeskOps.Api` behind admin auth. | Endpoint at `/jobs` | Visible when API runs |

**Phase exit check:** worker hosts cleanly, idempotency primitive works, dashboard reachable.

---

## Phase 2 — Outbox (must come before anything that notifies)

**Goal:** transactional outbox so domain TX and side-effects never share a transaction.

- 2.1 Add `OutboxMessage` entity (`Id`, `OccurredAt`, `Type`, `PayloadJson`, `ProcessedAt?`, `Attempts`, `LastError?`). Migration.
- 2.2 Add `IOutboxWriter.Enqueue<T>(T payload)` in `Application`, implementation in `Infrastructure` writing to `AppDbContext.OutboxMessages` (same TX as caller).
- 2.3 Refactor `NotificationService` to call `IOutboxWriter` instead of dispatching directly. Update existing tests.
- 2.4 Implement `OutboxDispatcherJob` — Hangfire recurring every 30s, batch 100, exponential retry on `Attempts`, mark `ProcessedAt`.
- 2.5 Tests: outbox row written inside a rolled-back TX is also rolled back; dispatcher processes only unprocessed; failure increments `Attempts`.

**Acceptance:** integration test — `LoanApplicationService.Approve()` rolled back leaves zero outbox rows.

---

## Phase 3 — EOD scaffolding

**Goal:** the EOD chain can run end-to-end with no-op steps.

- 3.1 Create `Jobs/Eod/` folder with one class per step (9 files from prior message). Each implements `IJob`, body = `return Task.CompletedTask` + structured log.
- 3.2 `EodOrchestrator` — Hangfire job that runs the 9 steps sequentially, fails fast, writes one `JobExecution` row per step.
- 3.3 Register orchestrator on cron `0 5 0 * * *` (00:05 daily, UTC) via `IRecurringJobManager`.
- 3.4 CLI trigger: `dotnet run --project ...Jobs -- run-eod --date 2026-05-11 --dry-run` for manual replay.
- 3.5 Integration test: trigger orchestrator with `FakeBusinessDateProvider`, assert 9 `JobExecution` rows with `Succeeded`.

---

## Phase 4 — Interest Accrual (the heart)

**Goal:** real money math, isolated, tested heavily.

- 4.1 `IDayCountConvention` strategy (ACT/365, ACT/360, 30/360). Default ACT/365. Pure functions, table-driven tests.
- 4.2 `InterestAccrualCalculator` in `Application/Services/Financial/`: input `(principal, annualRate, fromDate, toDate, convention)`, output `decimal`. Property-based test: sum of daily accruals over period == single-period calc within 1e-6.
- 4.3 `InterestAccrualJob` implementation:
  - Query active loans where `LastAccrualDate < businessDate`.
  - Per loan, compute daily accrual, write **immutable** `Entry { Type=InterestAccrual, LoanId, Amount, BusinessDate }`, bump `Loan.LastAccrualDate`.
  - Single DB transaction per loan (not per batch — isolation > throughput here).
  - Idempotency key `accrual:{loanId}:{date}`; re-run returns immediately.
- 4.4 Tests:
  - Re-running same date produces zero new entries.
  - 30-day accrual sum matches manual calc.
  - Loan with `Status=Closed` is skipped.
  - Loan with `LastAccrualDate` 5 days behind catches up with 5 entries.
- 4.5 Wire into EOD orchestrator step 2.

**Acceptance:** seed 100 loans, run EOD, assert `SUM(Entry.Amount where Type=InterestAccrual, date=today) == expected`.

---

## Phase 5 — Repayment lifecycle

Sub-phases each shippable on their own.

- **5.1 RepaymentDueDetectionJob** — flip `RepaymentSchedule.Status` from `Planned` → `Due` where `DueDate = businessDate`.
- **5.2 AutoRepaymentJob** — for each `Due` line with linked `RetailAccount`: debit account, credit GL via `Entry`, mark schedule `Paid` or `PartiallyPaid`. Allocation order **fees → penalty → interest → principal** (extract to `IPaymentAllocator` strategy). Tests for exact-pay, partial, insufficient funds, overpayment.
- **5.3 OverdueClassificationJob** — compute DPD per loan, update `Loan.Status` (`Current/Overdue1_30/Overdue31_60/Overdue61_90/NPL`). Pure function `OverdueClassifier.Classify(dpd) → LoanStatus` — heavily unit-tested boundary cases (DPD=0, 30, 31, 90, 91).
- **5.4 PenaltyInterestJob** — accrue only on overdue principal, separate `Entry.Type=PenaltyAccrual`. Same idempotency pattern as Phase 4.

**Acceptance per sub-phase:** integration test seeding a scripted scenario (loan disbursed day 0, miss payment day 30, partial pay day 45) and asserting expected entries/statuses.

---

## Phase 6 — Disbursement & schedule rebuild

- 6.1 `DisbursementJob` (every 5 min) — pick `LoanApplication.Status=Approved` with `DisbursementDate <= today`, create `Loan` + `Contract` + opening `Entry`, credit `RetailAccount`, emit `LoanDisbursed` outbox event.
- 6.2 `RepaymentScheduleRebuildJob` — on-demand handler; triggered by `RateChanged` outbox event or early-payoff request. Re-amortize using existing `FinancialCalculator`. Old schedule rows archived (`Status=Superseded`), not deleted.
- 6.3 Tests: disburse-then-rebuild produces consistent `Sum(Principal) == OriginalAmount`.

---

## Phase 7 — Periodic & reporting

- 7.1 `DunningJob` daily — emits notification per overdue bucket using templates.
- 7.2 `RateRefreshJob` daily — detects newly active `Rate` rows, emits `RateChanged` per affected loan.
- 7.3 `AIRiskRecalculationJob` nightly — re-runs `AIAnalysisService` on portfolio; flags deteriorating.
- 7.4 `MonthEndJob` (cron `0 0 2 1 * *`) — interest capitalization (where contract allows), customer statements via outbox.
- 7.5 `YearEndJob` — counter resets, ledger partition archive.

---

## Phase 8 — Observability & ops

- 8.1 Structured logging: enrich every log with `BusinessDate`, `JobRunId`, `LoanId` via `LogContext`.
- 8.2 Metrics (OpenTelemetry → Prometheus): `job_duration_seconds{job=…}`, `job_failures_total`, `accrual_sum_eur{date=…}`.
- 8.3 Health checks: `/health/jobs` returns red if last EOD older than 26h.
- 8.4 Runbook in `docs/jobs-runbook.md`: how to replay a failed EOD day, how to back out a wrong accrual.
- 8.5 Alert rules sample for Grafana/Azure Monitor.

---

## Phase 9 — Hardening

- 9.1 Distributed lock around `EodOrchestrator` (`sp_getapplock` or Hangfire `[DisableConcurrentExecution(timeout: 3600)]`).
- 9.2 Chaos test: kill worker mid-accrual, restart, assert no double entries.
- 9.3 Load test: 100k active loans, EOD must complete in <X min (set target with user).
- 9.4 Security review of `/jobs` dashboard (admin-only, audit log on triggers).

---

## How to consume this plan as an agent

1. Work strictly **one phase at a time**. After each phase: open PR, summarize what changed, link new tests, stop.
2. If a task says "ask the user" — do it; don't invent answers.
3. If Domain entities need new properties, **do not** add them inside a Jobs phase — open a separate Domain PR first, get it merged.
4. After Phase 4 ships, request user sign-off on the math (sample real loan, hand-calculated comparison) before continuing.
5. Keep `BankerDeskOps.Jobs` ignorant of HTTP/UI — pure background process consuming `Application` services.
