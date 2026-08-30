import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MediaService } from '../../core/services/media.service';
import { ToastService } from '../../core/services/toast.service';
import { MediaDto } from '../../core/models/media.model';
import { PagedResult } from '../../core/models/news.model';

@Component({
  selector: 'app-media-gallery',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="space-y-6 animate-fade-in max-w-6xl">
      <!-- Başlık & Yükleme Alanı -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="text-2xl font-extrabold text-slate-900 tracking-tight">Medya Kütüphanesi</h1>
          <p class="text-xs text-slate-500 mt-1">Haberlerde kullanılan tüm görsel ve video dosyalarının merkezi yönetimi.</p>
        </div>

        <!-- Hızlı Dosya Yükle Butonu -->
        <label class="inline-flex items-center gap-2 px-4 py-2 bg-gradient-to-r from-red-600 to-rose-600 hover:from-red-700 hover:to-rose-700 text-white text-xs font-semibold rounded-lg shadow-sm shadow-red-500/20 hover:shadow-md transition-all cursor-pointer self-start sm:self-auto">
          <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12"/></svg>
          <span>Yeni Dosya Yükle</span>
          <input type="file" (change)="onFileSelected($event)" accept="image/*,video/mp4" class="hidden" />
        </label>
      </div>

      <!-- SÜRÜKLE BIRAK YÜKLEME ALANI -->
      <div 
        (dragover)="onDragOver($event)" 
        (dragleave)="onDragLeave($event)"
        (drop)="onDrop($event)"
        class="border-2 border-dashed rounded-2xl p-6 transition-all text-center relative flex flex-col items-center justify-center min-h-[140px]"
        [ngClass]="isDragging() ? 'border-red-500 bg-red-50/50' : 'border-slate-200 bg-white hover:border-slate-300'"
      >
        <div class="w-10 h-10 rounded-full bg-red-50 text-red-600 flex items-center justify-center mb-2">
          <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12"/></svg>
        </div>
        <p class="text-xs font-bold text-slate-800">Dosyaları buraya sürükleyip bırakın veya seçin</p>
        <p class="text-[10px] text-slate-400 mt-0.5">JPEG, PNG, WebP, GIF, MP4 &bull; Max 50 MB</p>
        
        @if (isUploading()) {
          <div class="mt-3 flex items-center gap-2 text-xs text-red-600 font-semibold animate-pulse">
            <svg class="animate-spin w-4 h-4" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"></path></svg>
            <span>Dosya yükleniyor ve işleniyor...</span>
          </div>
        }
      </div>

      <!-- MEDYA GRID LİSTESİ -->
      <div class="premium-card p-5 space-y-4">
        <!-- Arama Çubuğu -->
        <div class="flex items-center justify-between">
          <div class="relative w-full max-w-xs">
            <span class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-slate-400">
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
            </span>
            <input 
              type="text" 
              [(ngModel)]="searchQuery"
              (ngModelChange)="onSearchChange()"
              placeholder="Dosya veya alt metin ara..." 
              class="w-full pl-9 pr-3 py-1.5 text-xs bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
            />
          </div>
          <span class="text-xs text-slate-400 font-medium">Toplam {{ pagedResult()?.totalCount || 0 }} dosya</span>
        </div>

        <!-- Görsel Kartları -->
        <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-3.5">
          @for (item of pagedResult()?.items || []; track item.id) {
            <div class="group relative bg-slate-50 border border-slate-200 rounded-xl overflow-hidden hover:shadow-md transition-all flex flex-col justify-between">
              <!-- Önizleme -->
              <div class="h-28 w-full bg-slate-200 overflow-hidden relative">
                @if (item.mimeType.startsWith('image/')) {
                  <img [src]="item.originalUrl" class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300" loading="lazy" />
                } @else {
                  <div class="w-full h-full flex items-center justify-center bg-slate-800 text-white text-xs font-bold">
                    VIDEO
                  </div>
                }

                <!-- Hover Aksiyonları -->
                <div class="absolute inset-0 bg-slate-950/60 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center gap-2">
                  <button 
                    (click)="copyUrl(item.originalUrl)"
                    class="p-1.5 bg-white text-slate-800 rounded-lg hover:bg-slate-100 shadow transition-colors"
                    title="URL Kopyala"
                  >
                    <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 5H6a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2v-1M8 5a2 2 0 002 2h2a2 2 0 002-2M8 5a2 2 0 012-2h2a2 2 0 012 2m0 0h2a2 2 0 012 2v3m2 4H10m0 0l3-3m-3 3l3 3"/></svg>
                  </button>
                  <button 
                    (click)="deleteMedia(item.id)"
                    class="p-1.5 bg-rose-600 text-white rounded-lg hover:bg-rose-700 shadow transition-colors"
                    title="Sil"
                  >
                    <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
                  </button>
                </div>
              </div>

              <!-- Alt Bilgi -->
              <div class="p-2 text-[10px]">
                <p class="font-semibold text-slate-700 truncate" [title]="item.altText || item.originalUrl">
                  {{ item.altText || 'İsimsiz dosya' }}
                </p>
                <div class="flex items-center justify-between text-slate-400 mt-0.5 font-mono">
                  <span>{{ formatSize(item.fileSizeBytes) }}</span>
                  <span>{{ item.uploadedAt | date:'dd.MM' }}</span>
                </div>
              </div>
            </div>
          } @empty {
            <div class="col-span-full py-12 text-center text-slate-400 text-xs">
              Kütüphanede henüz yüklenmiş medya dosyası bulunmuyor.
            </div>
          }
        </div>
      </div>
    </div>
  `
})
export class MediaGalleryComponent implements OnInit {
  pagedResult = signal<PagedResult<MediaDto> | null>(null);
  searchQuery = '';
  isDragging = signal(false);
  isUploading = signal(false);
  currentPage = 1;
  pageSize = 24;

  private searchTimeout: any;

  constructor(
    private mediaService: MediaService,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    this.loadMedia();
  }

  loadMedia() {
    this.mediaService.getPagedMedia(this.currentPage, this.pageSize, this.searchQuery || undefined).subscribe({
      next: (res) => this.pagedResult.set(res),
      error: (err) => console.error(err)
    });
  }

  onSearchChange() {
    clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      this.currentPage = 1;
      this.loadMedia();
    }, 350);
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) this.uploadFile(file);
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    this.isDragging.set(true);
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    this.isDragging.set(false);
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragging.set(false);
    if (event.dataTransfer && event.dataTransfer.files.length > 0) {
      this.uploadFile(event.dataTransfer.files[0]);
    }
  }

  uploadFile(file: File) {
    this.isUploading.set(true);
    this.mediaService.upload(file).subscribe({
      next: () => {
        this.isUploading.set(false);
        this.toastService.success('Yüklendi', 'Medya kütüphanesine eklendi.');
        this.loadMedia();
      },
      error: (err) => {
        this.isUploading.set(false);
        this.toastService.error('Hata', err.error?.message || 'Dosya yüklenemedi.');
      }
    });
  }

  copyUrl(url: string) {
    navigator.clipboard.writeText(url).then(() => {
      this.toastService.success('Kopyalandı', 'Görsel URL panoya kopyalandı.');
    });
  }

  deleteMedia(id: string) {
    if (!confirm('Bu medya dosyasını silmek istediğinize emin misiniz?')) return;

    this.mediaService.delete(id).subscribe({
      next: () => {
        this.toastService.success('Silindi', 'Dosya silindi.');
        this.loadMedia();
      },
      error: () => this.toastService.error('Hata', 'Dosya silinemedi.')
    });
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }
}
