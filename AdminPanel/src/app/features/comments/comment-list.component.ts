import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CommentService } from '../../core/services/comment.service';
import { ToastService } from '../../core/services/toast.service';
import { CommentDto, CommentStatus } from '../../core/models/comment.model';
import { PagedResult } from '../../core/models/news.model';

@Component({
  selector: 'app-comment-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-6 animate-fade-in max-w-5xl">
      <!-- Başlık & Bilgi -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="text-2xl font-extrabold text-slate-900 tracking-tight">Yorum Moderasyonu</h1>
          <p class="text-xs text-slate-500 mt-1">Okuyucular tarafından gönderilen ve onay bekleyen yorumları yönetin.</p>
        </div>

        <button 
          (click)="loadComments()"
          class="px-3.5 py-2 bg-white border border-slate-200 hover:bg-slate-50 text-slate-700 text-xs font-semibold rounded-lg shadow-xs transition-colors self-start sm:self-auto flex items-center gap-1.5"
        >
          <svg class="w-3.5 h-3.5 text-slate-500" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/></svg>
          Listeyi Yenile
        </button>
      </div>

      <!-- BEKLEYEN YORUMLAR LİSTESİ -->
      <div class="premium-card overflow-hidden">
        <div class="p-4 border-b border-slate-200 bg-slate-50/50 flex items-center justify-between">
          <span class="text-xs font-bold text-slate-700">Onay Bekleyenler Kuyruğu</span>
          <span class="text-[11px] font-semibold text-amber-700 bg-amber-50 px-2 py-0.5 rounded-full">
            {{ pagedResult()?.totalCount || 0 }} adet bekliyor
          </span>
        </div>

        <div class="divide-y divide-slate-100">
          @for (comment of pagedResult()?.items || []; track comment.id) {
            <div class="p-5 hover:bg-slate-50/60 transition-colors flex flex-col sm:flex-row sm:items-start justify-between gap-4">
              <div class="space-y-1.5 flex-1 min-w-0">
                <div class="flex items-center gap-2">
                  <span class="text-xs font-bold text-slate-900">{{ comment.authorName }}</span>
                  <span class="text-[10px] text-slate-400">&bull; {{ comment.createdAt | date:'dd.MM.yyyy HH:mm' }}</span>
                </div>

                <!-- Yorum Metni -->
                <p class="text-xs text-slate-700 bg-slate-50 p-3 rounded-xl border border-slate-200 leading-relaxed font-serif">
                  "{{ comment.content }}"
                </p>
              </div>

              <!-- Moderasyon Aksiyon Butonları -->
              <div class="flex items-center gap-2 shrink-0 self-end sm:self-start">
                <!-- ONAYLA -->
                <button 
                  (click)="moderate(comment.id, 1)"
                  class="px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-semibold rounded-lg shadow-xs transition-colors flex items-center gap-1"
                >
                  <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/></svg>
                  Onayla
                </button>

                <!-- REDDET -->
                <button 
                  (click)="moderate(comment.id, 2)"
                  class="px-3 py-1.5 bg-amber-600 hover:bg-amber-700 text-white text-xs font-semibold rounded-lg shadow-xs transition-colors flex items-center gap-1"
                >
                  <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
                  Reddet
                </button>

                <!-- SPAM OLARAK İŞARETLE -->
                <button 
                  (click)="moderate(comment.id, 3)"
                  class="p-1.5 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg transition-colors"
                  title="Spam Olarak İşaretle"
                >
                  <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636"/></svg>
                </button>
              </div>
            </div>
          } @empty {
            <div class="py-12 text-center text-slate-400 text-xs">
              Onay bekleyen yeni yorum bulunmuyor. Tüm yorumlar incelendi!
            </div>
          }
        </div>
      </div>
    </div>
  `
})
export class CommentListComponent implements OnInit {
  pagedResult = signal<PagedResult<CommentDto> | null>(null);
  currentPage = 1;
  pageSize = 20;

  constructor(
    private commentService: CommentService,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    this.loadComments();
  }

  loadComments() {
    this.commentService.getPending(this.currentPage, this.pageSize).subscribe({
      next: (res) => this.pagedResult.set(res),
      error: (err) => console.error(err)
    });
  }

  moderate(id: string, status: CommentStatus) {
    this.commentService.moderate(id, status).subscribe({
      next: () => {
        const msg = status === 1 ? 'Yorum onaylandı.' : (status === 2 ? 'Yorum reddedildi.' : 'Spam olarak işaretlendi.');
        this.toastService.success('Tamamlandı', msg);
        this.loadComments();
      },
      error: () => this.toastService.error('Hata', 'İşlem gerçekleştirilemedi.')
    });
  }
}
