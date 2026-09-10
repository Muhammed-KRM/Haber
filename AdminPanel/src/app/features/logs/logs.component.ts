import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { LogService } from '../../core/services/log.service';
import { EndpointLogResponseDto, FunctionLogResponseDto, LogFilterRequest } from '../../core/models/log.model';

@Component({
  selector: 'app-logs',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="space-y-6">
      <!-- Başlık & Açıklama -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h2 class="text-2xl font-bold tracking-tight text-slate-900">Sistem Logları</h2>
          <p class="text-xs sm:text-sm text-slate-500 mt-0.5">Uygulama arka ucunda gerçekleşen HTTP istekleri ve sistem hatalarını canlı izleyin.</p>
        </div>
        <button 
          (click)="loadLogs()" 
          class="self-start sm:self-auto inline-flex items-center gap-1.5 px-3.5 py-2 bg-white border border-slate-200 hover:bg-slate-50 text-slate-700 text-xs font-semibold rounded-lg shadow-xs transition-colors"
        >
          <svg class="w-3.5 h-3.5 text-slate-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
          </svg>
          Yenile
        </button>
      </div>

      <!-- Sekmeler (Tabs - Mobilde Yatay Kaydırılabilir) -->
      <div class="border-b border-slate-200">
        <nav class="-mb-px flex space-x-4 sm:space-x-8 overflow-x-auto custom-scrollbar pb-0.5">
          <button 
            (click)="switchTab('endpoints')"
            [class]="activeTab() === 'endpoints' 
              ? 'border-red-600 text-red-600 font-bold border-b-2' 
              : 'border-transparent text-slate-500 hover:text-slate-700 hover:border-slate-300 font-medium border-b-2'"
            class="whitespace-nowrap flex items-center py-3 px-1 text-xs sm:text-sm transition-colors shrink-0"
          >
            <svg class="w-4 h-4 mr-2 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
            </svg>
            API İstekleri (Endpoint)
          </button>
          <button 
            (click)="switchTab('functions')"
            [class]="activeTab() === 'functions' 
              ? 'border-red-600 text-red-600 font-bold border-b-2' 
              : 'border-transparent text-slate-500 hover:text-slate-700 hover:border-slate-300 font-medium border-b-2'"
            class="whitespace-nowrap flex items-center py-3 px-1 text-xs sm:text-sm transition-colors shrink-0"
          >
            <svg class="w-4 h-4 mr-2 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
            Uygulama Hataları (Function)
          </button>
        </nav>
      </div>

      <!-- Arama & Filtreleme Çubuğu -->
      <div class="flex flex-col sm:flex-row items-stretch sm:items-center gap-2.5 sm:gap-4 bg-white p-3 sm:p-4 rounded-xl shadow-xs border border-slate-200">
        <div class="flex-1 relative">
          <span class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none">
            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
            </svg>
          </span>
          <input 
            type="text" 
            [(ngModel)]="filterMethod" 
            (keyup.enter)="loadLogs()"
            [placeholder]="activeTab() === 'endpoints' ? 'Metoda veya yola göre ara (GET, /api/news)' : 'Hata seviyesine göre (Error, Critical)'"
            class="w-full pl-9 pr-4 py-2 text-xs sm:text-sm border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 transition-all placeholder:text-slate-400"
          />
        </div>
        <button 
          (click)="loadLogs()" 
          class="px-4 py-2 bg-slate-900 hover:bg-slate-800 text-white rounded-lg font-semibold text-xs transition-colors shrink-0"
        >
          Filtrele
        </button>
      </div>

      <!-- ============================================================ -->
      <!-- 1. ENDPOINTS (API İstekleri) BÖLÜMÜ                          -->
      <!-- ============================================================ -->
      <div [hidden]="activeTab() !== 'endpoints'" class="bg-white rounded-xl shadow-xs border border-slate-200 overflow-hidden">
        
        <!-- MOBİL KART GÖRÜNÜMÜ (Ekran < 768px - md:hidden) -->
        <div class="block md:hidden divide-y divide-slate-100">
          @for (log of endpointLogs(); track log.id) {
            <div 
              (click)="viewEndpointDetails(log)"
              class="p-3.5 space-y-2.5 hover:bg-slate-50 transition-colors cursor-pointer active:bg-slate-100"
            >
              <!-- Üst Satır: Rozetler & Detay Butonu -->
              <div class="flex items-center justify-between gap-2">
                <div class="flex items-center gap-1.5 flex-wrap">
                  <!-- HTTP Metod Rozeti -->
                  <span [class]="getMethodColor(log.method)" class="px-2 py-0.5 rounded text-[11px] font-bold tracking-wide">
                    {{ log.method }}
                  </span>
                  <!-- Durum Kodu -->
                  <span [class]="getStatusColor(log.statusCode)" class="px-2 py-0.5 rounded text-[11px] font-bold">
                    {{ log.statusCode }}
                  </span>
                  <!-- Süre -->
                  <span class="text-[11px] text-slate-500 font-mono bg-slate-100 px-1.5 py-0.5 rounded">
                    {{ log.durationMs }} ms
                  </span>
                </div>

                <!-- Detay Butonu (Geniş dokunma alanı) -->
                <button 
                  (click)="viewEndpointDetails(log); $event.stopPropagation()"
                  class="px-2.5 py-1 text-xs font-semibold text-red-600 bg-red-50 hover:bg-red-100 active:bg-red-200 rounded-lg shrink-0 transition-colors inline-flex items-center gap-1"
                >
                  <span>Detay</span>
                  <svg class="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
                  </svg>
                </button>
              </div>

              <!-- Yol (Path) -->
              <div class="font-mono text-xs text-slate-800 break-all bg-slate-50 px-2 py-1.5 rounded-lg border border-slate-100">
                {{ log.path }}
              </div>

              <!-- Alt Bilgi: Tarih & IP -->
              <div class="flex items-center justify-between text-[11px] text-slate-400">
                <span>{{ log.createdAt | date:'dd.MM.yyyy HH:mm:ss' }}</span>
                <span class="font-mono">{{ log.ipAddress || '—' }}</span>
              </div>
            </div>
          } @empty {
            <div class="py-12 text-center text-slate-400 text-xs">
              Kayıtlı API isteği bulunamadı.
            </div>
          }
        </div>

        <!-- MASAÜSTÜ TABLO GÖRÜNÜMÜ (Ekran >= 768px - hidden md:block) -->
        <div class="hidden md:block overflow-x-auto custom-scrollbar">
          <table class="w-full text-left text-xs divide-y divide-slate-200">
            <thead class="bg-slate-50/80 text-slate-500 text-[11px] uppercase tracking-wider font-semibold">
              <tr>
                <th class="py-3 px-4">Tarih</th>
                <th class="py-3 px-4">Method & Path</th>
                <th class="py-3 px-4">Durum</th>
                <th class="py-3 px-4">Süre</th>
                <th class="py-3 px-4">IP Adresi</th>
                <th class="py-3 px-4 text-right">İşlem</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100 bg-white">
              @for (log of endpointLogs(); track log.id) {
                <tr 
                  (click)="viewEndpointDetails(log)"
                  class="hover:bg-slate-50/80 transition-colors cursor-pointer group"
                >
                  <td class="py-3 px-4 whitespace-nowrap text-slate-500 font-mono text-[11px]">
                    {{ log.createdAt | date:'dd.MM.yyyy HH:mm:ss' }}
                  </td>
                  <td class="py-3 px-4">
                    <div class="flex items-center gap-2 max-w-md">
                      <span [class]="getMethodColor(log.method)" class="px-2 py-0.5 rounded text-[10px] font-bold shrink-0">
                        {{ log.method }}
                      </span>
                      <span class="text-slate-800 font-mono truncate" [title]="log.path">{{ log.path }}</span>
                    </div>
                  </td>
                  <td class="py-3 px-4 whitespace-nowrap">
                    <span [class]="getStatusColor(log.statusCode)" class="px-2 py-0.5 rounded text-[11px] font-semibold">
                      {{ log.statusCode }}
                    </span>
                  </td>
                  <td class="py-3 px-4 whitespace-nowrap text-slate-600 font-mono">
                    {{ log.durationMs }} ms
                  </td>
                  <td class="py-3 px-4 whitespace-nowrap text-slate-400 font-mono text-[11px]">
                    {{ log.ipAddress || '—' }}
                  </td>
                  <td class="py-3 px-4 whitespace-nowrap text-right">
                    <button 
                      (click)="viewEndpointDetails(log); $event.stopPropagation()"
                      class="px-2.5 py-1 text-xs font-semibold text-slate-700 bg-slate-100 hover:bg-red-50 hover:text-red-600 rounded-lg transition-colors inline-flex items-center gap-1"
                    >
                      Detay
                      <svg class="w-3 h-3 text-slate-400 group-hover:text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
                      </svg>
                    </button>
                  </td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="6" class="py-12 text-center text-slate-400 text-xs">
                    Kayıtlı API isteği bulunamadı.
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
        
        <!-- Sayfalama (Pagination) -->
        <div class="px-4 sm:px-6 py-3 flex flex-col sm:flex-row items-center justify-between gap-3 border-t border-slate-200 bg-slate-50/50">
          <div class="text-xs text-slate-500 text-center sm:text-left">
            Toplam <span class="font-bold text-slate-700">{{ endpointTotalCount() }}</span> kayıttan <span class="font-bold text-slate-700">{{ endpointLogs().length }}</span> tanesi listeleniyor.
          </div>
          <div class="flex items-center gap-2">
            <button 
              [disabled]="page() === 1" 
              (click)="changePage(page() - 1)" 
              class="p-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 text-slate-600 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
            >
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
              </svg>
            </button>
            <span class="py-1 px-3 text-xs font-semibold text-slate-700 bg-white border border-slate-200 rounded-lg">
              Sayfa {{ page() }} / {{ endpointTotalPages() || 1 }}
            </span>
            <button 
              [disabled]="page() >= endpointTotalPages()" 
              (click)="changePage(page() + 1)" 
              class="p-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 text-slate-600 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
            >
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
              </svg>
            </button>
          </div>
        </div>
      </div>

      <!-- ============================================================ -->
      <!-- 2. FUNCTIONS (Uygulama Hataları) BÖLÜMÜ                     -->
      <!-- ============================================================ -->
      <div [hidden]="activeTab() !== 'functions'" class="bg-white rounded-xl shadow-xs border border-slate-200 overflow-hidden">
        
        <!-- MOBİL KART GÖRÜNÜMÜ (Ekran < 768px - md:hidden) -->
        <div class="block md:hidden divide-y divide-slate-100">
          @for (log of functionLogs(); track log.id) {
            <div 
              (click)="viewFunctionDetails(log)"
              class="p-3.5 space-y-2.5 hover:bg-slate-50 transition-colors cursor-pointer active:bg-slate-100"
            >
              <!-- Üst Satır: Hata Seviyesi & Detay Butonu -->
              <div class="flex items-center justify-between gap-2">
                <div class="flex items-center gap-1.5 flex-wrap">
                  <span [class]="getSeverityColor(log.severity)" class="px-2 py-0.5 rounded text-[11px] font-bold">
                    {{ log.severity || 'Error' }}
                  </span>
                  @if (log.errorCode) {
                    <span class="px-1.5 py-0.5 rounded text-[10px] font-mono bg-slate-100 text-slate-700 border border-slate-200">
                      {{ log.errorCode }}
                    </span>
                  }
                </div>

                <button 
                  (click)="viewFunctionDetails(log); $event.stopPropagation()"
                  class="px-2.5 py-1 text-xs font-semibold text-red-600 bg-red-50 hover:bg-red-100 active:bg-red-200 rounded-lg shrink-0 transition-colors inline-flex items-center gap-1"
                >
                  <span>Detay</span>
                  <svg class="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
                  </svg>
                </button>
              </div>

              <!-- Kaynak (Class.Method) -->
              <div class="text-xs font-semibold text-slate-900 font-mono truncate">
                {{ log.className }}.{{ log.methodName }}
              </div>

              <!-- Hata Mesajı -->
              <div class="text-xs text-rose-700 bg-rose-50/70 p-2 rounded-lg border border-rose-100 line-clamp-2">
                {{ log.errorMessage }}
              </div>

              <!-- Alt Bilgi: Tarih -->
              <div class="text-[11px] text-slate-400">
                {{ log.createdAt | date:'dd.MM.yyyy HH:mm:ss' }}
              </div>
            </div>
          } @empty {
            <div class="py-12 text-center text-slate-400 text-xs">
              Kayıtlı uygulama hatası bulunamadı.
            </div>
          }
        </div>

        <!-- MASAÜSTÜ TABLO GÖRÜNÜMÜ (Ekran >= 768px - hidden md:block) -->
        <div class="hidden md:block overflow-x-auto custom-scrollbar">
          <table class="w-full text-left text-xs divide-y divide-slate-200">
            <thead class="bg-slate-50/80 text-slate-500 text-[11px] uppercase tracking-wider font-semibold">
              <tr>
                <th class="py-3 px-4">Tarih</th>
                <th class="py-3 px-4">Seviye</th>
                <th class="py-3 px-4">Kaynak</th>
                <th class="py-3 px-4">Hata Mesajı</th>
                <th class="py-3 px-4 text-right">İşlem</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100 bg-white">
              @for (log of functionLogs(); track log.id) {
                <tr 
                  (click)="viewFunctionDetails(log)"
                  class="hover:bg-slate-50/80 transition-colors cursor-pointer group"
                >
                  <td class="py-3 px-4 whitespace-nowrap text-slate-500 font-mono text-[11px]">
                    {{ log.createdAt | date:'dd.MM.yyyy HH:mm:ss' }}
                  </td>
                  <td class="py-3 px-4 whitespace-nowrap">
                    <span [class]="getSeverityColor(log.severity)" class="px-2 py-0.5 rounded text-[11px] font-semibold">
                      {{ log.severity || 'Error' }}
                    </span>
                  </td>
                  <td class="py-3 px-4 text-slate-900 font-mono text-xs max-w-xs truncate" [title]="log.className + '.' + log.methodName">
                    {{ log.className }}.{{ log.methodName }}
                  </td>
                  <td class="py-3 px-4 text-slate-600 max-w-sm truncate" [title]="log.errorMessage">
                    {{ log.errorMessage }}
                  </td>
                  <td class="py-3 px-4 whitespace-nowrap text-right">
                    <button 
                      (click)="viewFunctionDetails(log); $event.stopPropagation()"
                      class="px-2.5 py-1 text-xs font-semibold text-slate-700 bg-slate-100 hover:bg-red-50 hover:text-red-600 rounded-lg transition-colors inline-flex items-center gap-1"
                    >
                      Detay
                      <svg class="w-3 h-3 text-slate-400 group-hover:text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
                      </svg>
                    </button>
                  </td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="5" class="py-12 text-center text-slate-400 text-xs">
                    Kayıtlı uygulama hatası bulunamadı.
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
        
        <!-- Sayfalama (Pagination) -->
        <div class="px-4 sm:px-6 py-3 flex flex-col sm:flex-row items-center justify-between gap-3 border-t border-slate-200 bg-slate-50/50">
          <div class="text-xs text-slate-500 text-center sm:text-left">
            Toplam <span class="font-bold text-slate-700">{{ functionTotalCount() }}</span> kayıttan <span class="font-bold text-slate-700">{{ functionLogs().length }}</span> tanesi listeleniyor.
          </div>
          <div class="flex items-center gap-2">
            <button 
              [disabled]="page() === 1" 
              (click)="changePage(page() - 1)" 
              class="p-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 text-slate-600 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
            >
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
              </svg>
            </button>
            <span class="py-1 px-3 text-xs font-semibold text-slate-700 bg-white border border-slate-200 rounded-lg">
              Sayfa {{ page() }} / {{ functionTotalPages() || 1 }}
            </span>
            <button 
              [disabled]="page() >= functionTotalPages()" 
              (click)="changePage(page() + 1)" 
              class="p-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 text-slate-600 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
            >
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- ============================================================ -->
    <!-- 3. DETAY MODALI: ENDPOINT İSTEK DETAYI                         -->
    <!-- ============================================================ -->
    @if (selectedEndpointLog()) {
      <div class="fixed inset-0 bg-slate-950/60 backdrop-blur-xs z-50 flex items-end sm:items-center justify-center p-0 sm:p-4 animate-fade-in">
        <div class="bg-white rounded-t-2xl sm:rounded-2xl shadow-2xl w-full max-w-4xl max-h-[90vh] flex flex-col overflow-hidden">
          
          <!-- Modal Başlığı -->
          <div class="flex items-center justify-between p-4 sm:p-5 border-b border-slate-100 gap-2">
            <div class="min-w-0 flex-1">
              <h3 class="text-sm sm:text-base font-bold text-slate-900 truncate">
                İstek Detayı
                @if (selectedEndpointLog()!.traceId) {
                  <span class="text-xs font-mono font-normal text-slate-400 block sm:inline sm:ml-2 truncate">
                    ({{ selectedEndpointLog()!.traceId }})
                  </span>
                }
              </h3>
            </div>
            <button 
              (click)="selectedEndpointLog.set(null)" 
              title="Kapat"
              class="p-2 text-slate-400 hover:text-slate-700 hover:bg-slate-100 rounded-lg shrink-0 transition-colors"
            >
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
              </svg>
            </button>
          </div>

          <!-- Modal İçeriği (Kaydırılabilir) -->
          <div class="p-4 sm:p-6 overflow-y-auto custom-scrollbar flex-1 space-y-5">
            <!-- Temel Bilgiler Grid -->
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 sm:gap-4 text-xs sm:text-sm bg-slate-50 p-3.5 sm:p-4 rounded-xl border border-slate-100">
              <div class="flex items-center gap-2">
                <span class="font-semibold text-slate-500 w-24 shrink-0">Method:</span>
                <span [class]="getMethodColor(selectedEndpointLog()!.method)" class="px-2 py-0.5 rounded text-[11px] font-bold">
                  {{ selectedEndpointLog()!.method }}
                </span>
              </div>
              <div class="flex items-center gap-2">
                <span class="font-semibold text-slate-500 w-24 shrink-0">Durum:</span>
                <span [class]="getStatusColor(selectedEndpointLog()!.statusCode)" class="px-2 py-0.5 rounded text-[11px] font-bold">
                  {{ selectedEndpointLog()!.statusCode }}
                </span>
              </div>
              <div class="sm:col-span-2 flex flex-col sm:flex-row sm:items-start gap-1 sm:gap-2">
                <span class="font-semibold text-slate-500 w-24 shrink-0">Yol (Path):</span>
                <span class="font-mono text-slate-800 break-all bg-white px-2 py-1 rounded border border-slate-200 text-xs">{{ selectedEndpointLog()!.path }}</span>
              </div>
              <div class="flex items-center gap-2">
                <span class="font-semibold text-slate-500 w-24 shrink-0">Süre:</span>
                <span class="font-mono font-semibold text-slate-700">{{ selectedEndpointLog()!.durationMs }} ms</span>
              </div>
              <div class="flex items-center gap-2">
                <span class="font-semibold text-slate-500 w-24 shrink-0">Kullanıcı IP:</span>
                <span class="font-mono text-slate-700">{{ selectedEndpointLog()!.ipAddress || '—' }}</span>
              </div>
              <div class="sm:col-span-2 flex flex-col sm:flex-row sm:items-center gap-1 sm:gap-2">
                <span class="font-semibold text-slate-500 w-24 shrink-0">Kullanıcı ID:</span>
                <span class="font-mono text-slate-600 text-xs">{{ selectedEndpointLog()!.userId || 'Anonim (Ziyaretçi)' }}</span>
              </div>
              <div class="sm:col-span-2 flex flex-col sm:flex-row sm:items-center gap-1 sm:gap-2">
                <span class="font-semibold text-slate-500 w-24 shrink-0">Tarih:</span>
                <span class="text-slate-700">{{ selectedEndpointLog()!.createdAt | date:'dd MMMM yyyy, HH:mm:ss' }}</span>
              </div>
            </div>
            
            <!-- Request Body (İstek Gövdesi) -->
            @if (selectedEndpointLog()!.requestBody) {
              <div>
                <h4 class="font-bold text-xs uppercase tracking-wider text-slate-600 mb-1.5 flex items-center gap-1.5">
                  <span class="w-2 h-2 rounded-full bg-blue-500"></span>
                  İstek Gövdesi (Request Body)
                </h4>
                <pre class="bg-slate-900 text-slate-100 p-3 sm:p-4 rounded-xl text-xs overflow-x-auto whitespace-pre-wrap break-all custom-scrollbar font-mono"><code>{{ formatJson(selectedEndpointLog()!.requestBody!) }}</code></pre>
              </div>
            }
            
            <!-- Response Body (Yanıt Gövdesi) -->
            @if (selectedEndpointLog()!.responseBody) {
              <div>
                <h4 class="font-bold text-xs uppercase tracking-wider text-slate-600 mb-1.5 flex items-center gap-1.5">
                  <span class="w-2 h-2 rounded-full bg-emerald-500"></span>
                  Yanıt Gövdesi (Response Body)
                </h4>
                <pre class="bg-slate-900 text-slate-100 p-3 sm:p-4 rounded-xl text-xs overflow-x-auto whitespace-pre-wrap break-all custom-scrollbar font-mono max-h-80"><code>{{ formatJson(selectedEndpointLog()!.responseBody!) }}</code></pre>
              </div>
            }
          </div>

          <!-- Modal Alt Kapat Butonu -->
          <div class="p-3 sm:p-4 border-t border-slate-100 bg-slate-50 flex justify-end">
            <button 
              (click)="selectedEndpointLog.set(null)"
              class="w-full sm:w-auto px-5 py-2 bg-slate-800 hover:bg-slate-900 text-white rounded-lg text-xs font-semibold transition-colors"
            >
              Kapat
            </button>
          </div>
        </div>
      </div>
    }

    <!-- ============================================================ -->
    <!-- 4. DETAY MODALI: FUNCTION HATA DETAYI                         -->
    <!-- ============================================================ -->
    @if (selectedFunctionLog()) {
      <div class="fixed inset-0 bg-slate-950/60 backdrop-blur-xs z-50 flex items-end sm:items-center justify-center p-0 sm:p-4 animate-fade-in">
        <div class="bg-white rounded-t-2xl sm:rounded-2xl shadow-2xl w-full max-w-4xl max-h-[90vh] flex flex-col overflow-hidden">
          
          <!-- Modal Başlığı -->
          <div class="flex items-center justify-between p-4 sm:p-5 border-b border-slate-100 gap-2">
            <div class="min-w-0 flex-1">
              <h3 class="text-sm sm:text-base font-bold text-rose-600 flex items-center gap-2 truncate">
                <svg class="w-4 h-4 shrink-0 text-rose-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <span>Hata Detayı</span>
                @if (selectedFunctionLog()!.errorCode) {
                  <span class="text-xs font-mono font-normal text-slate-500 bg-slate-100 px-2 py-0.5 rounded">
                    {{ selectedFunctionLog()!.errorCode }}
                  </span>
                }
              </h3>
            </div>
            <button 
              (click)="selectedFunctionLog.set(null)" 
              title="Kapat"
              class="p-2 text-slate-400 hover:text-slate-700 hover:bg-slate-100 rounded-lg shrink-0 transition-colors"
            >
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
              </svg>
            </button>
          </div>

          <!-- Modal İçeriği (Kaydırılabilir) -->
          <div class="p-4 sm:p-6 overflow-y-auto custom-scrollbar flex-1 space-y-5">
            <!-- Hata Mesajı Kutusu -->
            <div class="bg-rose-50 border border-rose-200 rounded-xl p-3.5 sm:p-4 text-rose-900 text-xs sm:text-sm font-semibold break-words">
              {{ selectedFunctionLog()!.errorMessage }}
            </div>
            
            <!-- Detay Grid -->
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 sm:gap-4 text-xs sm:text-sm bg-slate-50 p-3.5 sm:p-4 rounded-xl border border-slate-100">
              <div class="flex items-center gap-2">
                <span class="font-semibold text-slate-500 w-20 shrink-0">Seviye:</span>
                <span [class]="getSeverityColor(selectedFunctionLog()!.severity)" class="px-2 py-0.5 rounded text-[11px] font-bold">
                  {{ selectedFunctionLog()!.severity || 'Error' }}
                </span>
              </div>
              <div class="flex items-center gap-2">
                <span class="font-semibold text-slate-500 w-20 shrink-0">Metot:</span>
                <span class="font-mono text-slate-800 text-xs truncate">{{ selectedFunctionLog()!.methodName }}</span>
              </div>
              <div class="sm:col-span-2 flex flex-col sm:flex-row sm:items-start gap-1 sm:gap-2">
                <span class="font-semibold text-slate-500 w-20 shrink-0">Sınıf:</span>
                <span class="font-mono text-slate-800 text-xs break-all">{{ selectedFunctionLog()!.className }}</span>
              </div>
              @if (selectedFunctionLog()!.filePath) {
                <div class="sm:col-span-2 flex flex-col sm:flex-row sm:items-start gap-1 sm:gap-2">
                  <span class="font-semibold text-slate-500 w-20 shrink-0">Dosya/Satır:</span>
                  <span class="font-mono text-slate-600 text-xs break-all bg-white px-2 py-1 rounded border border-slate-200">
                    {{ selectedFunctionLog()!.filePath }}:{{ selectedFunctionLog()!.lineNumber }}
                  </span>
                </div>
              }
              <div class="sm:col-span-2 flex items-center gap-2 text-slate-500 text-xs">
                <span class="font-semibold text-slate-500 w-20 shrink-0">Tarih:</span>
                <span>{{ selectedFunctionLog()!.createdAt | date:'dd MMMM yyyy, HH:mm:ss' }}</span>
              </div>
            </div>
            
            <!-- Input Data (Girdi Değeri) -->
            @if (selectedFunctionLog()!.inputValue) {
              <div>
                <h4 class="font-bold text-xs uppercase tracking-wider text-slate-600 mb-1.5 flex items-center gap-1.5">
                  <span class="w-2 h-2 rounded-full bg-amber-500"></span>
                  Girdi Parametreleri ({{ selectedFunctionLog()!.inputType || 'Payload' }})
                </h4>
                <pre class="bg-slate-900 text-slate-100 p-3 sm:p-4 rounded-xl text-xs overflow-x-auto whitespace-pre-wrap break-all custom-scrollbar font-mono"><code>{{ formatJson(selectedFunctionLog()!.inputValue!) }}</code></pre>
              </div>
            }
            
            <!-- Stack Trace (Hata Yığını) -->
            @if (selectedFunctionLog()!.stackTrace) {
              <div>
                <h4 class="font-bold text-xs uppercase tracking-wider text-slate-600 mb-1.5 flex items-center gap-1.5">
                  <span class="w-2 h-2 rounded-full bg-rose-500"></span>
                  Hata Yığını (Stack Trace)
                </h4>
                <pre class="bg-slate-900 text-rose-300 p-3 sm:p-4 rounded-xl text-xs overflow-x-auto whitespace-pre-wrap break-all custom-scrollbar font-mono max-h-72"><code>{{ selectedFunctionLog()!.stackTrace }}</code></pre>
              </div>
            }
          </div>

          <!-- Modal Alt Kapat Butonu -->
          <div class="p-3 sm:p-4 border-t border-slate-100 bg-slate-50 flex justify-end">
            <button 
              (click)="selectedFunctionLog.set(null)"
              class="w-full sm:w-auto px-5 py-2 bg-slate-800 hover:bg-slate-900 text-white rounded-lg text-xs font-semibold transition-colors"
            >
              Kapat
            </button>
          </div>
        </div>
      </div>
    }
  `
})
export class LogsComponent implements OnInit {
  private logService = inject(LogService);

  activeTab = signal<'endpoints' | 'functions'>('endpoints');
  
  page = signal(1);
  pageSize = signal(50);
  filterMethod = '';

  endpointLogs = signal<EndpointLogResponseDto[]>([]);
  endpointTotalCount = signal(0);
  endpointTotalPages = signal(0);

  functionLogs = signal<FunctionLogResponseDto[]>([]);
  functionTotalCount = signal(0);
  functionTotalPages = signal(0);

  selectedEndpointLog = signal<EndpointLogResponseDto | null>(null);
  selectedFunctionLog = signal<FunctionLogResponseDto | null>(null);

  ngOnInit() {
    this.loadLogs();
  }

  async loadLogs() {
    const filter: LogFilterRequest = {
      pageNumber: this.page(),
      pageSize: this.pageSize()
    };

    try {
      if (this.activeTab() === 'endpoints') {
        if (this.filterMethod) filter.method = this.filterMethod.toUpperCase();
        const res = await firstValueFrom(this.logService.getEndpointLogs(filter));
        this.endpointLogs.set(res.items);
        this.endpointTotalCount.set(res.totalCount);
        this.endpointTotalPages.set(res.totalPages);
      } else {
        if (this.filterMethod) filter.severity = this.filterMethod;
        const res = await firstValueFrom(this.logService.getFunctionLogs(filter));
        this.functionLogs.set(res.items);
        this.functionTotalCount.set(res.totalCount);
        this.functionTotalPages.set(res.totalPages);
      }
    } catch (err) {
      console.error('Log yükleme hatası:', err);
    }
  }

  changePage(newPage: number) {
    this.page.set(newPage);
    this.loadLogs();
  }

  switchTab(tab: 'endpoints' | 'functions') {
    if (this.activeTab() !== tab) {
      this.activeTab.set(tab);
      this.page.set(1);
      this.filterMethod = '';
      this.loadLogs();
    }
  }

  viewEndpointDetails(log: EndpointLogResponseDto) {
    this.selectedEndpointLog.set(log);
  }

  viewFunctionDetails(log: FunctionLogResponseDto) {
    this.selectedFunctionLog.set(log);
  }

  formatJson(val: string): string {
    if (!val) return '';
    try {
      const obj = JSON.parse(val);
      return JSON.stringify(obj, null, 2);
    } catch {
      return val;
    }
  }

  getMethodColor(method: string): string {
    switch (method?.toUpperCase()) {
      case 'GET': return 'bg-blue-50 text-blue-700 border border-blue-200';
      case 'POST': return 'bg-emerald-50 text-emerald-700 border border-emerald-200';
      case 'PUT': return 'bg-amber-50 text-amber-700 border border-amber-200';
      case 'DELETE': return 'bg-rose-50 text-rose-700 border border-rose-200';
      case 'OPTIONS': return 'bg-purple-50 text-purple-700 border border-purple-200';
      default: return 'bg-slate-100 text-slate-700 border border-slate-200';
    }
  }

  getStatusColor(code: number): string {
    if (code >= 200 && code < 300) return 'bg-emerald-50 text-emerald-700 border border-emerald-200';
    if (code >= 300 && code < 400) return 'bg-blue-50 text-blue-700 border border-blue-200';
    if (code >= 400 && code < 500) return 'bg-amber-50 text-amber-700 border border-amber-200';
    if (code >= 500) return 'bg-rose-50 text-rose-700 border border-rose-200';
    return 'bg-slate-100 text-slate-700 border border-slate-200';
  }

  getSeverityColor(severity?: string): string {
    if (severity === 'Critical') return 'bg-rose-50 text-rose-700 border border-rose-200';
    return 'bg-amber-50 text-amber-700 border border-amber-200';
  }
}
