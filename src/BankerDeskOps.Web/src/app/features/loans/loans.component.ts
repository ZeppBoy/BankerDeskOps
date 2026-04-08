import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LoanService } from '../../core/services/loan.service';
import { LoanDto, CreateLoanRequest, LoanStatus } from '../../core/models';

@Component({
  selector: 'app-loans',
  templateUrl: './loans.component.html',
  styleUrls: ['./loans.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule],
})
export class LoansComponent implements OnInit {
  @ViewChild('loanModal') loanModal!: ElementRef;

  loans$ = this.loanService.loans$;
  loading$ = this.loanService.loading$;
  error$ = this.loanService.error$;

  LoanStatus = LoanStatus;

  editingId: string | null = null;
  showModal = false;
  formData: CreateLoanRequest = {
    customerName: '',
    amount: 0,
    termMonths: 0,
    interestRate: 0,
  };

  constructor(private loanService: LoanService) {}

  ngOnInit(): void {
    this.loanService.loadLoans();
  }

  openModal(): void {
    this.editingId = null;
    this.resetForm();
    this.showModal = true;
    document.body.style.overflow = 'hidden';
  }

  hideModal(): void {
    this.showModal = false;
    document.body.style.overflow = 'auto';
  }

  editLoan(loan: LoanDto): void {
    this.editingId = loan.id;
    this.formData = {
      customerName: loan.customerName,
      amount: loan.amount,
      termMonths: loan.termMonths,
      interestRate: loan.interestRate,
    };
    this.showModal = true;
    document.body.style.overflow = 'hidden';
  }

  saveLoan(): void {
    if (!this.formData.customerName || this.formData.amount <= 0) {
      alert('Please fill all required fields correctly');
      return;
    }

    if (this.editingId) {
      this.loanService.updateLoan(this.editingId, this.formData).subscribe({
        next: () => {
          this.hideModal();
          this.resetForm();
        },
        error: (err: any) => console.error('Error saving loan:', err),
      });
    } else {
      this.loanService.createLoan(this.formData).subscribe({
        next: () => {
          this.hideModal();
          this.resetForm();
        },
        error: (err: any) => console.error('Error creating loan:', err),
      });
    }
  }

  approveLoan(id: string): void {
    if (confirm('Approve this loan?')) {
      this.loanService.approveLoan(id).subscribe({
        error: (err: any) => console.error('Error approving loan:', err),
      });
    }
  }

  rejectLoan(id: string): void {
    if (confirm('Reject this loan?')) {
      this.loanService.rejectLoan(id).subscribe({
        error: (err: any) => console.error('Error rejecting loan:', err),
      });
    }
  }

  deleteLoan(id: string): void {
    if (confirm('Are you sure you want to delete this loan?')) {
      this.loanService.deleteLoan(id).subscribe({
        error: (err: any) => console.error('Error deleting loan:', err),
      });
    }
  }

  getLoanStatusLabel(status: LoanStatus): string {
    return LoanStatus[status] ?? 'Unknown';
  }

  getLoanStatusClass(status: LoanStatus): string {
    switch (status) {
      case LoanStatus.Pending: return 'bg-warning text-dark';
      case LoanStatus.Approved: return 'bg-success';
      case LoanStatus.Rejected: return 'bg-danger';
      case LoanStatus.Closed: return 'bg-secondary';
      default: return 'bg-secondary';
    }
  }

  resetForm(): void {
    this.formData = {
      customerName: '',
      amount: 0,
      termMonths: 0,
      interestRate: 0,
    };
  }
}
