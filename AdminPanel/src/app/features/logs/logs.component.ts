import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LogService } from '../../core/services/log.service';
import { EndpointLogResponseDto, FunctionLogResponseDto, LogFilterRequest } from '../../core/models/log.model';
import {
  LucideActivity,
  LucideTriangleAlert,
  LucideX,
  LucideSearch,
  LucideChevronLeft,
  LucideChevronRight
} from '@lucide/angular';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-logs',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    LucideActivity,
    LucideTriangleAlert,
    LucideX,
    LucideSearch,
    LucideChevronLeft,
    LucideChevronRight
  ],
  template: `
    <div class="space-y-6">
      <div class="flex justify-between items-center">
        <div>
          <h2 class="text-2xl font-bold tracking-tight">Sistem Logları</h2>
          <p class="text-muted-foreground text-sm">Uygulama arka ucunda gerçekleşen HTTP istekleri ve hataları buradan izleyebilirsiniz.</p>
        </div>
      </div>

      <!-- Tabs -->
      <div class="border-b border-gray-200">
        <nav class="-mb-px flex space-x-8">
          <button (click)="switchTab('endpoints')"
                  [class]="activeTab() === 'endpoints' ? 'border-primary text-primary' : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700'"
                  class="whitespace-nowrap flex items-center py-4 px-1 border-b-2 font-medium text-sm">
            <svg lucideActivity class="mr-2" [size]="16"></svg>
            API İstekleri (Endpoint)
          </button>
          <button (click)="switchTab('functions')"
                  [class]="activeTab() === 'functions' ? 'border-primary text-primary' : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700'"
                  class="whitespace-nowrap flex items-center py-4 px-1 border-b-2 font-medium text-sm">
            <svg lucideTriangleAlert class="mr-2" [size]="16"></svg>
            Uygulama Hataları (Function)
          </button>
        </nav>
      </div>

      <!-- Filters -->
      <div class="flex items-center gap-4 bg-white p-4 rounded-lg shadow-sm border border-gray-100">
        <div class="flex-1 max-w-sm relative">
          <svg lucideSearch class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" [size]="16"></svg>
          <input type="text" [(ngModel)]="filterMethod" (keyup.enter)="loadLogs()"
                 [placeholder]="activeTab() === 'endpoints' ? 'Metoda göre ara (GET, POST)' : 'Hata seviyesine göre (Error, Critical)'"
                 class="w-full pl-9 pr-4 py-2 border rounded-md focus:ring-2 focus:ring-primary focus:border-primary">
        </div>
        <button (click)="loadLogs()" class="px-4 py-2 bg-gray-100 hover:bg-gray-200 rounded-md font-medium text-sm transition-colors">
          Filtrele
        </button>
      </div>

      <!-- Endpoints Table -->
      <div [hidden]="activeTab() !== 'endpoints'" class="bg-white rounded-lg shadow border overflow-hidden">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Tarih</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Method & Path</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Durum (Status)</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Süre</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">İşlem</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            @for (log of endpointLogs(); track log.id) {
              <tr class="hover:bg-gray-50 transition-colors">
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {{ log.createdAt | date:'short' }}
                </td>
                <td class="px-6 py-4 text-sm font-medium">
                  <span [class]="getMethodColor(log.method)" class="px-2 py-1 rounded text-xs font-bold mr-2">
                    {{ log.method }}
                  </span>
                  <span class="text-gray-900 truncate max-w-xs inline-block align-bottom" [title]="log.path">{{ log.path }}</span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span [class]="getStatusColor(log.statusCode)" class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full">
                    {{ log.statusCode }}
                  </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {{ log.durationMs }} ms
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                  <button (click)="viewEndpointDetails(log)" class="text-primary hover:text-primary/80">Detay</button>
                </td>
              </tr>
            }
          </tbody>
        </table>
        
        <!-- Pagination -->
        <div class="px-6 py-3 flex items-center justify-between border-t border-gray-200 bg-white">
          <div class="text-sm text-gray-700">
            Toplam <span class="font-medium">{{ endpointTotalCount() }}</span> kayıttan <span class="font-medium">{{ endpointLogs().length }}</span> tanesi gösteriliyor.
          </div>
          <div class="flex space-x-2">
            <button [disabled]="page() === 1" (click)="changePage(page() - 1)" class="p-1 rounded border hover:bg-gray-50 disabled:opacity-50">
              <svg lucideChevronLeft [size]="20"></svg>
            </button>
            <span class="py-1 px-2 text-sm">Sayfa {{ page() }} / {{ endpointTotalPages() }}</span>
            <button [disabled]="page() >= endpointTotalPages()" (click)="changePage(page() + 1)" class="p-1 rounded border hover:bg-gray-50 disabled:opacity-50">
              <svg lucideChevronRight [size]="20"></svg>
            </button>
          </div>
        </div>
      </div>

      <!-- Functions Table -->
      <div [hidden]="activeTab() !== 'functions'" class="bg-white rounded-lg shadow border overflow-hidden">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Tarih</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Seviye</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Kaynak</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Mesaj</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">İşlem</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            @for (log of functionLogs(); track log.id) {
              <tr class="hover:bg-gray-50 transition-colors">
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {{ log.createdAt | date:'short' }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span [class]="getSeverityColor(log.severity)" class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full">
                    {{ log.severity || 'Error' }}
                  </span>
                </td>
                <td class="px-6 py-4 text-sm font-medium text-gray-900 truncate max-w-xs" [title]="log.className + '.' + log.methodName">
                  {{ log.className }}.{{ log.methodName }}
                </td>
                <td class="px-6 py-4 text-sm text-gray-500 truncate max-w-sm" [title]="log.errorMessage">
                  {{ log.errorMessage }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                  <button (click)="viewFunctionDetails(log)" class="text-primary hover:text-primary/80">Detay</button>
                </td>
              </tr>
            }
          </tbody>
        </table>
        
        <!-- Pagination -->
        <div class="px-6 py-3 flex items-center justify-between border-t border-gray-200 bg-white">
          <div class="text-sm text-gray-700">
            Toplam <span class="font-medium">{{ functionTotalCount() }}</span> kayıttan <span class="font-medium">{{ functionLogs().length }}</span> tanesi gösteriliyor.
          </div>
          <div class="flex space-x-2">
            <button [disabled]="page() === 1" (click)="changePage(page() - 1)" class="p-1 rounded border hover:bg-gray-50 disabled:opacity-50">
              <svg lucideChevronLeft [size]="20"></svg>
            </button>
            <span class="py-1 px-2 text-sm">Sayfa {{ page() }} / {{ functionTotalPages() }}</span>
            <button [disabled]="page() >= functionTotalPages()" (click)="changePage(page() + 1)" class="p-1 rounded border hover:bg-gray-50 disabled:opacity-50">
              <svg lucideChevronRight [size]="20"></svg>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Details Modal -->
    @if (selectedEndpointLog()) {
      <div class="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
        <div class="bg-white rounded-lg shadow-xl w-full max-w-4xl max-h-[90vh] flex flex-col">
          <div class="flex items-center justify-between p-4 border-b">
            <h3 class="text-lg font-semibold">İstek Detayı ({{ selectedEndpointLog()!.traceId }})</h3>
            <button (click)="selectedEndpointLog.set(null)" class="text-gray-400 hover:text-gray-600">
              <svg lucideX [size]="20"></svg>
            </button>
          </div>
          <div class="p-6 overflow-y-auto flex-1 space-y-6">
            <div class="grid grid-cols-2 gap-4 text-sm">
              <div><span class="font-semibold text-gray-500">Method:</span> <span class="font-mono">{{ selectedEndpointLog()!.method }}</span></div>
              <div><span class="font-semibold text-gray-500">Durum:</span> {{ selectedEndpointLog()!.statusCode }}</div>
              <div><span class="font-semibold text-gray-500">Yol (Path):</span> <span class="font-mono">{{ selectedEndpointLog()!.path }}</span></div>
              <div><span class="font-semibold text-gray-500">Süre:</span> {{ selectedEndpointLog()!.durationMs }} ms</div>
              <div><span class="font-semibold text-gray-500">Kullanıcı (ID):</span> {{ selectedEndpointLog()!.userId || 'Anonim' }}</div>
              <div><span class="font-semibold text-gray-500">Kullanıcı IP:</span> {{ selectedEndpointLog()!.ipAddress }}</div>
            </div>
            
            @if (selectedEndpointLog()!.requestBody) {
              <div>
                <h4 class="font-semibold text-sm mb-2 text-gray-700">Request Body</h4>
                <pre class="bg-gray-900 text-gray-100 p-4 rounded-md text-xs overflow-x-auto"><code>{{ formatJson(selectedEndpointLog()!.requestBody!) }}</code></pre>
              </div>
            }
            
            @if (selectedEndpointLog()!.responseBody) {
              <div>
                <h4 class="font-semibold text-sm mb-2 text-gray-700">Response Body</h4>
                <pre class="bg-gray-900 text-gray-100 p-4 rounded-md text-xs overflow-x-auto"><code>{{ formatJson(selectedEndpointLog()!.responseBody!) }}</code></pre>
              </div>
            }
          </div>
        </div>
      </div>
    }

    @if (selectedFunctionLog()) {
      <div class="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
        <div class="bg-white rounded-lg shadow-xl w-full max-w-4xl max-h-[90vh] flex flex-col">
          <div class="flex items-center justify-between p-4 border-b">
            <h3 class="text-lg font-semibold text-red-600">Hata Detayı ({{ selectedFunctionLog()!.errorCode || 'Bilinmiyor' }})</h3>
            <button (click)="selectedFunctionLog.set(null)" class="text-gray-400 hover:text-gray-600">
              <svg lucideX [size]="20"></svg>
            </button>
          </div>
          <div class="p-6 overflow-y-auto flex-1 space-y-6">
            <div class="bg-red-50 border border-red-100 rounded-md p-4 text-red-800 text-sm font-medium">
              {{ selectedFunctionLog()!.errorMessage }}
            </div>
            
            <div class="grid grid-cols-2 gap-4 text-sm">
              <div><span class="font-semibold text-gray-500">Sınıf:</span> <span class="font-mono">{{ selectedFunctionLog()!.className }}</span></div>
              <div><span class="font-semibold text-gray-500">Metot:</span> <span class="font-mono">{{ selectedFunctionLog()!.methodName }}</span></div>
              <div class="col-span-2"><span class="font-semibold text-gray-500">Dosya:</span> <span class="font-mono text-xs">{{ selectedFunctionLog()!.filePath }}:{{ selectedFunctionLog()!.lineNumber }}</span></div>
            </div>
            
            @if (selectedFunctionLog()!.inputValue) {
              <div>
                <h4 class="font-semibold text-sm mb-2 text-gray-700">Input Data ({{ selectedFunctionLog()!.inputType }})</h4>
                <pre class="bg-gray-900 text-gray-100 p-4 rounded-md text-xs overflow-x-auto"><code>{{ formatJson(selectedFunctionLog()!.inputValue!) }}</code></pre>
              </div>
            }
            
            @if (selectedFunctionLog()!.stackTrace) {
              <div>
                <h4 class="font-semibold text-sm mb-2 text-gray-700">Stack Trace</h4>
                <pre class="bg-gray-100 text-gray-800 p-4 rounded-md border border-gray-200 text-xs overflow-x-auto whitespace-pre-wrap font-mono">{{ selectedFunctionLog()!.stackTrace }}</pre>
              </div>
            }
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

  // Her sekme için ayrı signal'ler - böylece Angular her zaman bunları takip eder
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
      case 'GET': return 'bg-blue-100 text-blue-800';
      case 'POST': return 'bg-green-100 text-green-800';
      case 'PUT': return 'bg-yellow-100 text-yellow-800';
      case 'DELETE': return 'bg-red-100 text-red-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }

  getStatusColor(code: number): string {
    if (code >= 200 && code < 300) return 'bg-green-100 text-green-800';
    if (code >= 400 && code < 500) return 'bg-yellow-100 text-yellow-800';
    if (code >= 500) return 'bg-red-100 text-red-800';
    return 'bg-gray-100 text-gray-800';
  }

  getSeverityColor(severity?: string): string {
    if (severity === 'Critical') return 'bg-red-100 text-red-800';
    return 'bg-orange-100 text-orange-800';
  }
}
