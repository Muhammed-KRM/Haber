import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CategoryManagerComponent } from './components/category-manager/category-manager.component';
import { ProductManagerComponent } from './components/product-manager/product-manager.component';
import { AuditLogsComponent } from './components/audit-logs/audit-logs.component';
import { QrGeneratorComponent } from './components/qr-generator/qr-generator.component';

type AdminTab = 'categories' | 'products' | 'audit-logs' | 'qr-generator';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    CategoryManagerComponent,
    ProductManagerComponent,
    AuditLogsComponent,
    QrGeneratorComponent
  ],
  template: `
    <div class="min-h-screen bg-[#F7F1E3] flex flex-col text-[#3A2418]">
      <!-- Admin Üst Navigasyon Barı -->
      <header class="bg-[#3A2418] text-[#F7F1E3] px-6 py-4 shadow-md sticky top-0 z-40">
        <div class="max-w-7xl mx-auto flex items-center justify-between">
          <div class="flex items-center gap-3">
            <div class="w-9 h-9 rounded-xl bg-[#C65D3A] text-white flex items-center justify-center font-extrabold text-base shadow-sm">
              🍽️
            </div>
            <div>
              <h1 class="font-extrabold text-base sm:text-lg leading-none">Lezzet Durağı</h1>
              <span class="text-[11px] text-[#EFE7D5]/70 font-semibold">Restoran Yönetim Paneli</span>
            </div>
          </div>

          <a 
            href="/"
            class="px-4 py-2 bg-white/10 hover:bg-white/20 text-[#F7F1E3] rounded-xl text-xs font-bold transition-colors flex items-center gap-2 border border-white/10">
            <span>Müşteri Menüsünü Aç ↗</span>
          </a>
        </div>
      </header>

      <!-- Admin Sekmeleri (Tabs) -->
      <div class="bg-white border-b border-[#E3D7C1] px-6 py-2 sticky top-[68px] z-30 shadow-xs">
        <div class="max-w-7xl mx-auto flex gap-2 overflow-x-auto no-scrollbar">
          <button 
            (click)="activeTab.set('categories')"
            [class.bg-[#3A2418]]="activeTab() === 'categories'"
            [class.text-[#F7F1E3]]="activeTab() === 'categories'"
            [class.bg-[#F7F1E3]]="activeTab() !== 'categories'"
            [class.text-[#3A2418]]="activeTab() !== 'categories'"
            class="px-4 py-2 rounded-xl text-xs sm:text-sm font-bold transition-all cursor-pointer flex items-center gap-2">
            <span>📁 Kategoriler</span>
          </button>

          <button 
            (click)="activeTab.set('products')"
            [class.bg-[#3A2418]]="activeTab() === 'products'"
            [class.text-[#F7F1E3]]="activeTab() === 'products'"
            [class.bg-[#F7F1E3]]="activeTab() !== 'products'"
            [class.text-[#3A2418]]="activeTab() !== 'products'"
            class="px-4 py-2 rounded-xl text-xs sm:text-sm font-bold transition-all cursor-pointer flex items-center gap-2">
            <span>🍲 Ürünler & Fiyatlar</span>
          </button>

          <button 
            (click)="activeTab.set('qr-generator')"
            [class.bg-[#3A2418]]="activeTab() === 'qr-generator'"
            [class.text-[#F7F1E3]]="activeTab() === 'qr-generator'"
            [class.bg-[#F7F1E3]]="activeTab() !== 'qr-generator'"
            [class.text-[#3A2418]]="activeTab() !== 'qr-generator'"
            class="px-4 py-2 rounded-xl text-xs sm:text-sm font-bold transition-all cursor-pointer flex items-center gap-2">
            <span>📱 Masa QR Üretici</span>
          </button>

          <button 
            (click)="activeTab.set('audit-logs')"
            [class.bg-[#3A2418]]="activeTab() === 'audit-logs'"
            [class.text-[#F7F1E3]]="activeTab() === 'audit-logs'"
            [class.bg-[#F7F1E3]]="activeTab() !== 'audit-logs'"
            [class.text-[#3A2418]]="activeTab() !== 'audit-logs'"
            class="px-4 py-2 rounded-xl text-xs sm:text-sm font-bold transition-all cursor-pointer flex items-center gap-2">
            <span>📜 Değişiklik Geçmişi (Audit Logs)</span>
          </button>
        </div>
      </div>

      <!-- Ana İçerik -->
      <main class="flex-1 max-w-7xl w-full mx-auto px-4 sm:px-6 py-8">
        @switch (activeTab()) {
          @case ('categories') {
            <app-category-manager />
          }
          @case ('products') {
            <app-product-manager />
          }
          @case ('qr-generator') {
            <app-qr-generator />
          }
          @case ('audit-logs') {
            <app-audit-logs />
          }
        }
      </main>
    </div>
  `
})
export class AdminDashboardComponent {
  activeTab = signal<AdminTab>('categories');
}
