import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { NewsService } from '../../core/services/news.service';
import { CategoryService } from '../../core/services/category.service';
import { ToastService } from '../../core/services/toast.service';
import { NewsListDto, NewsStatus, NewsType, PagedResult } from '../../core/models/news.model';
import { CategoryDto } from '../../core/models/category.model';

@Component({
  selector: 'app-news-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="space-y-6 animate-fade-in">
      <!-- Başlık & Aksiyon -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="text-2xl font-extrabold text-slate-900 tracking-tight">Haber Yönetimi</h1>
          <p class="text-xs text-slate-500 mt-1">Yayınlanmış, taslak ve arşivlenmiş tüm haberlerin kontrolü.</p>
        </div>

        <a 
          routerLink="/admin/news/new"
          class="inline-flex items-center gap-2 px-4 py-2 bg-gradient-to-r from-red-600 to-rose-600 hover:from-red-700 hover:to-rose-700 text-white text-xs font-semibold rounded-lg shadow-sm shadow-red-500/20 hover:shadow-md transition-all self-start sm:self-auto"
        >
          <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/></svg>
          Yeni Haber Oluştur
        </a>
      </div>

      <!-- FİLTRELEME & ARAMA ÇUBUĞU -->
      <div class="premium-card p-4 flex flex-col sm:flex-row items-center gap-3">
        <!-- Metin Arama -->
        <div class="relative flex-1 w-full">
          <span class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-slate-400">
            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
          </span>
          <input 
            type="text" 
            [(ngModel)]="searchQuery" 
            (ngModelChange)="onSearchChange()"
            placeholder="Başlık veya içerikte ara..." 
            class="w-full pl-9 pr-4 py-2 text-xs bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 transition-all placeholder:text-slate-400"
          />
        </div>

        <!-- Kategori Filtresi -->
        <select 
          [(ngModel)]="selectedCategory" 
          (change)="loadNews()"
          class="w-full sm:w-44 px-3 py-2 text-xs bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 transition-all text-slate-700"
        >
          <option [ngValue]="null">Tüm Kategoriler</option>
          @for (cat of categories(); track cat.id) {
            <option [ngValue]="cat.id">{{ cat.name }}</option>
          }
        </select>

        <!-- Durum Filtresi -->
        <select 
          [(ngModel)]="selectedStatus" 
          (change)="loadNews()"
          class="w-full sm:w-40 px-3 py-2 text-xs bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 transition-all text-slate-700"
        >
          <option [ngValue]="null">Tüm Durumlar</option>
          <option [ngValue]="2">Yayında</option>
          <option [ngValue]="0">Taslak</option>
          <option [ngValue]="1">İnceleme Bekliyor</option>
          <option [ngValue]="3">Arşivlendi</option>
        </select>
      </div>

      <!-- HABERLER TABLOSU -->
      <div class="premium-card overflow-hidden">
        <div class="overflow-x-auto">
          <table class="w-full text-left text-xs">
            <thead class="text-[11px] uppercase tracking-wider text-slate-500 bg-slate-50/80 border-b border-slate-200">
              <tr>
                <th class="py-3 px-4">Görsel</th>
                <th class="py-3 px-4">Haber Başlığı</th>
                <th class="py-3 px-4">Kategori</th>
                <th class="py-3 px-4">Yazar</th>
                <th class="py-3 px-4">Durum</th>
                <th class="py-3 px-4 text-center">Manşet / Son Dakika</th>
                <th class="py-3 px-4 text-right">Okunma</th>
                <th class="py-3 px-4 text-right">Tarih</th>
                <th class="py-3 px-4 text-right">İşlemler</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              @for (item of pagedResult()?.items || []; track item.id) {
                <tr class="hover:bg-slate-50/80 transition-colors">
                  <!-- Görsel -->
                  <td class="py-3 px-4">
                    <div class="w-12 h-9 rounded-lg bg-slate-100 overflow-hidden border border-slate-200 shrink-0">
                      @if (item.coverImageUrl) {
                        <img [src]="item.coverImageUrl" class="w-full h-full object-cover" />
                      } @else {
                        <div class="w-full h-full flex items-center justify-center text-slate-400 text-[9px] font-bold">KTV</div>
                      }
                    </div>
                  </td>

                  <!-- Başlık & Slug -->
                  <td class="py-3 px-4 max-w-xs">
                    <div class="font-semibold text-slate-900 line-clamp-1" [title]="item.title">
                      {{ item.title }}
                    </div>
                    <div class="text-[10px] text-slate-400 font-mono truncate">
                      /haber/{{ item.slug }}
                    </div>
                  </td>

                  <!-- Kategori -->
                  <td class="py-3 px-4">
                    @if (item.categories && item.categories.length > 0) {
                      <span class="inline-flex items-center px-2 py-0.5 rounded text-[10px] font-medium bg-slate-100 text-slate-700">
                        {{ item.categories[0].name }}
                      </span>
                    } @else {
                      <span class="text-slate-400 text-[10px]">-</span>
                    }
                  </td>

                  <!-- Yazar -->
                  <td class="py-3 px-4 text-slate-600 font-medium whitespace-nowrap">
                    {{ item.authorName }}
                  </td>

                  <!-- Durum Rozeti -->
                  <td class="py-3 px-4 whitespace-nowrap">
                    @if (item.status === 2) {
                      <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-semibold bg-emerald-50 text-emerald-700">
                        <span class="w-1.5 h-1.5 rounded-full bg-emerald-500"></span>
                        Yayında
                      </span>
                    } @else if (item.status === 0) {
                      <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-semibold bg-amber-50 text-amber-700">
                        <span class="w-1.5 h-1.5 rounded-full bg-amber-500"></span>
                        Taslak
                      </span>
                    } @else if (item.status === 1) {
                      <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-semibold bg-blue-50 text-blue-700">
                        <span class="w-1.5 h-1.5 rounded-full bg-blue-500"></span>
                        İncelemede
                      </span>
                    } @else {
                      <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-semibold bg-slate-100 text-slate-600">
                        Arşiv
                      </span>
                    }
                  </td>

                  <!-- Manşet / Son Dakika Rozeti -->
                  <td class="py-3 px-4 text-center whitespace-nowrap">
                    <div class="inline-flex items-center gap-1.5">
                      @if (item.headlineOrder > 0) {
                        <span class="px-1.5 py-0.5 rounded bg-red-100 text-red-700 text-[9px] font-bold">
                          Manşet #{{ item.headlineOrder }}
                        </span>
                      }
                      @if (item.isBreaking) {
                        <span class="px-1.5 py-0.5 rounded bg-amber-100 text-amber-800 text-[9px] font-bold animate-pulse">
                          Son Dakika
                        </span>
                      }
                    </div>
                  </td>

                  <!-- Okunma -->
                  <td class="py-3 px-4 text-right font-semibold text-slate-700 whitespace-nowrap">
                    {{ item.viewCount | number }}
                  </td>

                  <!-- Tarih -->
                  <td class="py-3 px-4 text-right text-slate-500 whitespace-nowrap text-[10px]">
                    {{ (item.publishedAt || item.createdAt) | date:'dd.MM.yyyy HH:mm' }}
                  </td>

                  <!-- İşlemler -->
                  <td class="py-3 px-4 text-right whitespace-nowrap">
                    <div class="inline-flex items-center gap-1">
                      <a 
                        [routerLink]="['/admin/news/edit', item.id]" 
                        class="p-1.5 text-slate-500 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                        title="Düzenle"
                      >
                        <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>
                      </a>
                      <button 
                        (click)="confirmDelete(item.id, item.title)" 
                        class="p-1.5 text-slate-500 hover:text-rose-600 hover:bg-rose-50 rounded-lg transition-colors delete-news-btn"
                        title="Sil"
                      >
                        <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
                      </button>
                    </div>
                  </td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="9" class="py-12 text-center text-slate-400">
                    <div class="max-w-xs mx-auto space-y-2">
                      <svg class="w-8 h-8 text-slate-300 mx-auto" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 20H5a2 2 0 01-2-2V6a2 2 0 012-2h10a2 2 0 012 2v1m2 13a2 2 0 01-2-2V7m2 13a2 2 0 002-2V9a2 2 0 00-2-2h-2m-4-3H9M7 16h6M7 8h6v4H7V8z"/></svg>
                      <p class="text-xs font-medium text-slate-600">Aranan kriterlere uygun haber bulunamadı.</p>
                    </div>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>

        <!-- SAYFALAMA -->
        @if (pagedResult() && pagedResult()!.totalPages > 1) {
          <div class="px-4 py-3 border-t border-slate-200 bg-slate-50/50 flex items-center justify-between">
            <span class="text-xs text-slate-500">
              Toplam <span class="font-bold text-slate-800">{{ pagedResult()?.totalCount }}</span> haberden 
              <span class="font-medium">{{ (currentPage() - 1) * pageSize + 1 }}</span> - 
              <span class="font-medium">{{ Math.min(currentPage() * pageSize, pagedResult()!.totalCount) }}</span> arası gösteriliyor
            </span>

            <div class="flex items-center gap-1.5">
              <button 
                [disabled]="currentPage() === 1"
                (click)="goToPage(currentPage() - 1)"
                class="px-2.5 py-1 text-xs border border-slate-200 rounded-lg hover:bg-white disabled:opacity-40 disabled:cursor-not-allowed text-slate-700 font-medium transition-colors"
              >
                Önceki
              </button>
              <span class="px-2 text-xs font-semibold text-slate-800">{{ currentPage() }} / {{ pagedResult()?.totalPages }}</span>
              <button 
                [disabled]="currentPage() === pagedResult()?.totalPages"
                (click)="goToPage(currentPage() + 1)"
                class="px-2.5 py-1 text-xs border border-slate-200 rounded-lg hover:bg-white disabled:opacity-40 disabled:cursor-not-allowed text-slate-700 font-medium transition-colors"
              >
                Sonraki
              </button>
            </div>
          </div>
        }
      </div>

      <!-- SİLME ONAY MODALI -->
      @if (newsToDelete()) {
        <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/60 backdrop-blur-sm">
          <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-md w-full p-6 space-y-4">
            <div class="flex items-center gap-3">
              <div class="w-10 h-10 rounded-full bg-rose-100 text-rose-600 flex items-center justify-center shrink-0">
                <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
              </div>
              <div>
                <h3 class="text-base font-bold text-slate-900">Haberi Sil</h3>
                <p class="text-xs text-slate-500 mt-0.5">Bu işlem geri alınamaz.</p>
              </div>
            </div>
            <p class="text-sm text-slate-600">
              <span class="font-semibold text-slate-800">"{{ newsToDelete()?.title }}"</span> başlıklı haberi silmek istediğinize emin misiniz?
            </p>
            <div class="flex items-center justify-end gap-2 pt-2">
              <button 
                (click)="cancelDelete()"
                id="cancel-delete-btn"
                class="px-4 py-2 text-xs font-semibold text-slate-600 hover:bg-slate-100 rounded-xl transition-colors"
              >
                Vazgeç
              </button>
              <button 
                (click)="executeDelete()"
                id="confirm-delete-btn"
                class="px-4 py-2 text-xs font-semibold text-white bg-rose-600 hover:bg-rose-700 rounded-xl shadow-lg shadow-rose-600/20 transition-all"
              >
                Evet, Sil
              </button>
            </div>
          </div>
        </div>
      }
    </div>
  `
})
export class NewsListComponent implements OnInit {
  pagedResult = signal<PagedResult<NewsListDto> | null>(null);
  categories = signal<CategoryDto[]>([]);

  searchQuery = '';
  selectedCategory: number | null = null;
  selectedStatus: number | null = null;
  currentPage = signal(1);
  pageSize = 20;
  Math = Math;

  private searchTimeout: any;

  constructor(
    private newsService: NewsService,
    private categoryService: CategoryService,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    this.loadCategories();
    this.loadNews();
  }

  loadCategories() {
    this.categoryService.getFlat().subscribe({
      next: (res) => this.categories.set(res),
      error: (err) => console.error(err)
    });
  }

  loadNews() {
    this.newsService.getAdminNews({
      page: this.currentPage(),
      pageSize: this.pageSize,
      search: this.searchQuery || undefined,
      categoryId: this.selectedCategory || undefined,
      status: this.selectedStatus !== null ? (this.selectedStatus as NewsStatus) : undefined
    }).subscribe({
      next: (res) => this.pagedResult.set(res),
      error: (err) => {
        console.error(err);
        this.toastService.error('Hata', 'Haberler yüklenirken bir sorun oluştu.');
      }
    });
  }

  onSearchChange() {
    clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      this.currentPage.set(1);
      this.loadNews();
    }, 350);
  }

  goToPage(page: number) {
    this.currentPage.set(page);
    this.loadNews();
  }

  newsToDelete = signal<{ id: string; title: string } | null>(null);

  confirmDelete(id: string, title: string) {
    this.newsToDelete.set({ id, title });
  }

  cancelDelete() {
    this.newsToDelete.set(null);
  }

  executeDelete() {
    const item = this.newsToDelete();
    if (!item) return;

    this.newsService.deleteNews(item.id).subscribe({
      next: () => {
        this.toastService.success('Silindi', 'Haber başarıyla silindi.');
        this.newsToDelete.set(null);
        this.loadNews();
      },
      error: () => {
        this.toastService.error('Hata', 'Haber silinemedi.');
        this.newsToDelete.set(null);
      }
    });
  }
}
