import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="min-h-screen bg-slate-950 flex items-center justify-center p-4 relative overflow-hidden font-sans">
      <!-- Arka Plan Kırmızı Işık Vurguları -->
      <div class="absolute -top-40 -left-40 w-96 h-96 bg-red-600/15 rounded-full blur-3xl pointer-events-none"></div>
      <div class="absolute -bottom-40 -right-40 w-96 h-96 bg-rose-600/10 rounded-full blur-3xl pointer-events-none"></div>

      <div class="w-full max-w-md bg-slate-900/90 backdrop-blur-xl border border-slate-800 rounded-2xl shadow-2xl p-8 relative z-10">
        <!-- Logo & Başlık -->
        <div class="text-center mb-8">
          <div class="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-gradient-to-tr from-red-700 to-rose-600 text-white font-black text-2xl shadow-lg shadow-red-900/40 mb-4">
            K
          </div>
          <h1 class="text-2xl font-extrabold text-white tracking-tight">KÜRSÜ<span class="text-red-500">TV</span></h1>
          <p class="text-xs text-slate-400 mt-1 font-medium">Haber Merkezi Yönetim Portalı</p>
        </div>

        <!-- Giriş Formu -->
        <form (ngSubmit)="onSubmit()" class="space-y-4">
          <div>
            <label class="block text-xs font-semibold text-slate-300 mb-1.5">E-Posta Adresi</label>
            <input 
              type="email" 
              [(ngModel)]="email" 
              name="email"
              required
              placeholder="admin@kursutv.com"
              class="w-full px-3.5 py-2.5 bg-slate-800/80 border border-slate-700 rounded-xl text-sm text-white placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-red-500/30 focus:border-red-500 transition-all"
            />
          </div>

          <div>
            <label class="block text-xs font-semibold text-slate-300 mb-1.5">Şifre</label>
            <input 
              type="password" 
              [(ngModel)]="password" 
              name="password"
              required
              placeholder="••••••••"
              class="w-full px-3.5 py-2.5 bg-slate-800/80 border border-slate-700 rounded-xl text-sm text-white placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-red-500/30 focus:border-red-500 transition-all"
            />
          </div>

          @if (errorMessage()) {
            <div class="p-3 bg-red-950/50 border border-red-800/60 rounded-xl text-xs text-red-300 flex items-center gap-2">
              <svg class="w-4 h-4 text-red-400 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
              <span>{{ errorMessage() }}</span>
            </div>
          }

          <button 
            type="submit" 
            [disabled]="isLoading()"
            class="w-full mt-2 py-3 bg-gradient-to-r from-red-600 to-rose-600 hover:from-red-700 hover:to-rose-700 text-white font-semibold text-sm rounded-xl shadow-lg shadow-red-900/30 transition-all transform active:scale-[0.98] disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
          >
            @if (isLoading()) {
              <svg class="animate-spin w-4 h-4 text-white" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"></path></svg>
              <span>Giriş Yapılıyor...</span>
            } @else {
              <span>Yönetim Paneline Giriş Yap</span>
            }
          </button>
        </form>

        <div class="mt-6 text-center">
          <p class="text-[11px] text-slate-500">Kürsü TV Haber Sistemi &copy; 2026</p>
        </div>
      </div>
    </div>
  `
})
export class LoginComponent {
  email = 'admin@kursutv.com';
  password = 'Admin@123!';
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  constructor(
    private authService: AuthService,
    private router: Router,
    private toastService: ToastService
  ) {}

  onSubmit() {
    if (!this.email || !this.password) return;

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService.login({ email: this.email, password: this.password }).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.success) {
          this.toastService.success('Giriş Başarılı', 'Hoş geldiniz!');
          this.router.navigate(['/admin/dashboard']);
        } else {
          this.errorMessage.set(res.errorMessage || 'Giriş yapılamadı.');
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.message || 'Sunucuya bağlanılamadı.');
      }
    });
  }
}
