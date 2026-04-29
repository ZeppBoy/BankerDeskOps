## 2. Database Design
Tables:
1. Products 
   - **productId** PK (Guid)
   - term INT
   - minAmount DECIMAL(18,2)
   - maxAmount DECIMAL(18,2)
   - currencyId FK (Guid) REFERENCES Currencies(currencyId)
   - description VARCHAR(MAX)
   - fees TEXT JSON -- Store serialized list of DisbursementFee objects
   - commissions TEXT JSON -- Store serialized list of Commission objects
2. Rates 
   - **rateId** PK (Guid)
   - sinceDate DATETIME
   - toDate DATETIME
   - rateType VARCHAR(50) -- 'Fixed' or 'Variable'
   - value DECIMAL(18,4)
3. Currencies 
   - **currencyId** PK (Guid)
   - code VARCHAR(10)
   - name VARCHAR(100)
4. Fees 
   - **feeId** PK (Guid)
   - type VARCHAR(50) -- 'Disbursement' or 'Commitment'
   - amount DECIMAL(18,2)
5. Commissions 
   - **commissionId** PK (Guid)
   - type VARCHAR(50) -- 'Application' or 'Origination'
   - amount DECIMAL(18,2)
6. RepaymentSchedules 
   - **scheduleId** PK (Guid)
   - loanApplicationId FK (Guid) REFERENCES LoanApplications(loanId)
   - plannedDate DATETIME
   - capital DECIMAL(18,2)
   - interest DECIMAL(18,2)
   - saldo DECIMAL(18,2)
   - eir DECIMAL(18,4)
7. LoanApplications 
   - **loanId** PK (Guid)
   - productId FK (Guid) REFERENCES Products(productId)
   - customerId Guid
   - pendingDate DATETIME
   - disbursementDate DATETIME
   - approvedDate DATETIME?
   - rejectedDate DATETIME?
   - status VARCHAR(50) -- 'Pending', 'Approved', 'Rejected'
   - totalAmount DECIMAL(18,2)
   - repaymentPlan TEXT JSON -- Store serialized RepaymentSchedule object
## 4. Business Logic Implementation
Key Features:
1. EIR Calculation using XIRR formula:
   - Must include all fees and commissions in calculation (disbursement fee, commitment fee, etc.)
   - Formula input: cash flows (initial loan amount minus fees/commissions, periodic interest payments, principal repayments) and dates.
   - Algorithm: Newton-Raphson or similar iterative method to find the root of the net present value equation.

**Example Code for EIR Calculation:**
```csharp
public class FinancialCalculator
{
    public decimal CalculateEIR(decimal[] cashFlows, DateTime[] dates)
    {
        double guess = 0.1; // Initial guess
        double tolerance = 1e-5;
        int maxIterations = 1000;
        double npv = NetPresentValue(cashFlows, dates, guess);
        for (int i = 0; i < maxIterations && Math.Abs(npv) > tolerance; i++)
        {
            guess -= npv / DerivativeNetPresentValue(cashFlows, dates, guess);
            npv = NetPresentValue(cashFlows, dates, guess);
        }
        return (decimal)guess;
    }

    private double NetPresentValue(decimal[] cashFlows, DateTime[] dates, double rate)
    {
        double npv = 0.0;
        for (int i = 0; i < cashFlows.Length; i++)
        {
            double period = (dates[i] - dates[0]).TotalDays / 365.0;
            npv += (double)cashFlows[i] / Math.Pow(1 + rate, period);
        }
        return npv;
    }

    private double DerivativeNetPresentValue(decimal[] cashFlows, DateTime[] dates, double rate)
    {
        double derivativeNpv = 0.0;
        for (int i = 0; i < cashFlows.Length; i++)
        {
            double period = (dates[i] - dates[0]).TotalDays / 365.0;
            derivativeNpv -= (double)cashFlows[i] * period * Math.Pow(1 + rate, -period - 1);
        }
        return derivativeNpv;
    }
}
```
2. RecalculateSchedule() Method:
   - Allow schedule recalculation based on:
      * Interest rate changes
      * Term adjustments
      * Repayment plan modifications
      * Fee/commission updates

**Example Code for RecalculateSchedule:**
```csharp
public class LoanService
{
    public void RecalculateSchedule(LoanApplication loanApplication, List<RepaymentSchedule> currentSchedule)
    {
        // Clear existing schedule
        loanApplication.RepaymentPlan.Clear();

        // Calculate new cash flows and dates based on changes in interest rate, term, etc.
        decimal[] cashFlows = GetCashFlows(loanApplication);
        DateTime[] dates = GetPaymentDates(loanApplication);

        // Calculate EIR using FinancialCalculator
        var financialCalculator = new FinancialCalculator();
        loanApplication.EIR = financialCalculator.CalculateEIR(cashFlows, dates);

        // Generate new repayment schedule based on recalculated EIR
        List<RepaymentSchedule> newSchedule = GenerateNewRepaymentSchedule(loanApplication, cashFlows, dates);
        loanApplication.RepaymentPlan.AddRange(newSchedule);
    }

    private decimal[] GetCashFlows(LoanApplication loanApplication)
    {
        // Implement logic to calculate cash flows based on loan details and changes
        return new decimal[] { -loanApplication.TotalAmount }; // Example initial payment
    }

    private DateTime[] GetPaymentDates(LoanApplication loanApplication)
    {
        // Implement logic to calculate payment dates based on term and other details
        return new DateTime[] { loanApplication.PendingDate.AddDays(loanApplication.Term * 30) }; // Example date calculation
    }

    private List<RepaymentSchedule> GenerateNewRepaymentSchedule(LoanApplication loanApplication, decimal[] cashFlows, DateTime[] dates)
    {
        // Implement logic to generate repayment schedule based on recalculated EIR and payment details
        return new List<RepaymentSchedule>();
    }
}
```
3. Validation Rules:
   - Minimum term/amount validation
   - Maximum allowed rates
   - Proper currency checks
   - Required fields validation
   - Data integrity constraints
4. Security Considerations:
   - Financial data encryption at rest and in transit
   - Role-based access control for forms
   - Audit logs for financial calculations
   - Proper error handling and logging
## 6. Expected Tech Stack
- **Language**: C# (.NET 8+)
- **Framework**: Windows Forms or .NET MAUI (for desktop)
- **ORM**: Entity Framework Core
- **Database**: SQL Server / PostgreSQL
- **Testing**: xUnit / NUnit
- **Design Pattern**: Repository, Service, and Unit of Work patterns
1. Requirements Analysis & Design 
   - Finalize requirements with stakeholders
   - Create database schema
   - Define entity relationships
2. Database Development:
   - Implement tables and indexes
   - Set up foreign key constraints
   - Add validation triggers if needed
3. Entity Framework Layer:
   - Create EF Core models
   - Implement repository pattern
   - Set up proper mapping configuration
4. Business Logic Layer:
   - Implement core calculation logic
   - Add validation rules
   - Create service layer
5. UI Development:
   - Create forms using Windows Forms/.NET MAUI
   - Implement data binding
   - Add validation controls
6. Testing:
   - Unit tests for business logic
   - Integration tests
   - User acceptance testing
7. Documentation:
   - API documentation
   - User manual
   - Technical documentation
8. Deployment Planning:
   - Database migration plan
   - Deployment scripts
   - Monitoring setup