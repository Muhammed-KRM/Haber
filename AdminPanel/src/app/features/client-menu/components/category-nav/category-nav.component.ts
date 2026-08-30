import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MenuService } from '../../../../core/services/menu.service';
import { ImageUrlPipe } from '../../../../shared/pipes/image-url.pipe';

@Component({
  selector: 'app-category-nav',
  standalone: true,
  imports: [CommonModule, ImageUrlPipe],
  template: `
    <nav class="sticky top-0 z-30 bg-[#F7F1E3]/95 backdrop-blur-md border-b border-[#E3D7C1] py-2.5 shadow-sm transition-all relative">
      <!-- Sağ Kenar Taşma / Devamı Var Gölge İndikatörü -->
      <div class="pointer-events-none absolute right-0 top-0 bottom-0 w-8 bg-gradient-to-l from-[#F7F1E3] to-transparent z-10"></div>

      <div class="max-w-6xl mx-auto px-4 flex items-center justify-start gap-2.5 overflow-x-auto category-scrollbar scroll-smooth pb-1.5">
        @for (cat of menuService.filteredCategories(); track cat.id) {
          <button 
            (click)="scrollToCategory(cat.id)"
            [class.bg-[#3A2418]]="menuService.activeCategoryId() === cat.id"
            [class.text-[#F7F1E3]]="menuService.activeCategoryId() === cat.id"
            [class.shadow-md]="menuService.activeCategoryId() === cat.id"
            [class.border-[#3A2418]]="menuService.activeCategoryId() === cat.id"
            [class.bg-white]="menuService.activeCategoryId() !== cat.id"
            [class.text-[#3A2418]]="menuService.activeCategoryId() !== cat.id"
            [class.border-[#E3D7C1]]="menuService.activeCategoryId() !== cat.id"
            class="flex items-center gap-2 px-3.5 py-1.5 sm:px-4 sm:py-2 rounded-full font-bold text-xs sm:text-sm whitespace-nowrap border transition-all duration-200 hover:border-[#3A2418] active:scale-95 cursor-pointer shrink-0">
            
            @if (cat.imageUrl) {
              <img [src]="cat.imageUrl | imageUrl" alt="" class="w-5 h-5 rounded-full object-cover border border-white/20 shrink-0" />
            }
            <span class="whitespace-nowrap">{{ cat.name }}</span>
            <span 
              [class.bg-[#C65D3A]]="menuService.activeCategoryId() === cat.id"
              [class.text-white]="menuService.activeCategoryId() === cat.id"
              [class.bg-[#C65D3A]/15]="menuService.activeCategoryId() !== cat.id"
              [class.text-[#C65D3A]]="menuService.activeCategoryId() !== cat.id"
              class="min-w-5 h-5 px-1.5 inline-flex items-center justify-center rounded-full text-[11px] font-extrabold leading-none shrink-0">
              {{ cat.products ? cat.products.length : 0 }}
            </span>
          </button>
        }
      </div>
    </nav>
  `
})
export class CategoryNavComponent {
  menuService = inject(MenuService);

  scrollToCategory(categoryId: string): void {
    this.menuService.setActiveCategory(categoryId);
    const element = document.getElementById(`category-${categoryId}`);
    if (element) {
      const yOffset = -75; // Sticky nav yüksekliği ofseti
      const y = element.getBoundingClientRect().top + window.pageYOffset + yOffset;
      window.scrollTo({ top: y, behavior: 'smooth' });
    }
  }
}
