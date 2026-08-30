import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FeedbackModalService } from '../../../core/services/feedback-modal.service';

@Component({
  selector: 'app-feedback-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (modalService.state().isOpen) {
      <div 
        (click)="modalService.close()"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-fade-in">
        
        <div 
          (click)="$event.stopPropagation()"
          class="bg-white rounded-3xl max-w-sm w-full p-6 sm:p-7 shadow-2xl border border-[#E3D7C1] relative animate-scale-up text-center flex flex-col items-center">
          
          <!-- İkon Rozeti -->
          @if (modalService.state().type === 'success') {
            <div class="w-16 h-16 rounded-2xl bg-emerald-500/10 text-emerald-600 flex items-center justify-center mb-4 ring-8 ring-emerald-500/5 shadow-inner">
              <svg class="w-8 h-8 animate-bounce-short" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 13l4 4L19 7"></path>
              </svg>
            </div>
          } @else if (modalService.state().type === 'error') {
            <div class="w-16 h-16 rounded-2xl bg-red-500/10 text-red-600 flex items-center justify-center mb-4 ring-8 ring-red-500/5 shadow-inner">
              <svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"></path>
              </svg>
            </div>
          } @else {
            <div class="w-16 h-16 rounded-2xl bg-blue-500/10 text-blue-600 flex items-center justify-center mb-4 ring-8 ring-blue-500/5 shadow-inner">
              <svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
              </svg>
            </div>
          }

          <!-- Başlık -->
          <h3 class="text-xl font-extrabold text-[#3A2418] mb-2 tracking-tight">
            {{ modalService.state().title }}
          </h3>

          <!-- Açıklama Metni -->
          <p class="text-sm text-[#725B4D] leading-relaxed mb-6 font-medium">
            {{ modalService.state().message }}
          </p>

          <!-- Onay / Kapat Butonu -->
          <button 
            (click)="modalService.close()"
            [class.bg-[#3A2418]]="modalService.state().type === 'success'"
            [class.hover:bg-[#2A180E]]="modalService.state().type === 'success'"
            [class.bg-red-600]="modalService.state().type === 'error'"
            [class.hover:bg-red-700]="modalService.state().type === 'error'"
            class="w-full py-3.5 px-6 rounded-2xl text-white font-bold text-sm shadow-md transition-all duration-200 active:scale-95 cursor-pointer">
            {{ modalService.state().confirmText }}
          </button>

        </div>
      </div>
    }
  `
})
export class FeedbackModalComponent {
  modalService = inject(FeedbackModalService);
}
