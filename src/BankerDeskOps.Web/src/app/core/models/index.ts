// LoanStatus enum: 0=Pending, 1=Approved, 2=Rejected, 3=Closed
export enum LoanStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Closed = 3,
}

// AccountType enum: 0=Checking, 1=Savings
export enum AccountType {
  Checking = 0,
  Savings = 1,
}

export interface LoanDto {
  id: string;
  customerName: string;
  amount: number;
  interestRate: number;
  termMonths: number;
  status: LoanStatus;
  createdAt: string;
  updatedAt: string;
}

export interface CreateLoanRequest {
  customerName: string;
  amount: number;
  interestRate: number;
  termMonths: number;
}

export interface RetailAccountDto {
  id: string;
  customerName: string;
  accountNumber: string;
  balance: number;
  accountType: AccountType;
  openedAt: string;
  updatedAt: string;
}

export interface CreateRetailAccountRequest {
  customerName: string;
  accountType: AccountType;
  initialDeposit?: number;
}

export interface DepositRequest {
  amount: number;
}

export interface WithdrawRequest {
  amount: number;
}
