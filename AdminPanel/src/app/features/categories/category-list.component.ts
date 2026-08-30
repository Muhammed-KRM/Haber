import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CategoryService } from '../../core/services/category.service';
import { ToastService } from '../../core/services/toast.service';
import { CategoryDto, CreateCategoryDto, UpdateCategoryDto } from '../../core/models/category.model';

@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="space-y-6 animate-fade-in max-w-5xl">
      <!-- Başlık & Yeni Kategori Butonu -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="text-2xl font-extrabold text-slate-900 tracking-tight">Kategori Yönetimi</h1>
          <p class="text-xs text-slate-500 mt-1">Haber kategorilerini, menü sıralamasını ve alt kategorileri düzenleyin.</p>
        </div>

        <button 
          (click)="openCreateModal()"
          class="inline-flex items-center gap-2 px-4 py-2 bg-gradient-to-r from-red-600 to-rose-600 hover:from-red-700 hover:to-rose-700 text-white text-xs font-semibold rounded-lg shadow-sm shadow-red-500/20 hover:shadow-md transition-all self-start sm:self-auto"
        >
          <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/></svg>
          Yeni Kategori Ekle
        </button>
      </div>

      <!-- KATEGORİ AĞACI / LİSTESİ -->
      <div class="premium-card overflow-hidden">
        <div class="p-4 border-b border-slate-200 bg-slate-50/50 flex items-center justify-between">
          <span class="text-xs font-bold text-slate-700">Aktif Kategori Hiyerarşisi</span>
          <span class="text-[11px] text-slate-400">Toplam {{ categories().length }} ana kategori</span>
        </div>

        <div class="divide-y divide-slate-100">
          @for (cat of categories(); track cat.id) {
            <div class="p-4 hover:bg-slate-50/60 transition-colors flex items-center justify-between">
              <div class="flex items-center gap-3">
                <div class="w-8 h-8 rounded-lg bg-red-50 text-red-600 flex items-center justify-center font-bold text-xs">
                  {{ cat.sortOrder }}
                </div>
                <div>
                  <div class="flex items-center gap-2">
                    <h3 class="text-sm font-bold text-slate-800">{{ cat.name }}</h3>
                    <span class="text-[10px] font-mono text-slate-400 bg-slate-100 px-1.5 py-0.5 rounded">/kategori/{{ cat.slug }}</span>
                    @if (cat.isActive) {
                      <span class="w-2 h-2 rounded-full bg-emerald-500" title="Aktif"></span>
                    } @else {
                      <span class="w-2 h-2 rounded-full bg-slate-300" title="Pasif"></span>
                    }
                  </div>
                  @if (cat.description) {
                    <p class="text-xs text-slate-500 mt-0.5">{{ cat.description }}</p>
                  }
                </div>
              </div>

              <!-- İşlem Butonları -->
              <div class="flex items-center gap-1.5">
                <button 
                  (click)="openEditModal(cat)"
                  class="p-1.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                  title="Düzenle"
                >
                  <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>
                </button>
                <button 
                  (click)="deleteCategory(cat.id, cat.name)"
                  class="p-1.5 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg transition-colors"
                  title="Sil"
                >
                  <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
                </button>
              </div>
            </div>

            <!-- Alt Kategoriler -->
            @for (child of cat.children || []; track child.id) {
              <div class="p-3 pl-12 bg-slate-50/40 hover:bg-slate-50 transition-colors flex items-center justify-between border-t border-slate-50">
                <div class="flex items-center gap-2.5">
                  <span class="text-slate-300">&boxur;</span>
                  <div>
                    <span class="text-xs font-semibold text-slate-700">{{ child.name }}</span>
                    <span class="text-[10px] font-mono text-slate-400 ml-2">/kategori/{{ child.slug }}</span>
                  </div>
                </div>

                <div class="flex items-center gap-1">
                  <button (click)="openEditModal(child)" class="p-1 text-slate-400 hover:text-red-600"><svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg></button>
                  <button (click)="deleteCategory(child.id, child.name)" class="p-1 text-slate-400 hover:text-rose-600"><svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg></button>
                </div>
              </div>
            }
          } @empty {
            <div class="p-8 text-center text-slate-400 text-xs">Henüz kategori eklenmemiş.</div>
          }
        </div>
      </div>

      <!-- MODAL: KATEGORİ EKLE / DÜZENLE -->
      @if (showModal()) {
        <div class="fixed inset-0 z-50 bg-slate-950/60 backdrop-blur-sm flex items-center justify-center p-4">
          <div class="bg-white border border-slate-200 rounded-2xl max-w-md w-full p-6 shadow-2xl space-y-4 animate-scale-up">
            <div class="flex items-center justify-between border-b border-slate-100 pb-3">
              <h3 class="text-sm font-bold text-slate-900">
                {{ editingCategory() ? 'Kategoriyi Düzenle' : 'Yeni Kategori Oluştur' }}
              </h3>
              <button (click)="closeModal()" class="text-slate-400 hover:text-slate-600 p-1">
                <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>

            <div class="space-y-3">
              <div>
                <label class="block text-xs font-semibold text-slate-700 mb-1">Kategori Adı *</label>
                <input 
                  type="text" 
                  [(ngModel)]="formData.name" 
                  placeholder="Örn: Gündem, Politika, Ekonomi"
                  class="w-full px-3 py-2 text-xs bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
                />
              </div>

              <div>
                <label class="block text-xs font-semibold text-slate-700 mb-1">Özel Slug (Boş bırakılabilir)</label>
                <input 
                  type="text" 
                  [(ngModel)]="formData.slug" 
                  placeholder="otomatik-olusturulur"
                  class="w-full px-3 py-2 text-xs bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
                />
              </div>

              <div>
                <label class="block text-xs font-semibold text-slate-700 mb-1">Açıklama</label>
                <input 
                  type="text" 
                  [(ngModel)]="formData.description" 
                  placeholder="Kategori hakkında kısa açıklama..."
                  class="w-full px-3 py-2 text-xs bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
                />
              </div>

              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-slate-700 mb-1">Menü Sırası</label>
                  <input 
                    type="number" 
                    [(ngModel)]="formData.sortOrder" 
                    class="w-full px-3 py-2 text-xs bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
                  />
                </div>

                <div class="flex items-center pt-5">
                  <label class="flex items-center gap-2 cursor-pointer text-xs font-medium text-slate-700">
                    <input 
                      type="checkbox" 
                      [(ngModel)]="formData.isActive" 
                      class="rounded border-slate-300 text-red-600 focus:ring-red-500"
                    />
                    <span>Aktif Olarak Yayınla</span>
                  </label>
                </div>
              </div>
            </div>

            <div class="flex items-center justify-end gap-2 pt-2">
              <button 
                type="button" 
                (click)="closeModal()"
                class="px-3.5 py-2 text-xs text-slate-600 hover:bg-slate-100 rounded-lg font-medium transition-colors"
              >
                İptal
              </button>
              <button 
                type="button" 
                (click)="saveCategory()"
                [disabled]="!formData.name"
                class="px-4 py-2 bg-red-600 hover:bg-red-700 text-white text-xs font-semibold rounded-lg shadow-sm transition-all disabled:opacity-50"
              >
                Kaydet
              </button>
            </div>
          </div>
        </div>
      }
    </div>
  `
})
export class CategoryListComponent implements OnInit {
  categories = signal<CategoryDto[]>([]);
  showModal = signal(false);
  editingCategory = signal<CategoryDto | null>(null);

  formData: CreateCategoryDto = {
    name: '',
    slug: '',
    description: '',
    sortOrder: 1,
    isActive: true
  };

  constructor(
    private categoryService: CategoryService,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    this.loadCategories();
  }

  loadCategories() {
    this.categoryService.getTree().subscribe({
      next: (res) => this.categories.set(res),
      error: (err) => console.error(err)
    });
  }

  openCreateModal() {
    this.editingCategory.set(null);
    this.formData = { name: '', slug: '', description: '', sortOrder: this.categories().length + 1, isActive: true };
    this.showModal.set(true);
  }

  openEditModal(cat: CategoryDto) {
    this.editingCategory.set(cat);
    this.formData = {
      name: cat.name,
      slug: cat.slug,
      description: cat.description || '',
      sortOrder: cat.sortOrder,
      isActive: cat.isActive,
      parentId: cat.parentId
    };
    this.showModal.set(true);
  }

  closeModal() {
    this.showModal.set(false);
  }

  saveCategory() {
    if (!this.formData.name.trim()) return;

    if (this.editingCategory()) {
      const updateDto: UpdateCategoryDto = {
        ...this.formData,
        id: this.editingCategory()!.id
      };
      this.categoryService.update(updateDto).subscribe({
        next: () => {
          this.toastService.success('Başarılı', 'Kategori güncellendi.');
          this.closeModal();
          this.loadCategories();
        },
        error: () => this.toastService.error('Hata', 'Kategori güncellenemedi.')
      });
    } else {
      this.categoryService.create(this.formData).subscribe({
        next: () => {
          this.toastService.success('Başarılı', 'Yeni kategori oluşturuldu.');
          this.closeModal();
          this.loadCategories();
        },
        error: () => this.toastService.error('Hata', 'Kategori oluşturulamadı.')
      });
    }
  }

  deleteCategory(id: number, name: string) {
    if (!confirm(`"${name}" kategorisini silmek istediğinize emin misiniz?`)) return;

    this.categoryService.delete(id).subscribe({
      next: () => {
        this.toastService.success('Silindi', 'Kategori silindi.');
        this.loadCategories();
      },
      error: () => this.toastService.error('Hata', 'Kategori silinemedi.')
    });
  }
}
