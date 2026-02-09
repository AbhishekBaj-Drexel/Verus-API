import { Routes } from '@angular/router';
import { CompaniesPageComponent } from './features/companies/companies-page.component';

export const routes: Routes = [
  { path: '', component: CompaniesPageComponent },
  { path: '**', redirectTo: '' },
];
