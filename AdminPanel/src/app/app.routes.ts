import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/client-menu/client-menu.component').then(m => m.ClientMenuComponent),
    title: 'Lezzet Durağı | QR Menü'
  },
  {
    path: 'admin',
    loadComponent: () => import('./features/admin/admin-dashboard.component').then(m => m.AdminDashboardComponent),
    title: 'Restoran Yönetim Paneli'
  },
  {
    path: '**',
    redirectTo: ''
  }
];
