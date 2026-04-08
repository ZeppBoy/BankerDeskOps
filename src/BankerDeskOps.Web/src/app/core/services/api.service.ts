import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  LoanDto,
  CreateLoanRequest,
  RetailAccountDto,
  CreateRetailAccountRequest,
  DepositRequest,
  WithdrawRequest,
} from '../models';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private apiUrl = 'http://localhost:5048/api';

  constructor(private http: HttpClient) {}

  // Loan endpoints
  getLoans(): Observable<LoanDto[]> {
    return this.http.get<LoanDto[]>(`${this.apiUrl}/loans`);
  }

  getLoanById(id: string): Observable<LoanDto> {
    return this.http.get<LoanDto>(`${this.apiUrl}/loans/${id}`);
  }

  createLoan(request: CreateLoanRequest): Observable<LoanDto> {
    return this.http.post<LoanDto>(`${this.apiUrl}/loans`, request);
  }

  updateLoan(id: string, request: CreateLoanRequest): Observable<LoanDto> {
    return this.http.put<LoanDto>(`${this.apiUrl}/loans/${id}`, request);
  }

  deleteLoan(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/loans/${id}`);
  }

  // Retail Account endpoints
  getAccounts(): Observable<RetailAccountDto[]> {
    return this.http.get<RetailAccountDto[]>(`${this.apiUrl}/accounts`);
  }

  getAccountById(id: string): Observable<RetailAccountDto> {
    return this.http.get<RetailAccountDto>(`${this.apiUrl}/accounts/${id}`);
  }

  createAccount(
    request: CreateRetailAccountRequest
  ): Observable<RetailAccountDto> {
    return this.http.post<RetailAccountDto>(`${this.apiUrl}/accounts`, request);
  }

  updateAccount(
    id: string,
    request: CreateRetailAccountRequest
  ): Observable<RetailAccountDto> {
    return this.http.put<RetailAccountDto>(`${this.apiUrl}/accounts/${id}`, request);
  }

  deleteAccount(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/accounts/${id}`);
  }

  // Account transaction endpoints
  deposit(accountId: string, request: DepositRequest): Observable<RetailAccountDto> {
    return this.http.post<RetailAccountDto>(
      `${this.apiUrl}/accounts/${accountId}/deposit`,
      request
    );
  }

  withdraw(accountId: string, request: WithdrawRequest): Observable<RetailAccountDto> {
    return this.http.post<RetailAccountDto>(
      `${this.apiUrl}/accounts/${accountId}/withdraw`,
      request
    );
  }
}
