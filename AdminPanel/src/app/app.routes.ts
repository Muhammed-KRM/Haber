import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent),
    title: 'Giriş Yap | Kürsü TV Yönetim'
  },
  {
    path: 'admin',
    loadComponent: () => import('./shared/components/admin-layout.component').then(m => m.AdminLayoutComponent),
    canActivate: [authGuard],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
        title: 'Kontrol Paneli | Kürsü TV'
      },
      {
        path: 'news',
        loadComponent: () => import('./features/news/news-list.component').then(m => m.NewsListComponent),
        title: 'Haberler | Kürsü TV'
      },
      {
        path: 'news/new',
        loadComponent: () => import('./features/news/news-editor.component').then(m => m.NewsEditorComponent),
        title: 'Yeni Haber Yaz | Kürsü TV'
      },
      {
        path: 'news/edit/:id',
        loadComponent: () => import('./features/news/news-editor.component').then(m => m.NewsEditorComponent),
        title: 'Haberi Düzenle | Kürsü TV'
      },
      {
        path: 'categories',
        loadComponent: () => import('./features/categories/category-list.component').then(m => m.CategoryListComponent),
        title: 'Kategoriler | Kürsü TV'
      },
      {
        path: 'media',
        loadComponent: () => import('./features/media/media-gallery.component').then(m => m.MediaGalleryComponent),
        title: 'Medya Kütüphanesi | Kürsü TV'
      },
      {
        path: 'comments',
        loadComponent: () => import('./features/comments/comment-list.component').then(m => m.CommentListComponent),
        title: 'Yorum Moderasyonu | Kürsü TV'
      },
      {
        path: 'users',
        loadComponent: () => import('./features/users/user-list.component').then(m => m.UserListComponent),
        title: 'Yazarlar & Kullanıcılar | Kürsü TV'
      },
      {
        path: 'logs',
        loadComponent: () => import('./features/logs/logs.component').then(m => m.LogsComponent),
        title: 'Sistem Logları | Kürsü TV'
      },
      {
        path: 'settings',
        loadComponent: () => import('./features/settings/settings.component').then(m => m.SettingsComponent),
        title: 'Site Ayarları & SEO | Kürsü TV'
      }
    ]
  },
  {
    path: '',
    redirectTo: 'admin/dashboard',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: 'admin/dashboard'
  }
];
