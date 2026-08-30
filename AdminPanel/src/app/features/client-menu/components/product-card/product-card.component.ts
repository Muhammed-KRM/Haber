import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { Product } from '../../../../core/models/menu.model';
import { ImageUrlPipe } from '../../../../shared/pipes/image-url.pipe';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [CommonModule, ImageUrlPipe, DecimalPipe],
  template: `
    <div class="bg-white rounded-2xl border border-[#E3D7C1] overflow-hidden shadow-sm hover:shadow-md transition-all duration-200 flex flex-col justify-between group hover:border-[#C65D3A]/40">
      
      <!-- Ürün Görseli -->
      <div 
        (click)="detailClick.emit(product)"
        class="relative w-full aspect-[4/3] bg-[#EFE7D5] overflow-hidden cursor-pointer">
        @if (product.imageUrl) {
          <img 
            [src]="product.imageUrl | imageUrl" 
            [alt]="product.name" 
            loading="eager"
            (error)="onImgError($event)"
            class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300" />
        } @else {
          <div class="w-full h-full flex items-center justify-center text-[#725B4D]">
            <svg class="w-10 h-10 opacity-30" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M12 6v6m0 0v6m0-6h6m-6 0H6"></path>
            </svg>
          </div>
        }

        <!-- Kalori Rozeti -->
        @if (product.calories) {
          <span class="absolute top-2.5 right-2.5 bg-[#3A2418]/85 backdrop-blur-md text-[#F7F1E3] text-[11px] font-bold px-2.5 py-0.5 rounded-full shadow-sm">
            {{ product.calories }} kcal
          </span>
        }
      </div>

      <!-- Ürün Bilgileri -->
      <div class="p-4 flex-1 flex flex-col justify-between">
        <div>
          <h3 
            (click)="detailClick.emit(product)"
            class="font-bold text-[#3A2418] text-base md:text-lg line-clamp-1 mb-1 group-hover:text-[#C65D3A] transition-colors cursor-pointer">
            {{ product.name }}
          </h3>
          <p class="text-xs md:text-sm text-[#725B4D] line-clamp-2 mb-3 leading-relaxed">
            {{ product.description }}
          </p>
        </div>

        <!-- Alt Bölüm: Fiyat ve Detay Butonu -->
        <div class="pt-3 border-t border-[#EFE7D5] flex items-center justify-between mt-auto">
          <div class="flex flex-col">
            <span class="text-[10px] uppercase font-bold text-[#725B4D] tracking-wider">Fiyat</span>
            <span class="text-lg md:text-xl font-extrabold text-[#C65D3A]">
              {{ product.price | number:'1.0-2' }} ₺
            </span>
          </div>

          <button 
            (click)="detailClick.emit(product)"
            class="px-3.5 py-2 rounded-xl bg-[#F7F1E3] text-[#3A2418] hover:bg-[#3A2418] hover:text-[#F7F1E3] font-bold text-xs transition-all duration-200 border border-[#E3D7C1] flex items-center gap-1.5 shadow-sm active:scale-95 cursor-pointer">
            <span>Detay</span>
            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"></path>
            </svg>
          </button>
        </div>
      </div>

    </div>
  `
})
export class ProductCardComponent {
  @Input({ required: true }) product!: Product;
  @Output() detailClick = new EventEmitter<Product>();

  onImgError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.style.display = 'none';
  }
}
