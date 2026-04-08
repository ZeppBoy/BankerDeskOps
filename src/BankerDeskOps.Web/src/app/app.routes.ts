import { Routes } from '@angular/router';
import { LoansComponent } from './features/loans/loans.component';
import { AccountsComponent } from './features/accounts/accounts.component';

export const routes: Routes = [
  { path: '', redirectTo: '/loans', pathMatch: 'full' },
  { path: 'loans', component: LoansComponent },
  { path: 'accounts', component: AccountsComponent },
];
