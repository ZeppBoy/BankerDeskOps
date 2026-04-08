export interface LoanDto {
  id: string;
  accountId: string;
  amount: number;
  interestRate: number;
  termMonths: number;
  startDate: string;
  endDate: string;
  status: string;
}

export interface CreateLoanRequest {
  customerName: string;
  amount: number;
  interestRate: number;
  termMonths: number;
}

export interface RetailAccountDto {
  id: string;
  accountNumber: string;
  accountHolder: string;
  balance: number;
  createdDate: string;
  isActive: boolean;
}

export interface CreateRetailAccountRequest {
  accountHolder: string;
  initialBalance?: number;
}

export interface DepositRequest {
  amount: number;
}

export interface WithdrawRequest {
  amount: number;
}
