import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import {
  RetailAccountDto,
  CreateRetailAccountRequest,
  DepositRequest,
  WithdrawRequest,
} from '../models';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private accountsSubject = new BehaviorSubject<RetailAccountDto[]>([]);
  public accounts$ = this.accountsSubject.asObservable();

  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();

  private errorSubject = new BehaviorSubject<string | null>(null);
  public error$ = this.errorSubject.asObservable();

  constructor(private apiService: ApiService) {}

  loadAccounts(): void {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    this.apiService.getAccounts().subscribe({
      next: (accounts) => {
        this.accountsSubject.next(accounts);
        this.loadingSubject.next(false);
      },
      error: (error) => {
        this.errorSubject.next('Failed to load accounts');
        this.loadingSubject.next(false);
        console.error('Error loading accounts:', error);
      },
    });
  }

  createAccount(request: CreateRetailAccountRequest): Observable<RetailAccountDto> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.apiService.createAccount(request).pipe(
      tap((newAccount) => {
        const currentAccounts = this.accountsSubject.value;
        this.accountsSubject.next([...currentAccounts, newAccount]);
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.errorSubject.next('Failed to create account');
        this.loadingSubject.next(false);
        console.error('Error creating account:', error);
        throw error;
      })
    );
  }

  updateAccount(
    id: string,
    request: CreateRetailAccountRequest
  ): Observable<RetailAccountDto> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.apiService.updateAccount(id, request).pipe(
      tap((updatedAccount) => {
        const currentAccounts = this.accountsSubject.value;
        const index = currentAccounts.findIndex((a) => a.id === id);
        if (index !== -1) {
          currentAccounts[index] = updatedAccount;
          this.accountsSubject.next([...currentAccounts]);
        }
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.errorSubject.next('Failed to update account');
        this.loadingSubject.next(false);
        console.error('Error updating account:', error);
        throw error;
      })
    );
  }

  deleteAccount(id: string): Observable<void> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.apiService.deleteAccount(id).pipe(
      tap(() => {
        const currentAccounts = this.accountsSubject.value;
        this.accountsSubject.next(currentAccounts.filter((a) => a.id !== id));
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.errorSubject.next('Failed to delete account');
        this.loadingSubject.next(false);
        console.error('Error deleting account:', error);
        throw error;
      })
    );
  }

  getAccountById(id: string): Observable<RetailAccountDto> {
    return this.apiService.getAccountById(id);
  }

  deposit(id: string, request: DepositRequest): Observable<RetailAccountDto> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.apiService.deposit(id, request).pipe(
      tap((updatedAccount) => {
        const currentAccounts = this.accountsSubject.value;
        const index = currentAccounts.findIndex((a) => a.id === id);
        if (index !== -1) {
          currentAccounts[index] = updatedAccount;
          this.accountsSubject.next([...currentAccounts]);
        }
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.errorSubject.next('Failed to deposit');
        this.loadingSubject.next(false);
        console.error('Error depositing:', error);
        throw error;
      })
    );
  }

  withdraw(id: string, request: WithdrawRequest): Observable<RetailAccountDto> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.apiService.withdraw(id, request).pipe(
      tap((updatedAccount) => {
        const currentAccounts = this.accountsSubject.value;
        const index = currentAccounts.findIndex((a) => a.id === id);
        if (index !== -1) {
          currentAccounts[index] = updatedAccount;
          this.accountsSubject.next([...currentAccounts]);
        }
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.errorSubject.next('Failed to withdraw');
        this.loadingSubject.next(false);
        console.error('Error withdrawing:', error);
        throw error;
      })
    );
  }
}
