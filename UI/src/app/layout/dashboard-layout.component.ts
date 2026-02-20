import { Component, signal, computed } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

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
export class DashboardLayoutComponent {
  sidebarOpen = signal(false);
  notificationsOpen = signal(false);

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
