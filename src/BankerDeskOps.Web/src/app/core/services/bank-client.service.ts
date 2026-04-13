import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { BankClientDto, CreateBankClientRequest, UpdateBankClientRequest } from '../models';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root',
})
export class BankClientService {
  private clientsSubject = new BehaviorSubject<BankClientDto[]>([]);
  public clients$ = this.clientsSubject.asObservable();

  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();

  private errorSubject = new BehaviorSubject<string | null>(null);
  public error$ = this.errorSubject.asObservable();

  constructor(private apiService: ApiService) {}

  loadClients(): void {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    this.apiService.getClients().subscribe({
      next: (clients) => {
        this.clientsSubject.next(clients);
        this.loadingSubject.next(false);
      },
      error: (error) => {
        this.errorSubject.next('Failed to load clients');
        this.loadingSubject.next(false);
        console.error('Error loading bank clients:', error);
      },
    });
  }

  createClient(request: CreateBankClientRequest): Observable<BankClientDto> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.apiService.createClient(request).pipe(
      tap((newClient) => {
        this.clientsSubject.next([...this.clientsSubject.value, newClient]);
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.errorSubject.next('Failed to create client');
        this.loadingSubject.next(false);
        console.error('Error creating bank client:', error);
        throw error;
      })
    );
  }

  updateClient(id: string, request: UpdateBankClientRequest): Observable<BankClientDto> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.apiService.updateClient(id, request).pipe(
      tap((updated) => {
        const current = this.clientsSubject.value;
        const index = current.findIndex((c) => c.id === id);
        if (index !== -1) {
          current[index] = updated;
          this.clientsSubject.next([...current]);
        }
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.errorSubject.next('Failed to update client');
        this.loadingSubject.next(false);
        console.error('Error updating bank client:', error);
        throw error;
      })
    );
  }

  suspendClient(id: string): Observable<BankClientDto> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.apiService.suspendClient(id).pipe(
      tap((updated) => {
        const current = this.clientsSubject.value;
        const index = current.findIndex((c) => c.id === id);
        if (index !== -1) {
          current[index] = updated;
          this.clientsSubject.next([...current]);
        }
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.errorSubject.next('Failed to suspend client');
        this.loadingSubject.next(false);
        console.error('Error suspending bank client:', error);
        throw error;
      })
    );
  }

  activateClient(id: string): Observable<BankClientDto> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.apiService.activateClient(id).pipe(
      tap((updated) => {
        const current = this.clientsSubject.value;
        const index = current.findIndex((c) => c.id === id);
        if (index !== -1) {
          current[index] = updated;
          this.clientsSubject.next([...current]);
        }
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.errorSubject.next('Failed to activate client');
        this.loadingSubject.next(false);
        console.error('Error activating bank client:', error);
        throw error;
      })
    );
  }

  deleteClient(id: string): Observable<void> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.apiService.deleteClient(id).pipe(
      tap(() => {
        this.clientsSubject.next(this.clientsSubject.value.filter((c) => c.id !== id));
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.errorSubject.next('Failed to delete client');
        this.loadingSubject.next(false);
        console.error('Error deleting bank client:', error);
        throw error;
      })
    );
  }
}
