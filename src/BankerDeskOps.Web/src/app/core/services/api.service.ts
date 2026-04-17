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
  BankClientDto,
  CreateBankClientRequest,
  UpdateBankClientRequest,
  UserDto,
  LoginRequest,
  LoginResponse,
  CreateUserRequest,
  UpdateUserRequest,
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

  approveLoan(id: string): Observable<LoanDto> {
    return this.http.put<LoanDto>(`${this.apiUrl}/loans/${id}/approve`, {});
  }

  rejectLoan(id: string): Observable<LoanDto> {
    return this.http.put<LoanDto>(`${this.apiUrl}/loans/${id}/reject`, {});
  }

  // Retail Account endpoints
  getAccounts(): Observable<RetailAccountDto[]> {
    return this.http.get<RetailAccountDto[]>(`${this.apiUrl}/retailaccounts`);
  }

  getAccountById(id: string): Observable<RetailAccountDto> {
    return this.http.get<RetailAccountDto>(`${this.apiUrl}/retailaccounts/${id}`);
  }

  createAccount(request: CreateRetailAccountRequest): Observable<RetailAccountDto> {
    return this.http.post<RetailAccountDto>(`${this.apiUrl}/retailaccounts`, request);
  }

  updateAccount(id: string, request: CreateRetailAccountRequest): Observable<RetailAccountDto> {
    return this.http.put<RetailAccountDto>(`${this.apiUrl}/retailaccounts/${id}`, request);
  }

  deleteAccount(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/retailaccounts/${id}`);
  }

  deposit(accountId: string, request: DepositRequest): Observable<RetailAccountDto> {
    return this.http.post<RetailAccountDto>(
      `${this.apiUrl}/retailaccounts/${accountId}/deposit`, request);
  }

  withdraw(accountId: string, request: WithdrawRequest): Observable<RetailAccountDto> {
    return this.http.post<RetailAccountDto>(
      `${this.apiUrl}/retailaccounts/${accountId}/withdraw`, request);
  }

  // Bank Client endpoints
  getClients(): Observable<BankClientDto[]> {
    return this.http.get<BankClientDto[]>(`${this.apiUrl}/bankclients`);
  }

  getClientById(id: string): Observable<BankClientDto> {
    return this.http.get<BankClientDto>(`${this.apiUrl}/bankclients/${id}`);
  }

  createClient(request: CreateBankClientRequest): Observable<BankClientDto> {
    return this.http.post<BankClientDto>(`${this.apiUrl}/bankclients`, request);
  }

  updateClient(id: string, request: UpdateBankClientRequest): Observable<BankClientDto> {
    return this.http.put<BankClientDto>(`${this.apiUrl}/bankclients/${id}`, request);
  }

  suspendClient(id: string): Observable<BankClientDto> {
    return this.http.put<BankClientDto>(`${this.apiUrl}/bankclients/${id}/suspend`, {});
  }

  activateClient(id: string): Observable<BankClientDto> {
    return this.http.put<BankClientDto>(`${this.apiUrl}/bankclients/${id}/activate`, {});
  }

  deleteClient(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/bankclients/${id}`);
  }

  // User endpoints
  getUsers(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(`${this.apiUrl}/users`);
  }

  getUserById(id: string): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.apiUrl}/users/${id}`);
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/users/login`, request);
  }

  createUser(request: CreateUserRequest): Observable<UserDto> {
    return this.http.post<UserDto>(`${this.apiUrl}/users`, request);
  }

  updateUser(id: string, request: UpdateUserRequest): Observable<UserDto> {
    return this.http.put<UserDto>(`${this.apiUrl}/users/${id}`, request);
  }

  activateUser(id: string): Observable<UserDto> {
    return this.http.put<UserDto>(`${this.apiUrl}/users/${id}/activate`, {});
  }

  deactivateUser(id: string): Observable<UserDto> {
    return this.http.put<UserDto>(`${this.apiUrl}/users/${id}/deactivate`, {});
  }

  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/users/${id}`);
  }
}

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

  approveLoan(id: string): Observable<LoanDto> {
    return this.http.put<LoanDto>(`${this.apiUrl}/loans/${id}/approve`, {});
  }

  rejectLoan(id: string): Observable<LoanDto> {
    return this.http.put<LoanDto>(`${this.apiUrl}/loans/${id}/reject`, {});
  }

  // Retail Account endpoints
  getAccounts(): Observable<RetailAccountDto[]> {
    return this.http.get<RetailAccountDto[]>(`${this.apiUrl}/retailaccounts`);
  }

  getAccountById(id: string): Observable<RetailAccountDto> {
    return this.http.get<RetailAccountDto>(`${this.apiUrl}/retailaccounts/${id}`);
  }

  createAccount(
    request: CreateRetailAccountRequest
  ): Observable<RetailAccountDto> {
    return this.http.post<RetailAccountDto>(`${this.apiUrl}/retailaccounts`, request);
  }

  updateAccount(
    id: string,
    request: CreateRetailAccountRequest
  ): Observable<RetailAccountDto> {
    return this.http.put<RetailAccountDto>(`${this.apiUrl}/retailaccounts/${id}`, request);
  }

  deleteAccount(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/retailaccounts/${id}`);
  }

  // Account transaction endpoints
  deposit(accountId: string, request: DepositRequest): Observable<RetailAccountDto> {
    return this.http.post<RetailAccountDto>(
      `${this.apiUrl}/retailaccounts/${accountId}/deposit`,
      request
    );
  }

  withdraw(accountId: string, request: WithdrawRequest): Observable<RetailAccountDto> {
    return this.http.post<RetailAccountDto>(
      `${this.apiUrl}/retailaccounts/${accountId}/withdraw`,
      request
    );
  }

  // Bank Client endpoints
  getClients(): Observable<BankClientDto[]> {
    return this.http.get<BankClientDto[]>(`${this.apiUrl}/bankclients`);
  }

  getClientById(id: string): Observable<BankClientDto> {
    return this.http.get<BankClientDto>(`${this.apiUrl}/bankclients/${id}`);
  }

  createClient(request: CreateBankClientRequest): Observable<BankClientDto> {
    return this.http.post<BankClientDto>(`${this.apiUrl}/bankclients`, request);
  }

  updateClient(id: string, request: UpdateBankClientRequest): Observable<BankClientDto> {
    return this.http.put<BankClientDto>(`${this.apiUrl}/bankclients/${id}`, request);
  }

  suspendClient(id: string): Observable<BankClientDto> {
    return this.http.put<BankClientDto>(`${this.apiUrl}/bankclients/${id}/suspend`, {});
  }

  activateClient(id: string): Observable<BankClientDto> {
    return this.http.put<BankClientDto>(`${this.apiUrl}/bankclients/${id}/activate`, {});
  }

  deleteClient(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/bankclients/${id}`);
  }
}
