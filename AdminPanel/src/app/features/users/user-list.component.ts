import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ToastService } from '../../core/services/toast.service';
import { AdminUserDto } from '../../core/models/auth.model';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-6 animate-fade-in max-w-5xl">
      <!-- Başlık -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="text-2xl font-extrabold text-slate-900 tracking-tight">Yazarlar & Kullanıcılar</h1>
          <p class="text-xs text-slate-500 mt-1">Köşe yazarları, editörler ve haber merkezi yetki dağılımı.</p>
        </div>
      </div>

      <!-- KULLANICI TABLOSU -->
      <div class="premium-card overflow-hidden">
        <div class="overflow-x-auto">
          <table class="w-full text-left text-xs">
            <thead class="text-[11px] uppercase tracking-wider text-slate-500 bg-slate-50/80 border-b border-slate-200">
              <tr>
                <th class="py-3 px-4">Kullanıcı / Yazar</th>
                <th class="py-3 px-4">E-Posta</th>
                <th class="py-3 px-4">Rol</th>
                <th class="py-3 px-4 text-center">Toplam Haber</th>
                <th class="py-3 px-4">Durum</th>
                <th class="py-3 px-4 text-right">Kayıt Tarihi</th>
                <th class="py-3 px-4 text-right">İşlemler</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              @for (user of users(); track user.id) {
                <tr class="hover:bg-slate-50/80 transition-colors">
                  <td class="py-3 px-4">
                    <div class="flex items-center gap-3">
                      <div class="w-8 h-8 rounded-full bg-slate-800 text-white font-bold flex items-center justify-center text-xs shrink-0">
                        {{ user.fullName.charAt(0) }}
                      </div>
                      <span class="font-bold text-slate-900">{{ user.fullName }}</span>
                    </div>
                  </td>
                  <td class="py-3 px-4 text-slate-600 font-mono text-[11px]">{{ user.email }}</td>
                  <td class="py-3 px-4">
                    <span class="inline-flex items-center px-2 py-0.5 rounded text-[10px] font-semibold"
                      [ngClass]="{
                        'bg-red-100 text-red-800': user.role === 0,
                        'bg-blue-100 text-blue-800': user.role === 1,
                        'bg-emerald-100 text-emerald-800': user.role === 2,
                        'bg-amber-100 text-amber-800': user.role === 3
                      }"
                    >
                      {{ getRoleName(user.role) }}
                    </span>
                  </td>
                  <td class="py-3 px-4 text-center font-bold text-slate-800">{{ user.newsCount }}</td>
                  <td class="py-3 px-4">
                    @if (user.isActive) {
                      <span class="inline-flex items-center gap-1 text-[11px] font-semibold text-emerald-600">
                        <span class="w-1.5 h-1.5 rounded-full bg-emerald-500"></span>
                        Aktif
                      </span>
                    } @else {
                      <span class="inline-flex items-center gap-1 text-[11px] font-semibold text-rose-600">
                        <span class="w-1.5 h-1.5 rounded-full bg-rose-500"></span>
                        Askıda
                      </span>
                    }
                  </td>
                  <td class="py-3 px-4 text-right text-slate-400 text-[10px]">{{ user.createdAt | date:'dd.MM.yyyy' }}</td>
                  <td class="py-3 px-4 text-right">
                    @if (user.isActive) {
                      <button (click)="suspendUser(user.id)" class="text-xs text-rose-600 hover:underline">Askıya Al</button>
                    } @else {
                      <button (click)="activateUser(user.id)" class="text-xs text-emerald-600 hover:underline">Aktif Et</button>
                    }
                  </td>
                </tr>
              } @empty {
                <tr><td colspan="7" class="py-8 text-center text-slate-400 text-xs">Kullanıcı bulunamadı.</td></tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    </div>
  `
})
export class UserListComponent implements OnInit {
  users = signal<AdminUserDto[]>([]);

  constructor(private http: HttpClient, private toastService: ToastService) {}

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.http.get<AdminUserDto[]>('http://localhost:5000/api/admin/users').subscribe({
      next: (res) => this.users.set(res),
      error: (err) => console.error(err)
    });
  }

  getRoleName(role: number): string {
    switch (role) {
      case 0: return 'Süper Yönetici';
      case 1: return 'Editör';
      case 2: return 'Muhabir';
      case 3: return 'Köşe Yazarı';
      default: return 'Kullanıcı';
    }
  }

  suspendUser(id: string) {
    if (!confirm('Kullanıcıyı askıya almak istiyor musunuz?')) return;
    this.http.post(`http://localhost:5000/api/admin/users/${id}/suspend`, { reason: 'Yönetici tarafından askıya alındı' }).subscribe({
      next: () => {
        this.toastService.success('Tamamlandı', 'Kullanıcı askıya alındı.');
        this.loadUsers();
      }
    });
  }

  activateUser(id: string) {
    this.http.post(`http://localhost:5000/api/admin/users/${id}/activate`, {}).subscribe({
      next: () => {
        this.toastService.success('Tamamlandı', 'Kullanıcı aktif edildi.');
        this.loadUsers();
      }
    });
  }
}
