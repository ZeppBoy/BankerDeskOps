# Jobs Pre-flight — Domain Gap Analysis

**Date:** 2026-05-11  
**Branch:** master  
**Analyst:** Phase 0 scan

---

## 0.1 Missing properties for accruals / jobs

### `Loan.cs` (`src/BankerDeskOps.Domain/Entities/Loan.cs`)

| Missing property | Required by | Notes |
|---|---|---|
| `DateOnly LastAccrualDate` | Phase 4 — `InterestAccrualJob` | Drives "which loans haven't been accrued today"; line 10-49 has no date tracking beyond `CreatedAt` |
| `int DPD` (Days Past Due) | Phase 5.3 — `OverdueClassificationJob` | Computed nightly; stored for reporting |
| `LoanStatus` overdue values | Phase 5.3 — `OverdueClassificationJob` | `LoanStatus` (line 7-34, `LoanStatus.cs`) only has `Pending/Approved/Rejected/Closed/Disbursed`; missing `Current`, `Overdue1_30`, `Overdue31_60`, `Overdue61_90`, `NPL` |
| `Guid? BankClientId` | Phase 6.1 — `DisbursementJob` | `Loan` has `CustomerName` string (line 18) but no FK to `BankClient`; disbursement needs to credit the right `RetailAccount` |

### `RepaymentSchedule.cs` (`src/BankerDeskOps.Domain/Entities/RepaymentSchedule.cs`)

| Missing property | Required by | Notes |
|---|---|---|
| `RepaymentScheduleStatus Status` enum | Phase 5.1 — `RepaymentDueDetectionJob` | No `Status` field exists; needs `Planned/Due/Paid/PartiallyPaid/Superseded` |
| `Guid? RetailAccountId` | Phase 5.2 — `AutoRepaymentJob` | Plan says "for each Due line with linked RetailAccount"; no such link exists |

### `Entry.cs` (`src/BankerDeskOps.Domain/Entities/Entry.cs`)

| Missing property | Required by | Notes |
|---|---|---|
| `DateOnly BusinessDate` | Phase 4 — `InterestAccrualJob` | Jobs write entries dated to `businessDate`, not `DateTime.UtcNow`; missing at line 13-60 |
| `Guid? LoanId` | Phase 4, 5.2, 5.4 | Accrual and penalty entries need a `LoanId` reference; current `Entry` only has `AccountId` |
| Semantic `EntryKind` / `EntryCategory` | Phase 4, 5.4 | `EntryType` (line 34) is only `Debit/Credit`; jobs need `InterestAccrual`, `PenaltyAccrual`, `Disbursement`, `Repayment` etc. |

### `Contract.cs` (`src/BankerDeskOps.Domain/Entities/Contract.cs`)

| Missing property | Required by | Notes |
|---|---|---|
| `bool AllowInterestCapitalization` | Phase 7.4 — `MonthEndJob` | Capitalization is contract-conditional; not present at line 9-51 |

---

## 0.2 `Entry` immutability check

**Finding: `Entry` is NOT a GL-style immutable ledger row.**

Evidence (`src/BankerDeskOps.Domain/Entities/Entry.cs`, lines 37-38):
```csharp
public decimal BalanceAfter { get; set; }  // mutable setter
```

All properties have public setters, and there is no append-only enforcement at the EF configuration level. The accrual jobs in Phase 4 depend on `Entry` being immutable (no updates, ever — only inserts). This must be addressed before Phase 4 ships: either enforce at the EF level (`ValueGeneratedOnAdd`, no update migrations on `Entry`) or document the constraint.

---

## 0.3 DB connectivity check

Cannot run `dotnet run` from analysis mode. Verify manually:

```bash
dotnet run --project src/BankerDeskOps.Api
```

Connection string should be configured in `appsettings.json` / user secrets under `ConnectionStrings:DefaultConnection`.

---

## Summary — what must change before which phase

| Domain change | Needed before | Blocking? |
|---|---|---|
| `Loan.LastAccrualDate` | Phase 4 | Yes — accrual job cannot function without it |
| `LoanStatus` overdue values | Phase 5.3 | Yes |
| `Loan.DPD` | Phase 5.3 | Yes |
| `RepaymentSchedule.Status` enum | Phase 5.1 | Yes |
| `RepaymentSchedule.RetailAccountId` | Phase 5.2 | Yes |
| `Entry.BusinessDate` | Phase 4 | Yes |
| `Entry.LoanId` | Phase 4 | Yes |
| `Entry.EntryKind` enum | Phase 4 | Yes |
| `Contract.AllowInterestCapitalization` | Phase 7.4 | Yes |
| `Entry` immutability (no public setters or EF enforce) | Phase 4 | Strongly recommended |

**Phase 1 (Foundations): zero Domain changes required.** Phase 1 creates only a new `BankerDeskOps.Jobs` project, abstract interfaces, and a `JobExecution` entity in Infrastructure — none of which touch the Domain entities listed above.

**Recommendation:** open a Domain PR (add missing fields/enums) after Phase 1 ships, before starting Phase 4.
