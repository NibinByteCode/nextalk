import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-register', standalone: true, imports: [FormsModule, RouterLink],
  template: `<main class="auth-card"><span class="eyebrow">GET STARTED</span><h1>Create your account</h1>
    <input [(ngModel)]="userName" placeholder="Username">
    <input [(ngModel)]="email" type="email" placeholder="Email">
    <input [(ngModel)]="password" type="password" placeholder="Password (10+ characters)">
    <button class="primary" (click)="register()">Create account</button><p class="error">{{ error }}</p>
    <a routerLink="/login">Already have an account?</a></main>`
})
export class RegisterComponent {
  userName = ''; email = ''; password = ''; error = '';
  constructor(private readonly auth: AuthService, private readonly router: Router) {}
  register() {
    this.error = '';
    this.auth.register(this.userName, this.email, this.password).subscribe({
      next: () => void this.router.navigateByUrl('/chat'),
      error: err => this.error = err.error?.message ?? 'Registration failed'
    });
  }
}
