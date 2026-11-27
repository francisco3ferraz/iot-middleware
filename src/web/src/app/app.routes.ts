import { Routes } from '@angular/router';
import { HealthPage } from './health/health-page';
import { LoginPage } from './auth/login-page';

export const routes: Routes = [
  { path: '', component: HealthPage },
  { path: 'login', component: LoginPage },
];
