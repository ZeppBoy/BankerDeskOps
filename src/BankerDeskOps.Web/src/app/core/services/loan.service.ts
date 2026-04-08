import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { LoanDto, CreateLoanRequest } from '../models';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root',
})
export class LoanService {
  private loansSubject = new BehaviorSubject<LoanDto[]>([]);
  public loans$ = this.loansSubject.asObservable();

  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();

  private errorSubject = new BehaviorSubject<string | null>(null);
  public error$ = this.errorSubject.asObservable();

  constructor(private apiService: ApiService) {}

  loadLoans(): void {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    this.apiService.getLoans().subscribe({
      next: (loans) => {
        this.loansSubject.next(loans);
        this.loadingSubject.next(false);
      },
      error: (error) => {
        this.errorSubject.next('Failed to load loans');
        this.loadingSubject.next(false);
        console.error('Error loading loans:', error);
      },
    });
  }

  createLoan(request: CreateLoanRequest): Observable<LoanDto> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.apiService.createLoan(request).pipe(
      tap((newLoan) => {
        const currentLoans = this.loansSubject.value;
        this.loansSubject.next([...currentLoans, newLoan]);
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.errorSubject.next('Failed to create loan');
        this.loadingSubject.next(false);
        console.error('Error creating loan:', error);
        throw error;
      })
    );
  }

  updateLoan(id: string, request: CreateLoanRequest): Observable<LoanDto> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.apiService.updateLoan(id, request).pipe(
      tap((updatedLoan) => {
        const currentLoans = this.loansSubject.value;
        const index = currentLoans.findIndex((l) => l.id === id);
        if (index !== -1) {
          currentLoans[index] = updatedLoan;
          this.loansSubject.next([...currentLoans]);
        }
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.errorSubject.next('Failed to update loan');
        this.loadingSubject.next(false);
        console.error('Error updating loan:', error);
        throw error;
      })
    );
  }

  deleteLoan(id: string): Observable<void> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.apiService.deleteLoan(id).pipe(
      tap(() => {
        const currentLoans = this.loansSubject.value;
        this.loansSubject.next(currentLoans.filter((l) => l.id !== id));
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.errorSubject.next('Failed to delete loan');
        this.loadingSubject.next(false);
        console.error('Error deleting loan:', error);
        throw error;
      })
    );
  }

  approveLoan(id: string): Observable<LoanDto> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.apiService.approveLoan(id).pipe(
      tap((updatedLoan) => {
        const currentLoans = this.loansSubject.value;
        const index = currentLoans.findIndex((l) => l.id === id);
        if (index !== -1) {
          currentLoans[index] = updatedLoan;
          this.loansSubject.next([...currentLoans]);
        }
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.errorSubject.next('Failed to approve loan');
        this.loadingSubject.next(false);
        console.error('Error approving loan:', error);
        throw error;
      })
    );
  }

  rejectLoan(id: string): Observable<LoanDto> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.apiService.rejectLoan(id).pipe(
      tap((updatedLoan) => {
        const currentLoans = this.loansSubject.value;
        const index = currentLoans.findIndex((l) => l.id === id);
        if (index !== -1) {
          currentLoans[index] = updatedLoan;
          this.loansSubject.next([...currentLoans]);
        }
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.errorSubject.next('Failed to reject loan');
        this.loadingSubject.next(false);
        console.error('Error rejecting loan:', error);
        throw error;
      })
    );
  }

  getLoanById(id: string): Observable<LoanDto> {
    return this.apiService.getLoanById(id);
  }
}
