import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DashboardService } from '../../core/services/dashboard.service';
import { DashboardStatsDto } from '../../core/models/dashboard.model';
import { NewsStatus, NewsType } from '../../core/models/news.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="space-y-6 animate-fade-in">
      <!-- Sayfa Başlığı & Hızlı Aksiyonlar -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="text-2xl font-extrabold text-slate-900 tracking-tight">Haber Masası Kontrol Paneli</h1>
          <p class="text-xs text-slate-500 mt-1">Anlık yayın akışı, okunma istatistikleri ve bekleyen onaylar.</p>
        </div>

        <div class="flex items-center gap-2.5">
          <button 
            (click)="loadStats()" 
            class="px-3 py-1.5 bg-white border border-slate-200 hover:bg-slate-50 text-slate-700 text-xs font-semibold rounded-lg shadow-xs transition-colors flex items-center gap-1.5"
          >
            <svg class="w-3.5 h-3.5 text-slate-500" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/></svg>
            Yenile
          </button>
          <a 
            routerLink="/admin/news/new" 
            class="px-3.5 py-1.5 bg-red-600 hover:bg-red-700 text-white text-xs font-semibold rounded-lg shadow-xs transition-colors flex items-center gap-1.5"
          >
            <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/></svg>
            Hızlı Haber Yaz
          </a>
        </div>
      </div>

      <!-- KPI KARTLARI (60-30-10 Kuralı: Temiz Beyaz Kartlar, Kırmızı/Zümrüt Vurgular) -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <!-- 1. Yayındaki Haberler -->
        <div class="premium-card p-5">
          <div class="flex items-center justify-between">
            <span class="text-xs font-semibold text-slate-500 uppercase tracking-wider">Yayında Olanlar</span>
            <div class="w-8 h-8 rounded-lg bg-emerald-50 text-emerald-600 flex items-center justify-center">
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
            </div>
          </div>
          <div class="mt-3 flex items-baseline gap-2">
            <span class="text-2xl font-black text-slate-900">{{ stats()?.totalPublishedNews || 0 }}</span>
            <span class="text-[11px] text-emerald-600 font-semibold bg-emerald-50 px-1.5 py-0.5 rounded">Aktif Haber</span>
          </div>
          <p class="text-[11px] text-slate-400 mt-2">Okuyuculara anlık açık içerik</p>
        </div>

        <!-- 2. Taslaklar & İnceleme Bekleyenler -->
        <div class="premium-card p-5">
          <div class="flex items-center justify-between">
            <span class="text-xs font-semibold text-slate-500 uppercase tracking-wider">Taslak / Hazırlık</span>
            <div class="w-8 h-8 rounded-lg bg-amber-50 text-amber-600 flex items-center justify-center">
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>
            </div>
          </div>
          <div class="mt-3 flex items-baseline gap-2">
            <span class="text-2xl font-black text-slate-900">{{ stats()?.totalDraftNews || 0 }}</span>
            <span class="text-[11px] text-amber-700 font-semibold bg-amber-50 px-1.5 py-0.5 rounded">Yazım Aşamasında</span>
          </div>
          <p class="text-[11px] text-slate-400 mt-2">Yayına alınmayı bekleyen haberler</p>
        </div>

        <!-- 3. Toplam Okunma / Trafik -->
        <div class="premium-card p-5">
          <div class="flex items-center justify-between">
            <span class="text-xs font-semibold text-slate-500 uppercase tracking-wider">Toplam Okunma</span>
            <div class="w-8 h-8 rounded-lg bg-red-50 text-red-600 flex items-center justify-center">
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/></svg>
            </div>
          </div>
          <div class="mt-3 flex items-baseline gap-2">
            <span class="text-2xl font-black text-slate-900">{{ (stats()?.totalViews || 0) | number }}</span>
            <span class="text-[11px] text-red-600 font-semibold bg-red-50 px-1.5 py-0.5 rounded">Görüntüleme</span>
          </div>
          <p class="text-[11px] text-slate-400 mt-2">Redis & DB senkron sayaç</p>
        </div>

        <!-- 4. Onay Bekleyen Yorumlar -->
        <div class="premium-card p-5">
          <div class="flex items-center justify-between">
            <span class="text-xs font-semibold text-slate-500 uppercase tracking-wider">Bekleyen Yorumlar</span>
            <div class="w-8 h-8 rounded-lg bg-indigo-50 text-indigo-600 flex items-center justify-center">
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z"/></svg>
            </div>
          </div>
          <div class="mt-3 flex items-baseline gap-2">
            <span class="text-2xl font-black text-slate-900">{{ stats()?.totalPendingComments || 0 }}</span>
            <span class="text-[11px] text-indigo-600 font-semibold bg-indigo-50 px-1.5 py-0.5 rounded">Moderasyonda</span>
          </div>
          <p class="text-[11px] text-slate-400 mt-2">
            <a routerLink="/admin/comments" class="text-indigo-600 hover:underline font-medium">İncelemek için tıkla &rarr;</a>
          </p>
        </div>
      </div>

      <!-- İKİ KOLONLU DÜZEN: Son Haberler & En Çok Okunanlar -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- SOL: Son Eklenen Haberler (2 Kolon) -->
        <div class="lg:col-span-2 premium-card p-5">
          <div class="flex items-center justify-between mb-4">
            <div>
              <h2 class="text-sm font-bold text-slate-900">Son Yayınlanan Haberler</h2>
              <p class="text-xs text-slate-400">Portalda aktif gösterilen en son içerikler</p>
            </div>
            <a routerLink="/admin/news" class="text-xs text-red-600 hover:text-red-700 font-semibold hover:underline">
              Tümünü Gör &rarr;
            </a>
          </div>

          <div class="overflow-x-auto">
            <table class="w-full text-left text-xs">
              <thead class="text-[11px] uppercase tracking-wider text-slate-400 bg-slate-50/80 border-y border-slate-100">
                <tr>
                  <th class="py-2.5 px-3">Haber</th>
                  <th class="py-2.5 px-3">Kategori</th>
                  <th class="py-2.5 px-3">Durum</th>
                  <th class="py-2.5 px-3 text-right">Okunma</th>
                  <th class="py-2.5 px-3 text-right">İşlem</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100">
                @for (item of stats()?.recentNews || []; track item.id) {
                  <tr class="hover:bg-slate-50/60 transition-colors">
                    <td class="py-3 px-3">
                      <div class="flex items-center gap-3">
                        <div class="w-10 h-10 rounded-lg bg-slate-100 overflow-hidden shrink-0 border border-slate-200 relative">
                          <div class="w-full h-full flex items-center justify-center text-slate-400 text-[10px] font-bold">KTV</div>
                          @if (item.coverImageUrl) {
                            <img [src]="item.coverImageUrl" class="absolute inset-0 w-full h-full object-cover" (error)="$event.target.remove()" />
                          }
                        </div>
                        <div class="min-w-0 max-w-xs">
                          <h4 class="font-semibold text-slate-800 truncate" [title]="item.title">{{ item.title }}</h4>
                          <span class="text-[10px] text-slate-400">{{ item.authorName }} &bull; {{ item.createdAt | date:'short' }}</span>
                        </div>
                      </div>
                    </td>
                    <td class="py-3 px-3">
                      @if (item.categories.length > 0) {
                        <span class="inline-flex items-center px-2 py-0.5 rounded text-[10px] font-medium bg-slate-100 text-slate-700">
                          {{ item.categories[0].name }}
                        </span>
                      }
                    </td>
                    <td class="py-3 px-3">
                      <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-semibold bg-emerald-50 text-emerald-700">
                        <span class="w-1.5 h-1.5 rounded-full bg-emerald-500"></span>
                        Yayında
                      </span>
                    </td>
                    <td class="py-3 px-3 text-right font-semibold text-slate-700">
                      {{ item.viewCount | number }}
                    </td>
                    <td class="py-3 px-3 text-right">
                      <a [routerLink]="['/admin/news/edit', item.id]" class="text-slate-400 hover:text-red-600 p-1 transition-colors">
                        Düzenle
                      </a>
                    </td>
                  </tr>
                } @empty {
                  <tr>
                    <td colspan="5" class="py-8 text-center text-slate-400 text-xs">Henüz yayınlanmış bir haber bulunmuyor.</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>

        <!-- SAĞ: En Çok Okunanlar & Trendler (1 Kolon) -->
        <div class="premium-card p-5">
          <div class="flex items-center justify-between mb-4">
            <div>
              <h2 class="text-sm font-bold text-slate-900">En Çok Okunanlar</h2>
              <p class="text-xs text-slate-400">Portalda en yüksek hit alanlar</p>
            </div>
            <span class="w-2 h-2 rounded-full bg-red-500 animate-ping"></span>
          </div>

          <div class="space-y-3">
            @for (top of stats()?.topViewedNews || []; track top.id; let idx = $index) {
              <div class="flex items-start gap-3 p-2.5 rounded-xl hover:bg-slate-50 transition-colors">
                <span class="w-6 h-6 rounded-lg flex items-center justify-center font-black text-xs shrink-0"
                  [ngClass]="{
                    'bg-red-600 text-white shadow-xs': idx === 0,
                    'bg-slate-800 text-white': idx === 1,
                    'bg-slate-200 text-slate-700': idx > 1
                  }"
                >
                  {{ idx + 1 }}
                </span>
                <div class="min-w-0 flex-1">
                  <h4 class="text-xs font-semibold text-slate-800 line-clamp-2 leading-snug">{{ top.title }}</h4>
                  <div class="flex items-center gap-2 mt-1 text-[10px] text-slate-400">
                    <span>{{ top.viewCount | number }} okunma</span>
                    <span>&bull;</span>
                    <span class="text-red-600 font-medium">{{ top.categories[0]?.name || 'Gündem' }}</span>
                  </div>
                </div>
              </div>
            } @empty {
              <p class="text-xs text-slate-400 text-center py-6">Okunma verisi henüz oluşmadı.</p>
            }
          </div>
        </div>
      </div>
    </div>
  `
})
export class DashboardComponent implements OnInit {
  stats = signal<DashboardStatsDto | null>(null);

  constructor(private dashboardService: DashboardService) {}

  ngOnInit() {
    this.loadStats();
  }

  loadStats() {
    this.dashboardService.getStats().subscribe({
      next: (res) => this.stats.set(res),
      error: (err) => console.error('Stats loading error:', err)
    });
  }
}
