import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../../core/services/admin.service';
import { AuditLog, PaginatedAuditLogResponse } from '../../../../core/models/admin.model';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="space-y-6">
      <!-- Başlık ve Filtreleme Alanı -->
      <div class="bg-white p-6 rounded-3xl border border-[#E3D7C1] shadow-sm flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h2 class="text-xl font-extrabold text-[#3A2418]">Sistem Değişiklik Geçmişi (Audit Logs)</h2>
          <p class="text-xs text-[#725B4D]">Veritabanında yapılan tüm ürün, kategori ve fiyat değişikliklerinin eksiksiz denetim kaydı.</p>
        </div>

        <!-- Filtreler -->
        <div class="flex flex-wrap gap-2.5">
          <select 
            [(ngModel)]="selectedActionType" 
            (change)="onFilterChange()"
            class="text-xs font-bold bg-[#F7F1E3] border border-[#E3D7C1] rounded-2xl px-3 py-2 text-[#3A2418] focus:outline-none">
            <option value="">Tüm İşlemler</option>
            <option value="CREATE">Ekleme (CREATE)</option>
            <option value="UPDATE">Güncelleme (UPDATE)</option>
            <option value="DELETE">Silme (DELETE)</option>
          </select>

          <select 
            [(ngModel)]="selectedEntityName" 
            (change)="onFilterChange()"
            class="text-xs font-bold bg-[#F7F1E3] border border-[#E3D7C1] rounded-2xl px-3 py-2 text-[#3A2418] focus:outline-none">
            <option value="">Tüm Varlıklar</option>
            <option value="Product">Ürünler</option>
            <option value="Category">Kategoriler</option>
            <option value="ProductCategory">Kategori Eşleşmeleri</option>
          </select>

          <input 
            type="text"
            [(ngModel)]="searchTerm"
            (keyup.enter)="onFilterChange()"
            placeholder="Açıklama veya kullanıcı ara..."
            class="text-xs bg-[#F7F1E3] border border-[#E3D7C1] rounded-2xl px-3 py-2 text-[#3A2418] focus:outline-none"
          />
        </div>
      </div>

      <!-- Log Tablosu -->
      <div class="bg-white rounded-3xl border border-[#E3D7C1] overflow-hidden shadow-sm">
        @if (isLoading()) {
          <div class="py-16 text-center text-[#725B4D] text-xs font-semibold">Denetim logları yükleniyor...</div>
        } @else if (logs().length === 0) {
          <div class="py-16 text-center text-[#725B4D] text-xs">Kayıtlı log bulunamadı.</div>
        } @else {
          <div class="overflow-x-auto">
            <table class="w-full text-left text-sm text-[#3A2418]">
              <thead class="text-xs uppercase bg-[#F7F1E3] text-[#725B4D] border-b border-[#E3D7C1]">
                <tr>
                  <th class="p-4">Tarih</th>
                  <th class="p-4">Kullanıcı</th>
                  <th class="p-4">İşlem</th>
                  <th class="p-4">Varlık</th>
                  <th class="p-4">Açıklama</th>
                  <th class="p-4">IP Adresi</th>
                  <th class="p-4 text-right">Detay</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-[#EFE7D5]">
                @for (log of logs(); track log.id) {
                  <tr class="hover:bg-[#F7F1E3]/40 transition-colors">
                    <td class="p-4 text-xs font-semibold whitespace-nowrap text-[#725B4D]">
                      {{ log.timestamp | date:'dd.MM.yyyy HH:mm:ss' }}
                    </td>
                    <td class="p-4 font-bold text-xs">{{ log.userId || 'Admin' }}</td>
                    <td class="p-4">
                      <span [class.bg-emerald-100]="log.actionType === 'CREATE'"
                            [class.text-emerald-800]="log.actionType === 'CREATE'"
                            [class.bg-blue-100]="log.actionType === 'UPDATE'"
                            [class.text-blue-800]="log.actionType === 'UPDATE'"
                            [class.bg-red-100]="log.actionType === 'DELETE'"
                            [class.text-red-800]="log.actionType === 'DELETE'"
                            class="px-2.5 py-1 rounded-full text-[11px] font-bold">
                        {{ log.actionType }}
                      </span>
                    </td>
                    <td class="p-4 text-xs font-bold text-[#3A2418]">{{ log.entityName }}</td>
                    <td class="p-4 text-xs text-[#725B4D] max-w-xs">{{ log.description }}</td>
                    <td class="p-4 text-xs text-gray-400 font-mono">{{ log.ipAddress || '127.0.0.1' }}</td>
                    <td class="p-4 text-right">
                      @if (log.oldValuesJson || log.newValuesJson) {
                        <button 
                          (click)="openDetailModal(log)"
                          class="px-3 py-1 bg-[#F7F1E3] hover:bg-[#3A2418] hover:text-white rounded-xl text-xs font-bold transition-colors cursor-pointer">
                          JSON Gör
                        </button>
                      }
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>

          <!-- Sayfalama (Pagination) -->
          <div class="p-4 border-t border-[#EFE7D5] flex items-center justify-between text-xs text-[#725B4D]">
            <span>Toplam {{ totalCount() }} işlem kaydı</span>
            <div class="flex gap-2">
              <button 
                [disabled]="currentPage() <= 1"
                (click)="changePage(currentPage() - 1)"
                class="px-3 py-1 rounded-xl bg-[#F7F1E3] disabled:opacity-40 font-bold hover:bg-[#3A2418] hover:text-white transition-colors cursor-pointer">
                Önceki
              </button>
              <span class="px-3 py-1 font-bold text-[#3A2418]">{{ currentPage() }} / {{ totalPages() || 1 }}</span>
              <button 
                [disabled]="currentPage() >= totalPages()"
                (click)="changePage(currentPage() + 1)"
                class="px-3 py-1 rounded-xl bg-[#F7F1E3] disabled:opacity-40 font-bold hover:bg-[#3A2418] hover:text-white transition-colors cursor-pointer">
                Sonraki
              </button>
            </div>
          </div>
        }
      </div>

      <!-- JSON Değişiklik Detay Modalı -->
      @if (selectedLog(); as log) {
        <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-fade-in">
          <div class="bg-white rounded-3xl max-w-2xl w-full p-6 shadow-2xl border border-[#E3D7C1] relative animate-scale-up max-h-[85vh] overflow-y-auto">
            <h3 class="text-lg font-extrabold text-[#3A2418] mb-1">Log Detayı & Değişiklik Değerleri</h3>
            <p class="text-xs text-[#725B4D] mb-4">{{ log.description }} ({{ log.timestamp | date:'dd.MM.yyyy HH:mm:ss' }})</p>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 text-xs font-mono mb-6">
              <!-- Eski Değerler -->
              <div class="bg-red-50 p-4 rounded-2xl border border-red-100">
                <div class="font-bold text-red-800 mb-2 font-sans">Eski Değerler (Old Values)</div>
                <pre class="whitespace-pre-wrap break-all text-red-900">{{ log.oldValuesJson || 'Eski değer yok (Yeni kayıt)' }}</pre>
              </div>

              <!-- Yeni Değerler -->
              <div class="bg-emerald-50 p-4 rounded-2xl border border-emerald-100">
                <div class="font-bold text-emerald-800 mb-2 font-sans">Yeni Değerler (New Values)</div>
                <pre class="whitespace-pre-wrap break-all text-emerald-900">{{ log.newValuesJson || 'Yeni değer yok (Silindi)' }}</pre>
              </div>
            </div>

            <div class="flex justify-end">
              <button 
                (click)="closeDetailModal()"
                class="px-5 py-2.5 bg-[#3A2418] text-[#F7F1E3] rounded-xl font-bold text-xs hover:bg-[#2A180E] cursor-pointer">
                Kapat
              </button>
            </div>
          </div>
        </div>
      }
    </div>
  `
})
export class AuditLogsComponent implements OnInit {
  private adminService = inject(AdminService);

  logs = signal<AuditLog[]>([]);
  isLoading = signal<boolean>(false);
  totalCount = signal<number>(0);
  totalPages = signal<number>(1);
  currentPage = signal<number>(1);
  pageSize = 15;

  selectedActionType = '';
  selectedEntityName = '';
  searchTerm = '';

  selectedLog = signal<AuditLog | null>(null);

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(): void {
    this.isLoading.set(true);
    this.adminService.getAuditLogs(
      this.currentPage(),
      this.pageSize,
      this.selectedActionType || undefined,
      this.selectedEntityName || undefined,
      this.searchTerm || undefined
    ).subscribe({
      next: (res: PaginatedAuditLogResponse) => {
        this.logs.set(res.data);
        this.totalCount.set(res.totalCount);
        this.totalPages.set(res.totalPages);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  onFilterChange(): void {
    this.currentPage.set(1);
    this.loadLogs();
  }

  changePage(page: number): void {
    this.currentPage.set(page);
    this.loadLogs();
  }

  openDetailModal(log: AuditLog): void {
    this.selectedLog.set(log);
  }

  closeDetailModal(): void {
    this.selectedLog.set(null);
  }
}
