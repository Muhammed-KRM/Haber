import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../../core/services/admin.service';
import { FeedbackModalService } from '../../../../core/services/feedback-modal.service';
import { AdminCategory, AdminCategoryDto } from '../../../../core/models/admin.model';
import { ImageUrlPipe } from '../../../../shared/pipes/image-url.pipe';

@Component({
  selector: 'app-category-manager',
  standalone: true,
  imports: [CommonModule, FormsModule, ImageUrlPipe],
  template: `
    <div class="space-y-6">
      <!-- Başlık ve Yeni Ekle Butonu -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-6 rounded-3xl border border-[#E3D7C1] shadow-sm">
        <div>
          <h2 class="text-xl font-extrabold text-[#3A2418]">Kategori Yönetimi</h2>
          <p class="text-xs text-[#725B4D]">Menüdeki ana kategorileri oluşturun, sıralayın veya düzenleyin.</p>
        </div>
        <button 
          (click)="openModal()"
          class="px-5 py-2.5 bg-[#C65D3A] hover:bg-[#AA4B2B] text-white rounded-2xl font-bold text-xs shadow-md transition-all active:scale-95 flex items-center gap-2 w-fit cursor-pointer">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M12 4v16m8-8H4"></path>
          </svg>
          <span>Yeni Kategori Ekle</span>
        </button>
      </div>

      <!-- Kategori Listesi Tablosu -->
      <div class="bg-white rounded-3xl border border-[#E3D7C1] overflow-hidden shadow-sm">
        @if (isLoading()) {
          <div class="py-16 text-center text-[#725B4D] text-xs font-semibold">Kategoriler yükleniyor...</div>
        } @else if (categories().length === 0) {
          <div class="py-16 text-center text-[#725B4D] text-xs">Henüz kategori eklenmedi.</div>
        } @else {
          <div class="overflow-x-auto">
            <table class="w-full text-left text-sm text-[#3A2418]">
              <thead class="text-xs uppercase bg-[#F7F1E3] text-[#725B4D] border-b border-[#E3D7C1]">
                <tr>
                  <th class="p-4">Görsel</th>
                  <th class="p-4">Kategori Adı</th>
                  <th class="p-4">Sıra</th>
                  <th class="p-4">Ürün Sayısı</th>
                  <th class="p-4">Durum</th>
                  <th class="p-4 text-right">İşlemler</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-[#EFE7D5]">
                @for (cat of categories(); track cat.id) {
                  <tr class="hover:bg-[#F7F1E3]/40 transition-colors">
                    <td class="p-4">
                      @if (cat.imageUrl) {
                        <img [src]="cat.imageUrl | imageUrl" alt="" class="w-10 h-10 rounded-xl object-cover border border-[#E3D7C1]" />
                      } @else {
                        <div class="w-10 h-10 rounded-xl bg-[#EFE7D5] flex items-center justify-center text-[#725B4D] text-xs font-bold">
                          📁
                        </div>
                      }
                    </td>
                    <td class="p-4 font-bold text-[#3A2418]">{{ cat.name }}</td>
                    <td class="p-4 font-semibold text-[#725B4D]">{{ cat.displayOrder }}</td>
                    <td class="p-4">
                      <span class="px-2.5 py-0.5 rounded-full bg-[#3A2418]/10 text-[#3A2418] text-xs font-bold">
                        {{ cat.productCount || 0 }} ürün
                      </span>
                    </td>
                    <td class="p-4">
                      <span [class.bg-emerald-100]="cat.isActive"
                            [class.text-emerald-800]="cat.isActive"
                            [class.bg-gray-100]="!cat.isActive"
                            [class.text-gray-800]="!cat.isActive"
                            class="px-2.5 py-1 rounded-full text-xs font-bold">
                        {{ cat.isActive ? 'Aktif' : 'Pasif' }}
                      </span>
                    </td>
                    <td class="p-4 text-right space-x-2">
                      <button 
                        (click)="openModal(cat)"
                        class="p-2 text-[#3A2418] hover:bg-[#F7F1E3] rounded-xl transition-colors cursor-pointer"
                        title="Düzenle">
                        ✏️
                      </button>
                      <button 
                        (click)="deleteCategory(cat)"
                        class="p-2 text-red-600 hover:bg-red-50 rounded-xl transition-colors cursor-pointer"
                        title="Sil">
                        🗑️
                      </button>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      </div>

      <!-- Kategori Ekleme / Düzenleme Modalı -->
      @if (showModal()) {
        <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-fade-in">
          <div class="bg-white rounded-3xl max-w-md w-full p-6 shadow-2xl border border-[#E3D7C1] relative animate-scale-up">
            <h3 class="text-xl font-extrabold text-[#3A2418] mb-4">
              {{ editingCategory() ? 'Kategoriyi Düzenle' : 'Yeni Kategori Ekle' }}
            </h3>

            <div class="space-y-4">
              <div>
                <label class="block text-xs font-bold text-[#725B4D] uppercase mb-1">Kategori Adı *</label>
                <input 
                  type="text" 
                  [(ngModel)]="formData.name"
                  placeholder="Örn: Tatlılar, İçecekler"
                  class="w-full bg-[#F7F1E3] text-[#3A2418] px-4 py-2.5 rounded-xl text-sm border border-[#E3D7C1] focus:border-[#C65D3A] focus:outline-none"
                />
              </div>

              <div>
                <label class="block text-xs font-bold text-[#725B4D] uppercase mb-1">Görsel Yükle / URL</label>
                <div class="flex gap-2 mb-2">
                  <input 
                    type="text" 
                    [(ngModel)]="formData.imageUrl"
                    placeholder="https://... veya dosya yükleyin"
                    class="w-full bg-[#F7F1E3] text-[#3A2418] px-4 py-2 rounded-xl text-xs border border-[#E3D7C1] focus:outline-none"
                  />
                  <label class="px-3 py-2 bg-[#3A2418] text-white rounded-xl text-xs font-bold hover:bg-[#2A180E] cursor-pointer whitespace-nowrap">
                    <span>Dosya Seç</span>
                    <input type="file" (change)="onFileSelected($event)" accept="image/*" class="hidden" />
                  </label>
                </div>
                @if (isUploading()) {
                  <p class="text-[11px] text-[#C65D3A] font-semibold">Görsel yükleniyor...</p>
                }
              </div>

              <div class="grid grid-cols-2 gap-4">
                <div>
                  <label class="block text-xs font-bold text-[#725B4D] uppercase mb-1">Sıralama (Sıra No)</label>
                  <input 
                    type="number" 
                    [(ngModel)]="formData.displayOrder"
                    class="w-full bg-[#F7F1E3] text-[#3A2418] px-4 py-2.5 rounded-xl text-sm border border-[#E3D7C1] focus:outline-none"
                  />
                </div>

                <div>
                  <label class="block text-xs font-bold text-[#725B4D] uppercase mb-1">Menüde Gösterilsin mi?</label>
                  <select 
                    [(ngModel)]="formData.isActive"
                    class="w-full bg-[#F7F1E3] text-[#3A2418] px-3 py-2.5 rounded-xl text-sm border border-[#E3D7C1] focus:outline-none">
                    <option [ngValue]="true">Aktif</option>
                    <option [ngValue]="false">Pasif</option>
                  </select>
                </div>
              </div>
            </div>

            <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-[#EFE7D5]">
              <button 
                (click)="closeModal()"
                [disabled]="isSaving()"
                class="px-4 py-2.5 text-xs font-bold text-[#725B4D] hover:bg-[#F7F1E3] rounded-xl transition-colors cursor-pointer disabled:opacity-50">
                İptal
              </button>
              <button 
                (click)="saveCategory()"
                [disabled]="!formData.name || isSaving()"
                class="px-5 py-2.5 bg-[#C65D3A] hover:bg-[#AA4B2B] disabled:opacity-50 text-white rounded-xl font-bold text-xs transition-colors cursor-pointer flex items-center gap-2">
                @if (isSaving()) {
                  <span class="w-3.5 h-3.5 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
                  <span>Kaydediliyor...</span>
                } @else {
                  <span>Kaydet</span>
                }
              </button>
            </div>
          </div>
        </div>
      }
    </div>
  `
})
export class CategoryManagerComponent implements OnInit {
  private adminService = inject(AdminService);
  private feedbackService = inject(FeedbackModalService);

  categories = signal<AdminCategory[]>([]);
  isLoading = signal<boolean>(false);
  isSaving = signal<boolean>(false);
  showModal = signal<boolean>(false);
  isUploading = signal<boolean>(false);
  editingCategory = signal<AdminCategory | null>(null);

  formData: AdminCategoryDto = {
    name: '',
    imageUrl: '',
    displayOrder: 0,
    isActive: true
  };

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.isLoading.set(true);
    this.adminService.getCategories().subscribe({
      next: (data: AdminCategory[]) => {
        this.categories.set(data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  openModal(category?: AdminCategory): void {
    if (category) {
      this.editingCategory.set(category);
      this.formData = {
        name: category.name,
        imageUrl: category.imageUrl || '',
        displayOrder: category.displayOrder,
        isActive: category.isActive
      };
    } else {
      this.editingCategory.set(null);
      this.formData = {
        name: '',
        imageUrl: '',
        displayOrder: this.categories().length + 1,
        isActive: true
      };
    }
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.isUploading.set(true);
    this.adminService.uploadImage(file).subscribe({
      next: (res: { fileName: string; imageUrl: string; size: number }) => {
        this.formData.imageUrl = `https://localhost:7000${res.imageUrl}`;
        this.isUploading.set(false);
      },
      error: () => {
        this.feedbackService.showError('Görsel yüklenirken bir hata oluştu.', 'Yükleme Hatası');
        this.isUploading.set(false);
      }
    });
  }

  saveCategory(): void {
    if (!this.formData.name) return;

    this.isSaving.set(true);
    const editing = this.editingCategory();
    if (editing) {
      this.adminService.updateCategory(editing.id, this.formData).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.closeModal();
          this.loadCategories();
          this.feedbackService.showSuccess(`"${this.formData.name}" kategorisi başarıyla güncellendi.`, 'İşlem Başarılı');
        },
        error: (err: unknown) => {
          this.isSaving.set(false);
          console.error('Kategori güncellenirken hata:', err);
          this.feedbackService.showError('Kategori güncellenirken bir hata oluştu.', 'Güncelleme Başarısız');
        }
      });
    } else {
      this.adminService.createCategory(this.formData).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.closeModal();
          this.loadCategories();
          this.feedbackService.showSuccess(`"${this.formData.name}" kategorisi menüye eklendi.`, 'Kategori Eklendi');
        },
        error: (err: unknown) => {
          this.isSaving.set(false);
          console.error('Kategori eklenirken hata:', err);
          this.feedbackService.showError('Kategori eklenirken bir hata oluştu.', 'Kayıt Başarısız');
        }
      });
    }
  }

  deleteCategory(cat: AdminCategory): void {
    if (confirm(`"${cat.name}" kategorisini silmek istediğinize emin misiniz?`)) {
      this.adminService.deleteCategory(cat.id).subscribe({
        next: () => {
          this.loadCategories();
          this.feedbackService.showSuccess(`"${cat.name}" kategorisi silindi.`, 'Kategori Silindi');
        },
        error: () => {
          this.feedbackService.showError('Kategori silinirken bir hata oluştu.', 'Silme Başarısız');
        }
      });
    }
  }
}
