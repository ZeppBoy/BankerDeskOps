import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../core/services/account.service';
import {
  RetailAccountDto,
  CreateRetailAccountRequest,
  DepositRequest,
  WithdrawRequest,
  AccountType,
} from '../../core/models';

@Component({
  selector: 'app-accounts',
  templateUrl: './accounts.component.html',
  styleUrls: ['./accounts.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule],
})
export class AccountsComponent implements OnInit {
  @ViewChild('accountModal') accountModal!: ElementRef;
  @ViewChild('transactionModal') transactionModal!: ElementRef;

  accounts$ = this.accountService.accounts$;
  loading$ = this.accountService.loading$;
  error$ = this.accountService.error$;

  AccountType = AccountType;

  editingId: string | null = null;
  selectedAccountId: string | null = null;
  transactionType: 'deposit' | 'withdraw' = 'deposit';

  showAccountModal = false;
  showTransactionModal = false;

  formData: CreateRetailAccountRequest = {
    customerName: '',
    accountType: AccountType.Checking,
    initialDeposit: 0,
  };

  transactionData: DepositRequest = {
    amount: 0,
  };

  constructor(private accountService: AccountService) {}

  ngOnInit(): void {
    this.accountService.loadAccounts();
  }

  openAccountModal(): void {
    this.editingId = null;
    this.resetForm();
    this.showAccountModal = true;
    document.body.style.overflow = 'hidden';
  }

  hideAccountModal(): void {
    this.showAccountModal = false;
    document.body.style.overflow = 'auto';
  }

  openTransactionModal(account: RetailAccountDto, type: 'deposit' | 'withdraw'): void {
    this.selectedAccountId = account.id;
    this.transactionType = type;
    this.transactionData.amount = 0;
    this.showTransactionModal = true;
    document.body.style.overflow = 'hidden';
  }

  hideTransactionModal(): void {
    this.showTransactionModal = false;
    document.body.style.overflow = 'auto';
  }

  editAccount(account: RetailAccountDto): void {
    this.editingId = account.id;
    this.formData = {
      customerName: account.customerName,
      accountType: account.accountType,
      initialDeposit: account.balance,
    };
    this.showAccountModal = true;
    document.body.style.overflow = 'hidden';
  }

  saveAccount(): void {
    if (!this.formData.customerName || (this.formData.initialDeposit != null && this.formData.initialDeposit < 0)) {
      alert('Please fill all required fields correctly');
      return;
    }

    if (this.editingId) {
      this.accountService.updateAccount(this.editingId, this.formData).subscribe({
        next: () => {
          this.hideAccountModal();
          this.resetForm();
        },
        error: (err: any) => console.error('Error saving account:', err),
      });
    } else {
      this.accountService.createAccount(this.formData).subscribe({
        next: () => {
          this.hideAccountModal();
          this.resetForm();
        },
        error: (err: any) => console.error('Error creating account:', err),
      });
    }
  }

  deleteAccount(id: string): void {
    if (confirm('Are you sure you want to close this account?')) {
      this.accountService.deleteAccount(id).subscribe({
        error: (err: any) => console.error('Error closing account:', err),
      });
    }
  }

  saveTransaction(): void {
    if (!this.selectedAccountId || this.transactionData.amount <= 0) {
      alert('Please enter a valid amount');
      return;
    }

    if (this.transactionType === 'deposit') {
      const request: DepositRequest = { amount: this.transactionData.amount };
      this.accountService.deposit(this.selectedAccountId, request).subscribe({
        next: () => {
          this.hideTransactionModal();
          this.transactionData.amount = 0;
          this.selectedAccountId = null;
        },
        error: (err: any) => console.error('Error depositing:', err),
      });
    } else {
      const request: WithdrawRequest = { amount: this.transactionData.amount };
      this.accountService.withdraw(this.selectedAccountId, request).subscribe({
        next: () => {
          this.hideTransactionModal();
          this.transactionData.amount = 0;
          this.selectedAccountId = null;
        },
        error: (err: any) => console.error('Error withdrawing:', err),
      });
    }
  }

  getAccountTypeLabel(accountType: AccountType): string {
    return AccountType[accountType] ?? 'Unknown';
  }

  resetForm(): void {
    this.formData = {
      customerName: '',
      accountType: AccountType.Checking,
      initialDeposit: 0,
    };
  }
}
