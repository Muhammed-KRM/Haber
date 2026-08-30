import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-qr-generator',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="space-y-6">
      <div class="bg-white p-6 rounded-3xl border border-[#E3D7C1] shadow-sm">
        <h2 class="text-xl font-extrabold text-[#3A2418] mb-1">Masa QR Kod Üretici</h2>
        <p class="text-xs text-[#725B4D] mb-6">Restoran masalarınız için özel QR kodlar oluşturun ve çıktısını alın.</p>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-8 items-start">
          <!-- Form Alanı -->
          <div class="space-y-4">
            <div>
              <label class="block text-xs font-bold text-[#725B4D] uppercase mb-1">Masa Numarası / Adı</label>
              <input 
                type="text" 
                [(ngModel)]="tableName"
                (ngModelChange)="generateQr()"
                placeholder="Örn: Masa 1, Bahçe 4, Teras 2"
                class="w-full bg-[#F7F1E3] text-[#3A2418] px-4 py-2.5 rounded-xl text-sm border border-[#E3D7C1] focus:border-[#C65D3A] focus:outline-none"
              />
            </div>

            <div>
              <label class="block text-xs font-bold text-[#725B4D] uppercase mb-1">Menü Web Adresi (URL)</label>
              <input 
                type="text" 
                [(ngModel)]="baseUrl"
                (ngModelChange)="generateQr()"
                placeholder="http://localhost:4200"
                class="w-full bg-[#F7F1E3] text-[#3A2418] px-4 py-2.5 rounded-xl text-sm border border-[#E3D7C1] focus:outline-none"
              />
            </div>

            <div class="p-4 bg-[#F7F1E3] rounded-2xl border border-[#E3D7C1] text-xs space-y-1 text-[#725B4D]">
              <div class="font-bold text-[#3A2418]">Oluşturulan Hedef Bağlantı:</div>
              <div class="font-mono text-[11px] text-[#C65D3A] break-all">{{ targetUrl() }}</div>
            </div>

            <button 
              (click)="printQr()"
              class="w-full py-3 bg-[#3A2418] hover:bg-[#2A180E] text-[#F7F1E3] font-bold text-xs rounded-xl shadow-md transition-all active:scale-98 flex items-center justify-center gap-2 cursor-pointer">
              <span>🖨️ QR Kodunu Yazdır</span>
            </button>
          </div>

          <!-- QR Kart Önizlemesi (Yazdırılabilir Alan) -->
          <div id="printable-qr-card" class="bg-[#F7F1E3] p-8 rounded-3xl border-2 border-dashed border-[#3A2418]/30 flex flex-col items-center justify-center text-center shadow-inner">
            <div class="inline-block px-3.5 py-1 rounded-full bg-[#3A2418] text-[#F7F1E3] text-xs font-bold tracking-wider uppercase mb-3">
              {{ tableName || 'Genel Menü' }}
            </div>
            
            <h3 class="text-2xl font-black text-[#3A2418] mb-1">Lezzet Durağı</h3>
            <p class="text-xs text-[#725B4D] mb-5">Menüyü görüntülemek için kameranızla okutun</p>

            <div class="bg-white p-4 rounded-2xl shadow-md border border-[#E3D7C1] mb-4">
              <img [src]="qrCodeUrl()" alt="Masa QR Kod" class="w-48 h-48 object-contain" />
            </div>

            <div class="text-[11px] font-bold text-[#C65D3A]">Afiyet Olsun!</div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class QrGeneratorComponent {
  tableName = 'Masa 1';
  baseUrl = 'http://localhost:4200';
  
  targetUrl = signal<string>('http://localhost:4200/?table=Masa 1');
  qrCodeUrl = signal<string>('https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=http://localhost:4200/?table=Masa%201');

  generateQr(): void {
    const url = `${this.baseUrl}/?table=${encodeURIComponent(this.tableName.trim())}`;
    this.targetUrl.set(url);
    this.qrCodeUrl.set(`https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=${encodeURIComponent(url)}`);
  }

  printQr(): void {
    window.print();
  }
}
