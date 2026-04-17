import { Routes } from '@angular/router';
import { HomeComponent } from './features/home/home.component';
import { LoansComponent } from './features/loans/loans.component';
import { AccountsComponent } from './features/accounts/accounts.component';
import { ClientsComponent } from './features/clients/clients.component';
import { LoginComponent } from './features/login/login.component';
import { UsersComponent } from './features/users/users.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: '',        component: HomeComponent,     canActivate: [authGuard] },
  { path: 'loans',   component: LoansComponent,    canActivate: [authGuard] },
  { path: 'accounts',component: AccountsComponent, canActivate: [authGuard] },
  { path: 'clients', component: ClientsComponent,  canActivate: [authGuard] },
  { path: 'users',   component: UsersComponent,    canActivate: [authGuard] },
];
