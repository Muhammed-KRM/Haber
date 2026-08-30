import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ToastService } from '../../core/services/toast.service';

interface SettingDto {
  key: string;
  value: string;
  description: string;
}

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="space-y-6 animate-fade-in max-w-4xl">
      <!-- Başlık -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="text-2xl font-extrabold text-slate-900 tracking-tight">Site Ayarları & SEO</h1>
          <p class="text-xs text-slate-500 mt-1">Genel haber portalı konfigürasyonu, Google News ve SEO haritası.</p>
        </div>
      </div>

      <!-- GENEL AYARLAR KARTI -->
      <div class="premium-card p-6 space-y-4">
        <h3 class="text-sm font-bold text-slate-900 border-b border-slate-100 pb-2">Genel Portal Bilgileri</h3>

        <div class="space-y-3">
          @for (s of settings(); track s.key) {
            <div>
              <label class="block text-xs font-semibold text-slate-700 mb-1">{{ s.description }} ({{ s.key }})</label>
              <div class="flex gap-2">
                <input 
                  type="text" 
                  [(ngModel)]="s.value" 
                  class="flex-1 px-3 py-2 text-xs bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
                />
                <button 
                  (click)="saveSetting(s)"
                  class="px-3 py-2 bg-slate-800 hover:bg-slate-900 text-white text-xs font-semibold rounded-lg transition-colors"
                >
                  Kaydet
                </button>
              </div>
            </div>
          }
        </div>
      </div>

      <!-- SITEMAP & GOOGLE NEWS ENTEGRASYONU -->
      <div class="premium-card p-6 space-y-4">
        <h3 class="text-sm font-bold text-slate-900 border-b border-slate-100 pb-2">Arama Motoru & Google News Haritaları</h3>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div class="p-4 rounded-xl bg-slate-50 border border-slate-200 space-y-2">
            <div class="flex items-center gap-2">
              <span class="w-2.5 h-2.5 rounded-full bg-emerald-500"></span>
              <h4 class="text-xs font-bold text-slate-800">Standart XML Sitemap</h4>
            </div>
            <p class="text-[11px] text-slate-500">Tüm haber ve kategorilerin Google dizinine anlık sunulması.</p>
            <a 
              href="http://localhost:5000/sitemap.xml" 
              target="_blank" 
              class="inline-block text-xs font-semibold text-red-600 hover:underline"
            >
              /sitemap.xml Görüntüle &rarr;
            </a>
          </div>

          <div class="p-4 rounded-xl bg-slate-50 border border-slate-200 space-y-2">
            <div class="flex items-center gap-2">
              <span class="w-2.5 h-2.5 rounded-full bg-red-500 animate-pulse"></span>
              <h4 class="text-xs font-bold text-slate-800">Google News XML Sitemap</h4>
            </div>
            <p class="text-[11px] text-slate-500">Son 48 saatte yayınlanan haberlerin Google Haberler formatında sunulması.</p>
            <a 
              href="http://localhost:5000/news-sitemap.xml" 
              target="_blank" 
              class="inline-block text-xs font-semibold text-red-600 hover:underline"
            >
              /news-sitemap.xml Görüntüle &rarr;
            </a>
          </div>
        </div>
      </div>
    </div>
  `
})
export class SettingsComponent implements OnInit {
  settings = signal<SettingDto[]>([]);

  constructor(private http: HttpClient, private toastService: ToastService) {}

  ngOnInit() {
    this.loadSettings();
  }

  loadSettings() {
    this.http.get<SettingDto[]>('http://localhost:5000/api/admin/settings').subscribe({
      next: (res) => this.settings.set(res),
      error: (err) => console.error(err)
    });
  }

  saveSetting(setting: SettingDto) {
    this.http.put('http://localhost:5000/api/admin/settings', setting).subscribe({
      next: () => {
        this.toastService.success('Kaydedildi', `${setting.description} güncellendi.`);
      },
      error: () => this.toastService.error('Hata', 'Ayar kaydedilemedi.')
    });
  }
}
