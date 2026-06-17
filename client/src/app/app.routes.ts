import { Routes } from '@angular/router';
import { LoginComponent } from './features/login/login.component';
import { RequestDetailComponent } from './features/request-detail/request-detail.component';
import { RequestFormComponent } from './features/request-form/request-form.component';
import { RequestListComponent } from './features/request-list/request-list.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'requests/new', component: RequestFormComponent },
  { path: 'requests/:id', component: RequestDetailComponent },
  { path: '', component: RequestListComponent },
  { path: '**', redirectTo: '' }
];
