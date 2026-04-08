import { Routes } from '@angular/router';
import { HomeComponent } from './features/home/home.component';
import { LoansComponent } from './features/loans/loans.component';
import { AccountsComponent } from './features/accounts/accounts.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'loans', component: LoansComponent },
  { path: 'accounts', component: AccountsComponent },
];
