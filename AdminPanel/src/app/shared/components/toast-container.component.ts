import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="fixed bottom-5 right-5 z-50 flex flex-col gap-2.5 max-w-sm w-full pointer-events-none">
      @for (toast of toastService.toasts(); track toast.id) {
        <div 
          class="pointer-events-auto flex items-start gap-3 p-4 rounded-xl shadow-lg border transition-all duration-300 animate-fade-in"
          [ngClass]="{
            'bg-white text-slate-900 border-emerald-200 shadow-emerald-500/10': toast.type === 'success',
            'bg-white text-slate-900 border-rose-200 shadow-rose-500/10': toast.type === 'error',
            'bg-white text-slate-900 border-amber-200 shadow-amber-500/10': toast.type === 'warning',
            'bg-white text-slate-900 border-blue-200 shadow-blue-500/10': toast.type === 'info'
          }"
        >
          <!-- İkon Göstergesi -->
          <div class="w-6 h-6 rounded-full flex items-center justify-center shrink-0 mt-0.5"
            [ngClass]="{
              'bg-emerald-50 text-emerald-600': toast.type === 'success',
              'bg-rose-50 text-rose-600': toast.type === 'error',
              'bg-amber-50 text-amber-600': toast.type === 'warning',
              'bg-blue-50 text-blue-600': toast.type === 'info'
            }"
          >
            @if (toast.type === 'success') {
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/></svg>
            } @else if (toast.type === 'error') {
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
            } @else if (toast.type === 'warning') {
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"/></svg>
            } @else {
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
            }
          </div>

          <div class="flex-1 min-w-0">
            <h4 class="text-sm font-semibold text-slate-800 leading-tight">{{ toast.title }}</h4>
            @if (toast.message) {
              <p class="text-xs text-slate-500 mt-1 leading-relaxed">{{ toast.message }}</p>
            }
          </div>

          <button 
            (click)="toastService.remove(toast.id)"
            class="text-slate-400 hover:text-slate-600 transition-colors p-1 rounded-md"
          >
            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
          </button>
        </div>
      }
    </div>
  `
})
export class ToastContainerComponent {
  constructor(public toastService: ToastService) {}
}
