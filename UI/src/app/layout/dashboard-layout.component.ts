import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ScriptsService } from '../services/scripts.service';

export interface LayoutNotification {
  id: string;
  title: string;
  message: string;
  date: string;
  read: boolean;
}

@Component({
  selector: 'app-dashboard-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './dashboard-layout.component.html',
  styleUrl: './dashboard-layout.component.css',
})
export class DashboardLayoutComponent implements OnInit {
  private authService = inject(AuthService);
  private scriptsService = inject(ScriptsService);
  sidebarOpen = signal(false);
  notificationsOpen = signal(false);

  ngOnInit(): void {
    if (this.authService.getToken()) {
      this.scriptsService.loadAll();
    }
  }

  logout(): void {
    this.authService.logout();
  }

  /** Notificaciones; en el futuro puede venir de un NotificationService */
  notifications = signal<LayoutNotification[]>([
    { id: '1', title: 'Trabajo completado', message: 'Backup diario documentos finalizó correctamente.', date: 'Hace 2 h', read: false },
    { id: '2', title: 'Recordatorio', message: 'Revisa los destinos configurados.', date: 'Ayer', read: true },
  ]);

  unreadCount = computed(() => this.notifications().filter((n) => !n.read).length);

  toggleSidebar(): void {
    this.sidebarOpen.update((v) => !v);
  }

  closeSidebar(): void {
    this.sidebarOpen.set(false);
  }

  toggleNotifications(): void {
    this.notificationsOpen.update((v) => !v);
  }

  closeNotifications(): void {
    this.notificationsOpen.set(false);
  }

  markAsRead(n: LayoutNotification): void {
    this.notifications.update((list) =>
      list.map((item) => (item.id === n.id ? { ...item, read: true } : item))
    );
  }
}
