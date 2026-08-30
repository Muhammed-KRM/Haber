import { Injectable, signal } from '@angular/core';

export interface FeedbackModalState {
  isOpen: boolean;
  type: 'success' | 'error' | 'info';
  title: string;
  message: string;
  confirmText: string;
}

@Injectable({ providedIn: 'root' })
export class FeedbackModalService {
  state = signal<FeedbackModalState>({
    isOpen: false,
    type: 'success',
    title: 'İşlem Başarılı',
    message: 'Kayıt işlemi başarıyla tamamlandı.',
    confirmText: 'Tamam'
  });

  showSuccess(message: string, title: string = 'İşlem Başarılı'): void {
    this.state.set({
      isOpen: true,
      type: 'success',
      title,
      message,
      confirmText: 'Tamam'
    });
  }

  showError(message: string, title: string = 'Bir Hata Oluştu'): void {
    this.state.set({
      isOpen: true,
      type: 'error',
      title,
      message,
      confirmText: 'Anladım'
    });
  }

  close(): void {
    this.state.update(s => ({ ...s, isOpen: false }));
  }
}
