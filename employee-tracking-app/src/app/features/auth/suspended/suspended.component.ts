import { Component, inject } from '@angular/core';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-suspended',
  standalone: true,
  templateUrl: './suspended.component.html',
  styleUrl: '../login/login.component.scss'
})
export class SuspendedComponent {
  private readonly auth = inject(AuthService);

  signOut(): void {
    this.auth.logout();
  }
}
