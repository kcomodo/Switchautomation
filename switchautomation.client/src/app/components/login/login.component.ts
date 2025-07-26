import { Component } from '@angular/core';

import { Router } from '@angular/router';
import { DeviceService } from '../../services/device.service';
import { MatSnackBar } from '@angular/material/snack-bar';
@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  username: string = '';
  password: string = '';

  constructor(
    private router: Router,
    private deviceService: DeviceService,
    private snackBar: MatSnackBar
  ) { }

  onLogin(): void {
    this.deviceService.validateLogin(this.username, this.password).subscribe({
      next: (result) => {
        console.log(result);
        if (result === true) {
          this.snackBar.open('Login successful!', 'Close', {
            duration: 3000,
            panelClass: ['snackbar-success']
          });
          this.router.navigate(['/home']); // 👈 navigate to home/dashboard
        } else {
          this.snackBar.open('Invalid credentials.', 'Close', {
            duration: 3000,
            panelClass: ['snackbar-error']
          });
        }
      },
      error: (err) => {
        this.snackBar.open('Login failed. Try again later.', 'Close', {
          duration: 3000,
          panelClass: ['snackbar-error']
        });
      }
    });
  }
}
