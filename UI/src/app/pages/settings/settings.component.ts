import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'app-settings',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './settings.component.html',
    styleUrl: './settings.component.css'
})
export class SettingsComponent {
    requireAuth = true;
    oldPassword = '';
    newPassword = '';
    confirmPassword = '';

    message = '';
    messageType: 'success' | 'error' = 'success';

    saveAuthSettings() {
        // Aquí se llamaría al servicio para guardar la configuración de autenticación
        console.log('Require Auth:', this.requireAuth);
        this.showMessage('Configuración de autenticación guardada', 'success');
    }

    changePassword() {
        if (this.newPassword !== this.confirmPassword) {
            this.showMessage('Las contraseñas no coinciden', 'error');
            return;
        }

        // Aquí se llamaría al servicio para cambiar la contraseña
        console.log('Changing password...');
        this.showMessage('Contraseña actualizada correctamente', 'success');

        this.oldPassword = '';
        this.newPassword = '';
        this.confirmPassword = '';
    }

    private showMessage(msg: string, type: 'success' | 'error') {
        this.message = msg;
        this.messageType = type;
        setTimeout(() => this.message = '', 3000);
    }
}
