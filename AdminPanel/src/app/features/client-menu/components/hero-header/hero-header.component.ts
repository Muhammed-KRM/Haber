import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MenuService } from '../../../../core/services/menu.service';

@Component({
  selector: 'app-hero-header',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <header class="bg-gradient-to-b from-[#3A2418] to-[#2A180E] text-[#F7F1E3] pt-8 pb-10 px-4 shadow-lg relative overflow-hidden">
      <!-- Arka Plan Dekoratif Işık Efekti -->
      <div class="absolute -top-24 -right-24 w-64 h-64 bg-[#C65D3A]/20 rounded-full blur-3xl pointer-events-none"></div>
      <div class="absolute -bottom-24 -left-24 w-64 h-64 bg-[#C65D3A]/15 rounded-full blur-3xl pointer-events-none"></div>

      <div class="max-w-4xl mx-auto relative z-10 text-center">
        <!-- Üst Rozetler: Servis Durumu ve URL'den Okunan Masa Rozeti -->
        <div class="flex items-center justify-center gap-2.5 mb-3 flex-wrap">
          <div class="inline-flex items-center gap-2 px-3.5 py-1 rounded-full bg-white/10 backdrop-blur-md border border-white/10 text-xs font-semibold tracking-wider uppercase text-[#EFE7D5]">
            <span class="w-2 h-2 rounded-full bg-emerald-400 animate-pulse"></span>
            <span>Açık & Serviste</span>
          </div>

          <!-- Masa / Genel Menü Rozeti (Yalnızca URL'den Anlık Okunur) -->
          <div class="inline-flex items-center gap-1.5 px-3.5 py-1 rounded-full bg-[#C65D3A]/25 backdrop-blur-md border border-[#C65D3A]/40 text-xs font-bold text-[#F7F1E3] shadow-sm">
            <span>📍</span>
            <span>{{ tableName() }}</span>
          </div>
        </div>

        <!-- Restoran Adı -->
        <h1 class="text-3xl sm:text-4xl md:text-5xl font-extrabold tracking-tight mb-2 text-[#F7F1E3]">
          Lezzet Durağı
        </h1>

        <p class="text-sm md:text-base text-[#EFE7D5]/80 max-w-md mx-auto mb-6 font-normal">
          Usta şeflerimizin elinden çıkan seçkin lezzetler, taze malzemeler ve sıcak sunumlar.
        </p>

        <!-- Canlı Arama Çubuğu -->
        <div class="max-w-md mx-auto relative">
          <div class="relative flex items-center">
            <svg class="w-5 h-5 absolute left-4 text-[#725B4D]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path>
            </svg>
            <input 
              type="text"
              [(ngModel)]="searchQuery"
              (ngModelChange)="onSearchChange($event)"
              placeholder="Yemek, içecek veya tatlı ara..."
              class="w-full bg-[#F7F1E3] text-[#3A2418] placeholder-[#725B4D] pl-11 pr-10 py-3.5 rounded-2xl text-sm font-medium border-2 border-transparent focus:border-[#C65D3A] focus:outline-none shadow-md transition-all duration-200"
            />
            @if (searchQuery) {
              <button 
                (click)="clearSearch()"
                class="absolute right-3.5 text-[#725B4D] hover:text-[#3A2418] p-1 rounded-full bg-[#EFE7D5] transition-colors">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                </svg>
              </button>
            }
          </div>
        </div>
      </div>
    </header>
  `
})
export class HeroHeaderComponent implements OnInit {
  menuService = inject(MenuService);
  private route = inject(ActivatedRoute);

  searchQuery = '';
  tableName = signal<string>('Genel Menü');

  ngOnInit(): void {
    // Sadece anlık URL query parametresinden (?table=...) okur, hiçbir kalıcı hafızaya yazmaz.
    this.route.queryParams.subscribe(params => {
      const table = params['table']?.trim();
      this.tableName.set(table && table.length > 0 ? table : 'Genel Menü');
    });
  }

  onSearchChange(value: string): void {
    this.menuService.searchQuery.set(value);
  }

  clearSearch(): void {
    this.searchQuery = '';
    this.menuService.searchQuery.set('');
  }
}
