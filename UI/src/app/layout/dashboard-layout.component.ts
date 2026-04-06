import { Component, OnInit, signal, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ScriptsService } from '../services/scripts.service';
import {
  BellNotificationsService,
  BellNotification,
} from '../services/bell-notifications.service';

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
  private router = inject(Router);
  protected bell = inject(BellNotificationsService);
  sidebarOpen = signal(false);
  notificationsOpen = signal(false);

  ngOnInit(): void {
    if (this.authService.getToken()) {
      this.scriptsService.loadAll();
      this.bell.refresh();
    }
  }

  logout(): void {
    this.authService.logout();
  }

  toggleSidebar(): void {
    this.sidebarOpen.update((v) => !v);
  }

  closeSidebar(): void {
    this.sidebarOpen.set(false);
  }

  toggleNotifications(): void {
    this.notificationsOpen.update((v) => {
      const next = !v;
      if (next) this.bell.refresh();
      return next;
    });
  }

  closeNotifications(): void {
    this.notificationsOpen.set(false);
  }

  markAsRead(n: BellNotification): void {
    this.bell.markAsRead(n);
    if (n.link) {
      this.router.navigateByUrl(n.link);
      this.closeNotifications();
    }
  }
}
