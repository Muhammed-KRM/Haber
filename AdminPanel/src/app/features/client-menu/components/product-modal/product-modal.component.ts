import { Component, inject } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { MenuService } from '../../../../core/services/menu.service';
import { ImageUrlPipe } from '../../../../shared/pipes/image-url.pipe';

@Component({
  selector: 'app-product-modal',
  standalone: true,
  imports: [CommonModule, ImageUrlPipe, DecimalPipe],
  template: `
    @if (menuService.selectedProductForModal(); as product) {
      <div 
        (click)="menuService.closeProductDetail()"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-fade-in">
        
        <div 
          (click)="$event.stopPropagation()"
          class="bg-white rounded-3xl max-w-lg w-full max-h-[90vh] overflow-y-auto shadow-2xl border border-[#E3D7C1] relative animate-scale-up flex flex-col">
          
          <!-- Kapat Butonu -->
          <button 
            (click)="menuService.closeProductDetail()"
            class="absolute top-4 right-4 z-20 w-9 h-9 rounded-full bg-black/50 hover:bg-black/80 text-white flex items-center justify-center transition-colors cursor-pointer shadow-md">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
            </svg>
          </button>

          <!-- Büyük Ürün Görseli -->
          <div class="w-full h-64 sm:h-72 bg-[#EFE7D5] relative flex-shrink-0">
            @if (product.imageUrl) {
              <img [src]="product.imageUrl | imageUrl" [alt]="product.name" class="w-full h-full object-cover" />
            } @else {
              <div class="w-full h-full flex items-center justify-center text-[#725B4D]">
                <svg class="w-16 h-16 opacity-30" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M12 6v6m0 0v6m0-6h6m-6 0H6"></path>
                </svg>
              </div>
            }
          </div>

          <!-- Detay Gövdesi -->
          <div class="p-6 flex-1 flex flex-col justify-between">
            <div>
              <div class="flex items-start justify-between gap-4 mb-3">
                <h2 class="text-2xl font-extrabold text-[#3A2418] tracking-tight">{{ product.name }}</h2>
                <span class="text-2xl font-black text-[#C65D3A] whitespace-nowrap">
                  {{ product.price | number:'1.0-2' }} ₺
                </span>
              </div>

              <p class="text-[#725B4D] text-sm sm:text-base leading-relaxed mb-6">
                {{ product.description }}
              </p>

              <!-- Rozet / Ek Bilgi Kutuları -->
              <div class="grid grid-cols-2 gap-3 mb-6">
                @if (product.calories) {
                  <div class="bg-[#F7F1E3] p-3 rounded-2xl border border-[#E3D7C1] flex items-center gap-3">
                    <div class="w-9 h-9 rounded-xl bg-[#C65D3A]/10 text-[#C65D3A] flex items-center justify-center font-bold">
                      🔥
                    </div>
                    <div>
                      <div class="text-[11px] text-[#725B4D] font-semibold uppercase tracking-wider">Enerji</div>
                      <div class="text-sm font-extrabold text-[#3A2418]">{{ product.calories }} kcal</div>
                    </div>
                  </div>
                }
                @if (product.preparationTime) {
                  <div class="bg-[#F7F1E3] p-3 rounded-2xl border border-[#E3D7C1] flex items-center gap-3">
                    <div class="w-9 h-9 rounded-xl bg-[#C65D3A]/10 text-[#C65D3A] flex items-center justify-center font-bold">
                      ⏱️
                    </div>
                    <div>
                      <div class="text-[11px] text-[#725B4D] font-semibold uppercase tracking-wider">Hazırlık</div>
                      <div class="text-sm font-extrabold text-[#3A2418]">{{ product.preparationTime }}</div>
                    </div>
                  </div>
                }
              </div>
            </div>

            <!-- Kapat / Menüye Dön Butonu -->
            <button 
              (click)="menuService.closeProductDetail()"
              class="w-full py-3.5 rounded-2xl bg-[#3A2418] hover:bg-[#2A180E] text-[#F7F1E3] font-bold text-sm transition-all duration-200 shadow-md active:scale-98 cursor-pointer">
              Menüye Dön
            </button>
          </div>

        </div>
      </div>
    }
  `
})
export class ProductModalComponent {
  menuService = inject(MenuService);
}
