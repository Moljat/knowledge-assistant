import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/dashboard/dashboard').then((component) => component.Dashboard)
  },
  {
    path: 'records',
    loadComponent: () =>
      import('./features/records/record-list').then((component) => component.RecordList)
  },
  {
    path: '**',
    redirectTo: ''
  }
];
