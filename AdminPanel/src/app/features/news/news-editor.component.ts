import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NewsService } from '../../core/services/news.service';
import { CategoryService } from '../../core/services/category.service';
import { MediaService } from '../../core/services/media.service';
import { ToastService } from '../../core/services/toast.service';
import { NewsCreateDto, NewsStatus, NewsType, NewsUpdateDto } from '../../core/models/news.model';
import { CategoryDto } from '../../core/models/category.model';

@Component({
  selector: 'app-news-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="space-y-6 animate-fade-in max-w-6xl mx-auto pb-12">
      <!-- Üst Bar: Başlık, Auto-Save Durumu ve Kaydet Butonları -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 sticky top-0 bg-slate-50/90 backdrop-blur-md py-3 z-10 border-b border-slate-200">
        <div>
          <div class="flex items-center gap-2">
            <a routerLink="/admin/news" class="text-slate-400 hover:text-slate-600 transition-colors">
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18"/></svg>
            </a>
            <h1 class="text-xl font-bold text-slate-900">
              {{ isEditMode() ? 'Haberi Düzenle' : 'Yeni Haber Yaz' }}
            </h1>
          </div>
          <p class="text-xs text-slate-400 mt-0.5 ml-7">
            {{ isEditMode() ? 'Mevcut haber içerik ve ayarlarını güncelleyin.' : 'SEO uyumlu, kaliteli ve zengin içerikli haber oluşturun.' }}
          </p>
        </div>

        <div class="flex items-center gap-3">
          <!-- Otomatik Taslak Kayıt Göstergesi -->
          @if (isEditMode()) {
            <div class="flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-slate-100 border border-slate-200 text-[11px] text-slate-500">
              <span class="w-2 h-2 rounded-full" [ngClass]="isAutoSaving() ? 'bg-amber-500 animate-ping' : 'bg-emerald-500'"></span>
              <span>{{ autoSaveMessage() }}</span>
            </div>
          }

          <!-- Taslak Kaydet -->
          <button 
            type="button"
            (click)="saveNews(0)" 
            [disabled]="isSaving()"
            class="px-3.5 py-2 bg-white border border-slate-200 hover:bg-slate-50 text-slate-700 text-xs font-semibold rounded-lg shadow-xs transition-all disabled:opacity-50"
          >
            Taslak Olarak Kaydet
          </button>

          <!-- %10 VURGU: Yayına Al / Güncelle Butonu (Kürsü TV Kırmızı) -->
          <button 
            type="button"
            (click)="saveNews(2)" 
            [disabled]="isSaving() || !title"
            class="px-4 py-2 bg-gradient-to-r from-red-600 to-rose-600 hover:from-red-700 hover:to-rose-700 text-white text-xs font-semibold rounded-lg shadow-sm shadow-red-500/20 hover:shadow-md transition-all transform active:scale-95 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
          >
            @if (isSaving()) {
              <svg class="animate-spin w-3.5 h-3.5 text-white" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"></path></svg>
              <span>Kaydediliyor...</span>
            } @else {
              <span>{{ isEditMode() ? 'Güncellemeleri Kaydet' : 'Haberi Yayınla' }}</span>
            }
          </button>
        </div>
      </div>

      <!-- ANA FORM DÜZENİ (Sol: Editör & İçerik, Sağ: Ayarlar & Medya) -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- SOL KOLON (2/3): Başlık, Spot ve Zengin Metin Editörü -->
        <div class="lg:col-span-2 space-y-5">
          <!-- 1. Başlık ve Slug -->
          <div class="premium-card p-5 space-y-3">
            <div>
              <label class="block text-xs font-bold text-slate-800 mb-1.5">
                Haber Başlığı <span class="text-red-500">*</span>
              </label>
              <input 
                type="text" 
                [(ngModel)]="title" 
                (ngModelChange)="onTitleChange()"
                placeholder="Örn: 1 milyar lira zarar eden Tarım Kredi Kooperatifi marketleri kapattı" 
                class="w-full px-3.5 py-2.5 text-sm font-semibold text-slate-900 bg-slate-50/50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 transition-all"
              />
            </div>

            <!-- Slug Önizleme -->
            <div class="flex items-center gap-1.5 text-[11px] text-slate-500 bg-slate-50 px-3 py-1.5 rounded-lg border border-slate-100 font-mono overflow-hidden">
              <span class="text-slate-400 shrink-0">https://kursutv.com/haber/</span>
              <input 
                type="text" 
                [(ngModel)]="slug" 
                class="bg-transparent border-none p-0 text-slate-700 font-semibold focus:outline-none w-full"
                placeholder="otomatik-olusturulan-slug"
              />
            </div>

            <!-- 2. Spot / Özet -->
            <div class="pt-2">
              <label class="block text-xs font-bold text-slate-800 mb-1.5">
                Haber Spotu (Özet)
              </label>
              <textarea 
                [(ngModel)]="spot" 
                rows="2"
                placeholder="Anasayfada ve sosyal medya paylaşımlarında görünecek vurucu özet..."
                class="w-full px-3.5 py-2 text-xs text-slate-800 bg-slate-50/50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 transition-all placeholder:text-slate-400 resize-none"
              ></textarea>
            </div>
          </div>

          <!-- 3. ZENGİN METİN EDİTÖRÜ (TipTap Uyumlu Toolbar) -->
          <div class="premium-card p-5 space-y-3">
            <div class="flex items-center justify-between border-b border-slate-200 pb-3">
              <label class="text-xs font-bold text-slate-800">
                Haber Metni & İçerik <span class="text-red-500">*</span>
              </label>

              <!-- Hızlı Biçimlendirme Araçları -->
              <div class="flex items-center gap-1 bg-slate-100 p-1 rounded-lg">
                <button type="button" (click)="insertTag('<b>', '</b>')" class="px-2 py-0.5 text-xs font-bold hover:bg-white rounded transition-colors" title="Kalın">B</button>
                <button type="button" (click)="insertTag('<i>', '</i>')" class="px-2 py-0.5 text-xs italic hover:bg-white rounded transition-colors" title="İtalik">I</button>
                <button type="button" (click)="insertTag('<h3>', '</h3>')" class="px-2 py-0.5 text-xs font-semibold hover:bg-white rounded transition-colors" title="Alt Başlık">H3</button>
                <button type="button" (click)="insertTag('<blockquote>', '</blockquote>')" class="px-2 py-0.5 text-xs hover:bg-white rounded transition-colors" title="Alıntı">""</button>
                <button type="button" (click)="insertTag('<p>', '</p>')" class="px-2 py-0.5 text-xs hover:bg-white rounded transition-colors" title="Paragraf">P</button>
              </div>
            </div>

            <!-- Editör Metin Alanı -->
            <textarea 
              #editorTextArea
              [(ngModel)]="content" 
              rows="16"
              placeholder="Haberin detaylı içeriğini buraya yazın veya yapıştırın. HTML formatında zengin biçimlendirmeyi destekler..."
              class="w-full p-4 text-sm text-slate-800 bg-white border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 transition-all font-sans leading-relaxed custom-scrollbar"
            ></textarea>
          </div>

          <!-- 4. SEO & Meta Ayarları -->
          <div class="premium-card p-5 space-y-3">
            <h3 class="text-xs font-bold text-slate-800">SEO & Arama Motoru Optimizasyonu</h3>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div>
                <label class="block text-[11px] font-semibold text-slate-600 mb-1">Meta Başlık (Title)</label>
                <input 
                  type="text" 
                  [(ngModel)]="metaTitle" 
                  placeholder="Boşsa haber başlığı kullanılır"
                  class="w-full px-3 py-1.5 text-xs bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
                />
              </div>
              <div>
                <label class="block text-[11px] font-semibold text-slate-600 mb-1">Meta Açıklama (Description)</label>
                <input 
                  type="text" 
                  [(ngModel)]="metaDescription" 
                  placeholder="Boşsa haber spotu kullanılır"
                  class="w-full px-3 py-1.5 text-xs bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
                />
              </div>
            </div>
          </div>
        </div>

        <!-- SAĞ KOLON (1/3): Yayın Ayarları, Kapak Görseli, Kategori & Etiketler -->
        <div class="space-y-5">
          <!-- 1. KAPAK GÖRSELİ (WebP & Alt Text) -->
          <div class="premium-card p-5 space-y-3">
            <div class="flex items-center justify-between">
              <label class="text-xs font-bold text-slate-800">Kapak Görseli</label>
              <span class="text-[10px] text-slate-400 font-medium">1200x675 (16:9)</span>
            </div>

            <!-- Görsel Önizleme / Yükleme Alanı -->
            <div class="relative group border-2 border-dashed border-slate-200 hover:border-red-400 rounded-xl p-2 transition-colors bg-slate-50 flex flex-col items-center justify-center min-h-[160px] text-center overflow-hidden">
              @if (coverImageUrl) {
                <img [src]="coverImageUrl" class="w-full h-36 object-cover rounded-lg" />
                <button 
                  type="button" 
                  (click)="coverImageUrl = ''"
                  class="absolute top-3 right-3 bg-red-600 text-white p-1 rounded-full shadow hover:bg-red-700 transition-colors"
                >
                  <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              } @else {
                <div class="space-y-1 py-4">
                  <svg class="w-8 h-8 text-slate-400 mx-auto" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"/></svg>
                  <p class="text-xs font-medium text-slate-600">Görsel Yüklemek İçin Tıklayın</p>
                  <p class="text-[10px] text-slate-400">PNG, JPG, WebP (Max 50MB)</p>
                </div>
                <input 
                  type="file" 
                  (change)="onFileSelected($event)" 
                  accept="image/*"
                  class="absolute inset-0 opacity-0 cursor-pointer"
                />
              }
            </div>

            <!-- Görsel Alt Metni (SEO & Erişilebilirlik) -->
            <div>
              <label class="block text-[11px] font-semibold text-slate-600 mb-1">Görsel Alt Yazısı (Alt Text)</label>
              <input 
                type="text" 
                [(ngModel)]="coverImageAlt" 
                placeholder="Görseli tarif eden kısa metin..."
                class="w-full px-3 py-1.5 text-xs bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
              />
            </div>
          </div>

          <!-- 2. KATEGORİ & ETİKET SEÇİMİ -->
          <div class="premium-card p-5 space-y-4">
            <!-- Kategoriler -->
            <div>
              <label class="block text-xs font-bold text-slate-800 mb-2">
                Kategoriler <span class="text-red-500">*</span>
              </label>
              <div class="space-y-1.5 max-h-40 overflow-y-auto custom-scrollbar pr-1">
                @for (cat of categories(); track cat.id) {
                  <label class="flex items-center gap-2 p-1.5 rounded-lg hover:bg-slate-50 transition-colors cursor-pointer text-xs">
                    <input 
                      type="checkbox" 
                      [checked]="selectedCategoryIds.includes(cat.id)"
                      (change)="toggleCategory(cat.id)"
                      class="rounded border-slate-300 text-red-600 focus:ring-red-500"
                    />
                    <span class="font-medium text-slate-700">{{ cat.name }}</span>
                  </label>
                }
              </div>
            </div>

            <!-- Etiketler (Tag Tokenizer) -->
            <div>
              <label class="block text-xs font-bold text-slate-800 mb-1.5">Etiketler</label>
              <div class="flex flex-wrap gap-1.5 mb-2">
                @for (tag of tags; track tag) {
                  <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-slate-100 text-slate-700 text-[11px] font-medium rounded-md">
                    #{{ tag }}
                    <button type="button" (click)="removeTag(tag)" class="text-slate-400 hover:text-red-600">&times;</button>
                  </span>
                }
              </div>
              <input 
                type="text" 
                [(ngModel)]="newTagInput" 
                (keydown.enter)="addTag($event)"
                placeholder="Etiket yazıp Enter'a basın..."
                class="w-full px-3 py-1.5 text-xs bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
              />
            </div>
          </div>

          <!-- 3. YAYIN & MANŞET SEÇENEKLERİ -->
          <div class="premium-card p-5 space-y-4">
            <h3 class="text-xs font-bold text-slate-800">Yayın Ayarları</h3>

            <!-- Son Dakika Bandı -->
            <label class="flex items-center justify-between p-2.5 rounded-xl bg-amber-50/60 border border-amber-200/60 cursor-pointer">
              <div class="flex items-center gap-2">
                <span class="w-2 h-2 rounded-full bg-amber-500 animate-ping"></span>
                <span class="text-xs font-bold text-amber-950">Son Dakika Bandı</span>
              </div>
              <input 
                type="checkbox" 
                [(ngModel)]="isBreaking" 
                class="rounded border-amber-300 text-amber-600 focus:ring-amber-500"
              />
            </label>

            <!-- Manşet Sıralaması -->
            <div>
              <label class="block text-[11px] font-semibold text-slate-600 mb-1">Manşet Sırası (1-7)</label>
              <select 
                [(ngModel)]="headlineOrder" 
                class="w-full px-3 py-1.5 text-xs bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 text-slate-700"
              >
                <option [ngValue]="0">Manşette Gösterme (Normal Liste)</option>
                <option [ngValue]="1">Manşet #1 (Ana Dev Manşet)</option>
                <option [ngValue]="2">Manşet #2</option>
                <option [ngValue]="3">Manşet #3</option>
                <option [ngValue]="4">Manşet #4</option>
                <option [ngValue]="5">Manşet #5</option>
                <option [ngValue]="6">Manşet #6</option>
                <option [ngValue]="7">Manşet #7</option>
              </select>
            </div>

            <!-- Haber Format Türü -->
            <div>
              <label class="block text-[11px] font-semibold text-slate-600 mb-1">İçerik Türü</label>
              <select 
                [(ngModel)]="newsType" 
                class="w-full px-3 py-1.5 text-xs bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 text-slate-700"
              >
                <option [ngValue]="0">Standart Makale</option>
                <option [ngValue]="1">Foto Galeri</option>
                <option [ngValue]="2">Video Haber</option>
                <option [ngValue]="4">Köşe Yazısı</option>
              </select>
            </div>

            <!-- Zamanlanmış Yayın -->
            <div>
              <label class="block text-[11px] font-semibold text-slate-600 mb-1">İleri Tarihli Yayınlama</label>
              <input 
                type="datetime-local" 
                [(ngModel)]="scheduledPublishAt" 
                class="w-full px-3 py-1.5 text-xs bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 text-slate-700"
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class NewsEditorComponent implements OnInit, OnDestroy {
  newsId: string | null = null;
  isEditMode = signal(false);
  isSaving = signal(false);
  isAutoSaving = signal(false);
  autoSaveMessage = signal('Kaydedildi');

  categories = signal<CategoryDto[]>([]);

  // Form Değerleri
  title = '';
  slug = '';
  spot = '';
  content = '';
  coverImageUrl = '';
  coverImageAlt = '';
  metaTitle = '';
  metaDescription = '';
  isBreaking = false;
  headlineOrder = 0;
  newsType: NewsType = NewsType.Article;
  scheduledPublishAt = '';
  selectedCategoryIds: number[] = [];
  tags: string[] = [];
  newTagInput = '';

  private autoSaveInterval: any;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private newsService: NewsService,
    private categoryService: CategoryService,
    private mediaService: MediaService,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    this.loadCategories();
    this.newsId = this.route.snapshot.paramMap.get('id');

    if (this.newsId) {
      this.isEditMode.set(true);
      this.loadNewsForEdit(this.newsId);
      this.startAutoSave();
    }
  }

  ngOnDestroy() {
    if (this.autoSaveInterval) {
      clearInterval(this.autoSaveInterval);
    }
  }

  loadCategories() {
    this.categoryService.getFlat().subscribe({
      next: (res) => this.categories.set(res),
      error: (err) => console.error(err)
    });
  }

  loadNewsForEdit(id: string) {
    this.newsService.getByIdForEdit(id).subscribe({
      next: (news) => {
        this.title = news.title;
        this.slug = news.slug;
        this.spot = news.spot || '';
        this.content = news.content;
        this.coverImageUrl = news.coverImageUrl || '';
        this.coverImageAlt = news.coverImageAlt || '';
        this.metaTitle = news.metaTitle || '';
        this.metaDescription = news.metaDescription || '';
        this.isBreaking = news.isBreaking;
        this.headlineOrder = news.headlineOrder;
        this.newsType = news.type;
        this.selectedCategoryIds = news.categories.map(c => c.id);
        this.tags = news.tags.map(t => t.name);
      },
      error: (err) => {
        this.toastService.error('Hata', 'Haber yüklenemedi.');
        this.router.navigate(['/admin/news']);
      }
    });
  }

  onTitleChange() {
    if (!this.isEditMode() || !this.slug) {
      this.slug = this.generateSlug(this.title);
    }
  }

  generateSlug(text: string): string {
    return text
      .toLowerCase()
      .trim()
      .replace(/ğ/g, 'g')
      .replace(/ü/g, 'u')
      .replace(/ş/g, 's')
      .replace(/ı/g, 'i')
      .replace(/ö/g, 'o')
      .replace(/ç/g, 'c')
      .replace(/[^a-z0-9\s-]/g, '')
      .replace(/[\s-]+/g, '-')
      .replace(/^-+|-+$/g, '');
  }

  toggleCategory(id: number) {
    const idx = this.selectedCategoryIds.indexOf(id);
    if (idx > -1) {
      this.selectedCategoryIds.splice(idx, 1);
    } else {
      this.selectedCategoryIds.push(id);
    }
  }

  addTag(event: Event) {
    event.preventDefault();
    const tag = this.newTagInput.trim().replace(/^#/, '');
    if (tag && !this.tags.includes(tag)) {
      this.tags.push(tag);
      this.newTagInput = '';
    }
  }

  removeTag(tag: string) {
    this.tags = this.tags.filter(t => t !== tag);
  }

  insertTag(openTag: string, closeTag: string) {
    this.content += `${openTag}${closeTag}`;
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (!file) return;

    this.mediaService.upload(file, this.title || file.name).subscribe({
      next: (media) => {
        this.coverImageUrl = media.originalUrl;
        if (!this.coverImageAlt) this.coverImageAlt = this.title;
        this.toastService.success('Görsel Yüklendi', 'Kapak fotoğrafı hazır.');
      },
      error: (err) => {
        this.toastService.error('Hata', 'Görsel yüklenemedi.');
      }
    });
  }

  saveNews(targetStatus: NewsStatus) {
    if (!this.title.trim()) {
      this.toastService.warning('Uyarı', 'Haber başlığı zorunludur.');
      return;
    }
    if (this.selectedCategoryIds.length === 0) {
      this.toastService.warning('Uyarı', 'Lütfen en az bir kategori seçin.');
      return;
    }

    this.isSaving.set(true);

    const dto: NewsCreateDto = {
      title: this.title.trim(),
      slug: this.slug || undefined,
      spot: this.spot.trim() || undefined,
      content: this.content,
      coverImageUrl: this.coverImageUrl || undefined,
      coverImageAlt: this.coverImageAlt || undefined,
      metaTitle: this.metaTitle || undefined,
      metaDescription: this.metaDescription || undefined,
      status: targetStatus,
      type: this.newsType,
      isBreaking: this.isBreaking,
      headlineOrder: this.headlineOrder,
      scheduledPublishAt: this.scheduledPublishAt || undefined,
      categoryIds: this.selectedCategoryIds,
      tagNames: this.tags
    };

    if (this.isEditMode() && this.newsId) {
      const updateDto: NewsUpdateDto = { ...dto, id: this.newsId };
      this.newsService.updateNews(updateDto).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.toastService.success('Başarılı', 'Haber güncellendi.');
          this.router.navigate(['/admin/news']);
        },
        error: (err) => {
          this.isSaving.set(false);
          this.toastService.error('Hata', 'Haber güncellenemedi.');
        }
      });
    } else {
      this.newsService.createNews(dto).subscribe({
        next: (res) => {
          this.isSaving.set(false);
          this.toastService.success('Başarılı', 'Haber başarıyla kaydedildi.');
          this.router.navigate(['/admin/news']);
        },
        error: (err) => {
          this.isSaving.set(false);
          this.toastService.error('Hata', 'Haber kaydedilemedi.');
        }
      });
    }
  }

  startAutoSave() {
    this.autoSaveInterval = setInterval(() => {
      if (this.isEditMode() && this.newsId && this.title.trim()) {
        this.isAutoSaving.set(true);
        const dto: NewsUpdateDto = {
          id: this.newsId,
          title: this.title.trim(),
          slug: this.slug,
          spot: this.spot.trim(),
          content: this.content,
          coverImageUrl: this.coverImageUrl,
          coverImageAlt: this.coverImageAlt,
          metaTitle: this.metaTitle,
          metaDescription: this.metaDescription,
          status: NewsStatus.Draft,
          type: this.newsType,
          isBreaking: this.isBreaking,
          headlineOrder: this.headlineOrder,
          categoryIds: this.selectedCategoryIds,
          tagNames: this.tags
        };

        this.newsService.autoSaveDraft(dto).subscribe({
          next: () => {
            this.isAutoSaving.set(false);
            this.autoSaveMessage.set(`Otomatik kaydedildi (${new Date().toLocaleTimeString()})`);
          },
          error: () => this.isAutoSaving.set(false)
        });
      }
    }, 15000); // 15 saniyede bir auto-save
  }
}
