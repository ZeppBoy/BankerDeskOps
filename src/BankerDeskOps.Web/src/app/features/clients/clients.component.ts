import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BankClientService } from '../../core/services/bank-client.service';
import {
  BankClientDto,
  ClientStatus,
  CreateBankClientRequest,
  UpdateBankClientRequest,
} from '../../core/models';

@Component({
  selector: 'app-clients',
  templateUrl: './clients.component.html',
  styleUrls: ['./clients.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule],
})
export class ClientsComponent implements OnInit {
  clients$ = this.clientService.clients$;
  loading$ = this.clientService.loading$;
  error$ = this.clientService.error$;

  ClientStatus = ClientStatus;

  editingId: string | null = null;
  showModal = false;

  formData: CreateBankClientRequest = this.emptyForm();

  constructor(private clientService: BankClientService) {}

  ngOnInit(): void {
    this.clientService.loadClients();
  }

  openModal(): void {
    this.editingId = null;
    this.formData = this.emptyForm();
    this.showModal = true;
    document.body.style.overflow = 'hidden';
  }

  editClient(client: BankClientDto): void {
    this.editingId = client.id;
    this.formData = {
      firstName: client.firstName,
      lastName: client.lastName,
      email: client.email,
      phoneNumber: client.phoneNumber,
      dateOfBirth: client.dateOfBirth.substring(0, 10),
      nationalId: client.nationalId,
      street: client.street,
      city: client.city,
      postalCode: client.postalCode,
      country: client.country,
    };
    this.showModal = true;
    document.body.style.overflow = 'hidden';
  }

  hideModal(): void {
    this.showModal = false;
    document.body.style.overflow = 'auto';
  }

  saveClient(): void {
    if (!this.formData.firstName || !this.formData.lastName || !this.formData.email) {
      alert('First name, last name and email are required');
      return;
    }

    if (this.editingId) {
      const updateReq: UpdateBankClientRequest = { ...this.formData };
      this.clientService.updateClient(this.editingId, updateReq).subscribe({
        next: () => { this.hideModal(); },
        error: (err: any) => console.error('Error updating client:', err),
      });
    } else {
      this.clientService.createClient(this.formData).subscribe({
        next: () => { this.hideModal(); },
        error: (err: any) => console.error('Error creating client:', err),
      });
    }
  }

  suspendClient(id: string): void {
    if (confirm('Suspend this client?')) {
      this.clientService.suspendClient(id).subscribe({
        error: (err: any) => console.error('Error suspending client:', err),
      });
    }
  }

  activateClient(id: string): void {
    if (confirm('Activate this client?')) {
      this.clientService.activateClient(id).subscribe({
        error: (err: any) => console.error('Error activating client:', err),
      });
    }
  }

  deleteClient(id: string): void {
    if (confirm('Are you sure you want to delete this client?')) {
      this.clientService.deleteClient(id).subscribe({
        error: (err: any) => console.error('Error deleting client:', err),
      });
    }
  }

  getStatusLabel(status: ClientStatus): string {
    return ClientStatus[status] ?? 'Unknown';
  }

  getStatusClass(status: ClientStatus): string {
    switch (status) {
      case ClientStatus.Active:    return 'bg-success';
      case ClientStatus.Inactive:  return 'bg-secondary';
      case ClientStatus.Suspended: return 'bg-danger';
      default:                     return 'bg-secondary';
    }
  }

  private emptyForm(): CreateBankClientRequest {
    return {
      firstName: '',
      lastName: '',
      email: '',
      phoneNumber: '',
      dateOfBirth: '',
      nationalId: '',
      street: '',
      city: '',
      postalCode: '',
      country: '',
    };
  }
}
