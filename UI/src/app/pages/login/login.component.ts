import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [FormsModule],
    templateUrl: './login.component.html',
    styleUrl: './login.component.css'
})
export class LoginComponent {
    private router = inject(Router);

    password = '';
    showPassword = false;
    rememberPassword = false;
    error = '';
    loading = false;

    login() {
        this.loading = true;
        this.error = '';

        // Simular delay de red
        setTimeout(() => {
            this.loading = false;
            // Aquí iría la validación real contra un servicio
            if (this.password === '123456') { // Mock check
                this.router.navigate(['/']);
            } else {
                this.error = 'Contraseña incorrecta';
            }
        }, 1000);
    }
}
