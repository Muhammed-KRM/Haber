import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ToastContainerComponent } from '../components/toast-container.component';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, ToastContainerComponent],
  template: `
    <div class="flex h-screen bg-slate-50 text-slate-900 font-sans overflow-hidden relative">
      <!-- MOBİL KARARTMA KATMANI (Backdrop Overlay) -->
      @if (isMobileOpen()) {
        <div 
          (click)="closeMobileSidebar()" 
          class="fixed inset-0 bg-slate-950/70 backdrop-blur-xs z-40 lg:hidden transition-opacity duration-300"
        ></div>
      }

      <!-- SIDEBAR (Masaüstünde Normal/Küçülebilir, Mobilde Açılır-Kapanır Drawer) -->
      <aside 
        class="bg-slate-900 text-slate-300 flex flex-col justify-between border-r border-slate-800 transition-all duration-300 select-none
               fixed lg:static inset-y-0 left-0 z-50 h-full shrink-0 shadow-2xl lg:shadow-none"
        [class.translate-x-0]="isMobileOpen()"
        [class.-translate-x-full]="!isMobileOpen()"
        [class.lg:translate-x-0]="true"
        [class.w-72]="isMobileOpen()"
        [class.lg:w-64]="!isDesktopCollapsed()"
        [class.lg:w-20]="isDesktopCollapsed()"
      >
        <!-- Üst Kısım (Logo & Navigasyon) -->
        <div class="flex flex-col h-full overflow-hidden">
          <!-- Logo & Başlık & Kapatma Butonları -->
          <div class="h-16 flex items-center justify-between px-4 sm:px-5 border-b border-slate-800/80 bg-slate-950/40 shrink-0">
            <div class="flex items-center gap-3 overflow-hidden">
              <!-- Kürsü TV Kırmızı İkon Rozeti -->
              <div class="w-9 h-9 rounded-xl bg-gradient-to-tr from-rose-700 to-red-600 flex items-center justify-center text-white font-black text-lg shadow-md shadow-red-900/30 shrink-0">
                K
              </div>
              <!-- Başlık -->
              <div class="flex flex-col" [class.lg:hidden]="isDesktopCollapsed()">
                <div class="flex items-center gap-1.5">
                  <span class="font-extrabold text-white text-base tracking-tight">KÜRSÜ<span class="text-red-500">TV</span></span>
                  <span class="w-2 h-2 rounded-full bg-red-500 animate-pulse"></span>
                </div>
                <span class="text-[10px] text-slate-400 font-medium tracking-wider uppercase">Yönetim Portalı</span>
              </div>
            </div>
            
            <!-- Masaüstü Küçültme/Büyütme Butonu -->
            <button 
              (click)="toggleDesktopSidebar()" 
              title="Menüyü Küçült / Büyüt"
              class="hidden lg:flex text-slate-400 hover:text-white p-1.5 rounded-lg hover:bg-slate-800 transition-colors"
            >
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h7"/>
              </svg>
            </button>

            <!-- Mobil Kapatma (X) Butonu -->
            <button 
              (click)="closeMobileSidebar()" 
              title="Menüyü Kapat"
              class="lg:hidden text-slate-400 hover:text-white p-1.5 rounded-lg hover:bg-slate-800 transition-colors"
            >
              <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
              </svg>
            </button>
          </div>

          <!-- Navigasyon Menüsü -->
          <nav class="p-3 space-y-1 overflow-y-auto custom-scrollbar flex-1">
            <!-- İçerik Yönetimi Başlığı -->
            <div 
              class="px-3 pt-3 pb-1 text-[11px] font-semibold uppercase tracking-wider text-slate-500"
              [class.lg:hidden]="isDesktopCollapsed()"
            >
              İçerik Yönetimi
            </div>
            <div class="hidden my-2 border-t border-slate-800" [class.lg:block]="isDesktopCollapsed()"></div>

            <!-- Dashboard -->
            <a 
              routerLink="/admin/dashboard" 
              (click)="closeMobileSidebar()"
              routerLinkActive="bg-red-600/15 text-red-400 font-semibold border-r-2 border-red-500"
              [title]="'Kontrol Paneli'"
              class="flex items-center gap-3 px-3.5 py-2.5 rounded-lg text-sm text-slate-300 hover:bg-slate-800/70 hover:text-white transition-all group"
              [class.lg:justify-center]="isDesktopCollapsed()"
              [class.lg:px-0]="isDesktopCollapsed()"
            >
              <svg class="w-5 h-5 text-slate-400 group-hover:text-red-400 transition-colors shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"/>
              </svg>
              <span [class.lg:hidden]="isDesktopCollapsed()">Kontrol Paneli</span>
            </a>

            <!-- Haberler -->
            <a 
              routerLink="/admin/news" 
              (click)="closeMobileSidebar()"
              routerLinkActive="bg-red-600/15 text-red-400 font-semibold border-r-2 border-red-500"
              [title]="'Haber Listesi'"
              class="flex items-center gap-3 px-3.5 py-2.5 rounded-lg text-sm text-slate-300 hover:bg-slate-800/70 hover:text-white transition-all group"
              [class.lg:justify-center]="isDesktopCollapsed()"
              [class.lg:px-0]="isDesktopCollapsed()"
            >
              <svg class="w-5 h-5 text-slate-400 group-hover:text-red-400 transition-colors shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 20H5a2 2 0 01-2-2V6a2 2 0 012-2h10a2 2 0 012 2v1m2 13a2 2 0 01-2-2V7m2 13a2 2 0 002-2V9a2 2 0 00-2-2h-2m-4-3H9M7 16h6M7 8h6v4H7V8z"/>
              </svg>
              <span [class.lg:hidden]="isDesktopCollapsed()">Haber Listesi</span>
            </a>

            <!-- Yeni Haber Ekle -->
            <a 
              routerLink="/admin/news/new" 
              (click)="closeMobileSidebar()"
              routerLinkActive="bg-red-600/15 text-red-400 font-semibold border-r-2 border-red-500"
              [title]="'Haber Yaz (Editör)'"
              class="flex items-center gap-3 px-3.5 py-2.5 rounded-lg text-sm text-slate-300 hover:bg-slate-800/70 hover:text-white transition-all group"
              [class.lg:justify-center]="isDesktopCollapsed()"
              [class.lg:px-0]="isDesktopCollapsed()"
            >
              <svg class="w-5 h-5 text-slate-400 group-hover:text-red-400 transition-colors shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/>
              </svg>
              <span [class.lg:hidden]="isDesktopCollapsed()">Haber Yaz (Editör)</span>
            </a>

            <!-- Kategoriler -->
            <a 
              routerLink="/admin/categories" 
              (click)="closeMobileSidebar()"
              routerLinkActive="bg-red-600/15 text-red-400 font-semibold border-r-2 border-red-500"
              [title]="'Kategoriler'"
              class="flex items-center gap-3 px-3.5 py-2.5 rounded-lg text-sm text-slate-300 hover:bg-slate-800/70 hover:text-white transition-all group"
              [class.lg:justify-center]="isDesktopCollapsed()"
              [class.lg:px-0]="isDesktopCollapsed()"
            >
              <svg class="w-5 h-5 text-slate-400 group-hover:text-red-400 transition-colors shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z"/>
              </svg>
              <span [class.lg:hidden]="isDesktopCollapsed()">Kategoriler</span>
            </a>

            <!-- Medya Galerisi -->
            <a 
              routerLink="/admin/media" 
              (click)="closeMobileSidebar()"
              routerLinkActive="bg-red-600/15 text-red-400 font-semibold border-r-2 border-red-500"
              [title]="'Medya Kütüphanesi'"
              class="flex items-center gap-3 px-3.5 py-2.5 rounded-lg text-sm text-slate-300 hover:bg-slate-800/70 hover:text-white transition-all group"
              [class.lg:justify-center]="isDesktopCollapsed()"
              [class.lg:px-0]="isDesktopCollapsed()"
            >
              <svg class="w-5 h-5 text-slate-400 group-hover:text-red-400 transition-colors shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"/>
              </svg>
              <span [class.lg:hidden]="isDesktopCollapsed()">Medya Kütüphanesi</span>
            </a>

            <!-- Yorumlar -->
            <a 
              routerLink="/admin/comments" 
              (click)="closeMobileSidebar()"
              routerLinkActive="bg-red-600/15 text-red-400 font-semibold border-r-2 border-red-500"
              [title]="'Yorum Moderasyonu'"
              class="flex items-center gap-3 px-3.5 py-2.5 rounded-lg text-sm text-slate-300 hover:bg-slate-800/70 hover:text-white transition-all group"
              [class.lg:justify-center]="isDesktopCollapsed()"
              [class.lg:px-0]="isDesktopCollapsed()"
            >
              <svg class="w-5 h-5 text-slate-400 group-hover:text-red-400 transition-colors shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"/>
              </svg>
              <span [class.lg:hidden]="isDesktopCollapsed()">Yorum Moderasyonu</span>
            </a>

            <!-- Sistem & Yetki Başlığı -->
            <div 
              class="px-3 pt-4 pb-1 text-[11px] font-semibold uppercase tracking-wider text-slate-500"
              [class.lg:hidden]="isDesktopCollapsed()"
            >
              Sistem & Yetki
            </div>
            <div class="hidden my-2 border-t border-slate-800" [class.lg:block]="isDesktopCollapsed()"></div>

            <!-- Sistem Logları -->
            <a 
              routerLink="/admin/logs" 
              (click)="closeMobileSidebar()"
              routerLinkActive="bg-red-600/15 text-red-400 font-semibold border-r-2 border-red-500"
              [title]="'Sistem Logları'"
              class="flex items-center gap-3 px-3.5 py-2.5 rounded-lg text-sm text-slate-300 hover:bg-slate-800/70 hover:text-white transition-all group"
              [class.lg:justify-center]="isDesktopCollapsed()"
              [class.lg:px-0]="isDesktopCollapsed()"
            >
              <svg class="w-5 h-5 text-slate-400 group-hover:text-red-400 transition-colors shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
              </svg>
              <span [class.lg:hidden]="isDesktopCollapsed()">Sistem Logları</span>
            </a>

            <!-- Yazarlar & Kullanıcılar -->
            <a 
              routerLink="/admin/users" 
              (click)="closeMobileSidebar()"
              routerLinkActive="bg-red-600/15 text-red-400 font-semibold border-r-2 border-red-500"
              [title]="'Yazarlar & Yetkiler'"
              class="flex items-center gap-3 px-3.5 py-2.5 rounded-lg text-sm text-slate-300 hover:bg-slate-800/70 hover:text-white transition-all group"
              [class.lg:justify-center]="isDesktopCollapsed()"
              [class.lg:px-0]="isDesktopCollapsed()"
            >
              <svg class="w-5 h-5 text-slate-400 group-hover:text-red-400 transition-colors shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"/>
              </svg>
              <span [class.lg:hidden]="isDesktopCollapsed()">Yazarlar & Yetkiler</span>
            </a>

            <!-- Ayarlar -->
            <a 
              routerLink="/admin/settings" 
              (click)="closeMobileSidebar()"
              routerLinkActive="bg-red-600/15 text-red-400 font-semibold border-r-2 border-red-500"
              [title]="'Site Ayarları & SEO'"
              class="flex items-center gap-3 px-3.5 py-2.5 rounded-lg text-sm text-slate-300 hover:bg-slate-800/70 hover:text-white transition-all group"
              [class.lg:justify-center]="isDesktopCollapsed()"
              [class.lg:px-0]="isDesktopCollapsed()"
            >
              <svg class="w-5 h-5 text-slate-400 group-hover:text-red-400 transition-colors shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"/>
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
              </svg>
              <span [class.lg:hidden]="isDesktopCollapsed()">Site Ayarları & SEO</span>
            </a>
          </nav>
        </div>

        <!-- Kullanıcı Bilgisi & Çıkış Yap (Alt Kısım) -->
        <div class="p-3 border-t border-slate-800 bg-slate-950/40 shrink-0">
          <div 
            class="flex items-center justify-between gap-3 p-2 rounded-xl bg-slate-800/50"
            [class.lg:justify-center]="isDesktopCollapsed()"
            [class.lg:p-1.5]="isDesktopCollapsed()"
          >
            <div class="flex items-center gap-2.5 overflow-hidden">
              <div 
                [title]="authService.currentUser()?.fullName || 'Yönetici'"
                class="w-8 h-8 rounded-full bg-slate-700 text-slate-200 font-bold flex items-center justify-center text-xs shrink-0 border border-slate-600"
              >
                {{ authService.currentUser()?.fullName?.charAt(0) || 'A' }}
              </div>
              <div class="flex flex-col min-w-0" [class.lg:hidden]="isDesktopCollapsed()">
                <span class="text-xs font-semibold text-white truncate">{{ authService.currentUser()?.fullName || 'Yönetici' }}</span>
                <span class="text-[10px] text-slate-400 truncate">{{ authService.currentUser()?.role || 'SuperAdmin' }}</span>
              </div>
            </div>
            <button 
              (click)="authService.logout()" 
              title="Çıkış Yap"
              class="text-slate-400 hover:text-red-400 transition-colors p-1.5 rounded-lg hover:bg-slate-800 shrink-0"
              [class.lg:hidden]="isDesktopCollapsed()"
            >
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"/>
              </svg>
            </button>
          </div>
        </div>
      </aside>

      <!-- SAĞ İÇERİK ALANI -->
      <div class="flex-1 flex flex-col min-w-0 overflow-hidden bg-slate-50 w-full">
        <!-- TOPBAR (Üst Panel) -->
        <header class="h-16 bg-white border-b border-slate-200 px-3 sm:px-6 flex items-center justify-between shrink-0 z-20 shadow-xs gap-2 sm:gap-4">
          <!-- Sol Kısım: Mobil Menü Butonu + Arama -->
          <div class="flex items-center gap-2 sm:gap-3 flex-1 min-w-0 max-w-md">
            <!-- Mobil Hamburger Açma Butonu -->
            <button 
              (click)="openMobileSidebar()" 
              title="Menüyü Aç"
              class="lg:hidden p-2 text-slate-600 hover:text-slate-900 rounded-lg hover:bg-slate-100 transition-colors shrink-0"
            >
              <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"/>
              </svg>
            </button>

            <!-- Arama Çubuğu -->
            <div class="relative w-full">
              <span class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-slate-400">
                <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
                </svg>
              </span>
              <input 
                type="text" 
                placeholder="Haber, etiket veya yazar ara..." 
                class="w-full pl-9 pr-3 py-1.5 text-xs bg-slate-100/70 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 transition-all placeholder:text-slate-400"
              />
            </div>
          </div>

          <!-- Sağ Aksiyonlar -->
          <div class="flex items-center gap-2 sm:gap-3 shrink-0">
            <!-- Sosyal Medya Linkleri -->
            <div class="flex items-center gap-0.5 sm:gap-1 text-slate-500 bg-slate-50 border border-slate-200/80 rounded-lg p-1">
              <!-- YouTube -->
              <a 
                href="https://www.youtube.com/c/kursutv" 
                target="_blank" 
                rel="noopener noreferrer" 
                title="Kürsü TV YouTube"
                class="p-1.5 hover:text-red-600 hover:bg-white rounded-md transition-all duration-150"
              >
                <svg class="w-3.5 h-3.5 fill-current" viewBox="0 0 24 24">
                  <path d="M23.498 6.186a3.016 3.016 0 0 0-2.122-2.136C19.505 3.545 12 3.545 12 3.545s-7.505 0-9.377.505A3.017 3.017 0 0 0 .502 6.186C0 8.07 0 12 0 12s0 3.93.502 5.814a3.016 3.016 0 0 0 2.122 2.136c1.871.505 9.376.505 9.376.505s7.505 0 9.377-.505a3.015 3.015 0 0 0 2.122-2.136C24 15.93 24 12 24 12s0-3.93-.502-5.814zM9.545 15.568V8.432L15.818 12l-6.273 3.568z"/>
                </svg>
              </a>
              <!-- X (Twitter) -->
              <a 
                href="https://x.com/kursu_tv" 
                target="_blank" 
                rel="noopener noreferrer" 
                title="Kürsü TV X (Twitter)"
                class="p-1.5 hover:text-slate-900 hover:bg-white rounded-md transition-all duration-150"
              >
                <svg class="w-3.5 h-3.5 fill-current" viewBox="0 0 24 24">
                  <path d="M18.244 2.25h3.308l-7.227 8.26 8.502 11.24H16.17l-5.214-6.817L4.99 21.75H1.68l7.73-8.835L1.254 2.25H8.08l4.713 6.231zm-1.161 17.52h1.833L7.084 4.126H5.117z"/>
                </svg>
              </a>
              <!-- Instagram -->
              <a 
                href="https://www.instagram.com/kursu.tv" 
                target="_blank" 
                rel="noopener noreferrer" 
                title="Kürsü TV Instagram"
                class="p-1.5 hover:text-pink-600 hover:bg-white rounded-md transition-all duration-150"
              >
                <svg class="w-3.5 h-3.5 fill-current" viewBox="0 0 24 24">
                  <path d="M12 2.163c3.204 0 3.584.012 4.85.07 3.252.148 4.771 1.691 4.919 4.919.058 1.265.069 1.645.069 4.849 0 3.205-.012 3.584-.069 4.849-.149 3.225-1.664 4.771-4.919 4.919-1.266.058-1.644.07-4.85.07-3.204 0-3.584-.012-4.849-.07-3.26-.149-4.771-1.699-4.919-4.92-.058-1.265-.07-1.644-.07-4.849 0-3.204.013-3.583.07-4.849.149-3.227 1.664-4.771 4.919-4.919 1.266-.057 1.645-.069 4.849-.069zm0-2.163c-3.259 0-3.667.014-4.947.072-4.358.2-6.78 2.618-6.98 6.98-.059 1.281-.073 1.689-.073 4.948 0 3.259.014 3.668.072 4.948.2 4.358 2.618 6.78 6.98 6.98 1.281.058 1.689.072 4.948.072 3.259 0 3.668-.014 4.948-.072 4.354-.2 6.782-2.618 6.979-6.98.059-1.28.073-1.689.073-4.948 0-3.259-.014-3.667-.072-4.947-.196-4.354-2.617-6.78-6.979-6.98-1.281-.059-1.69-.073-4.949-.073zm0 5.838c-3.403 0-6.162 2.759-6.162 6.162s2.759 6.163 6.162 6.163 6.162-2.759 6.162-6.163c0-3.403-2.759-6.162-6.162-6.162zm0 10.162c-2.209 0-4-1.79-4-4 0-2.209 1.791-4 4-4s4 1.791 4 4c0 2.21-1.791 4-4 4zm6.406-11.845c-.796 0-1.441.645-1.441 1.44s.645 1.44 1.441 1.44c.795 0 1.439-.645 1.439-1.44s-.644-1.44-1.439-1.44z"/>
                </svg>
              </a>
              <!-- Facebook -->
              <a 
                href="https://www.facebook.com/kursutv" 
                target="_blank" 
                rel="noopener noreferrer" 
                title="Kürsü TV Facebook"
                class="p-1.5 hover:text-blue-600 hover:bg-white rounded-md transition-all duration-150"
              >
                <svg class="w-3.5 h-3.5 fill-current" viewBox="0 0 24 24">
                  <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z"/>
                </svg>
              </a>
            </div>

            <!-- Canlı Siteyi Aç -->
            <a 
              href="http://localhost:5248" 
              target="_blank" 
              class="hidden md:inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium text-slate-600 hover:text-slate-900 bg-slate-100 hover:bg-slate-200/80 rounded-lg transition-colors"
            >
              <svg class="w-3.5 h-3.5 text-slate-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"/>
              </svg>
              Siteyi Gör
            </a>

            <!-- %10 VURGU: Yeni Haber Ekle Butonu -->
            <a 
              routerLink="/admin/news/new"
              class="inline-flex items-center gap-1.5 px-3 sm:px-4 py-2 bg-gradient-to-r from-red-600 to-rose-600 hover:from-red-700 hover:to-rose-700 text-white text-xs font-semibold rounded-lg shadow-sm shadow-red-500/20 hover:shadow-md transition-all transform active:scale-95"
            >
              <svg class="w-4 h-4 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
              </svg>
              <span class="hidden sm:inline">Yeni Haber Ekle</span>
              <span class="sm:hidden">Yeni</span>
            </a>
          </div>
        </header>

        <!-- SAYFA İÇERİĞİ -->
        <main class="flex-1 overflow-y-auto custom-scrollbar p-3 sm:p-6">
          <router-outlet></router-outlet>
        </main>
      </div>
    </div>

    <!-- Global Toast Bildirimleri -->
    <app-toast-container></app-toast-container>
  `
})
export class AdminLayoutComponent {
  isDesktopCollapsed = signal<boolean>(false);
  isMobileOpen = signal<boolean>(false);

  constructor(public authService: AuthService) {}

  toggleDesktopSidebar() {
    this.isDesktopCollapsed.update(v => !v);
  }

  toggleMobileSidebar() {
    this.isMobileOpen.update(v => !v);
  }

  openMobileSidebar() {
    this.isMobileOpen.set(true);
  }

  closeMobileSidebar() {
    this.isMobileOpen.set(false);
  }
}
