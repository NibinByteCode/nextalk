import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login', standalone: true, imports: [FormsModule, RouterLink],
  template: `<main class="auth-card"><span class="eyebrow">WELCOME BACK</span><h1>Sign in to NexTalk</h1>
    <input [(ngModel)]="email" type="email" placeholder="Email">
    <input [(ngModel)]="password" type="password" placeholder="Password">
    <button class="primary" (click)="login()">Sign in</button><p class="error">{{ error }}</p>
    <a routerLink="/register">Create an account</a></main>`
})
export class LoginComponent {
  email = ''; password = ''; error = '';
  constructor(private readonly auth: AuthService, private readonly router: Router) {}
  login() {
    this.error = '';
    this.auth.login(this.email, this.password).subscribe({
      next: () => void this.router.navigateByUrl('/chat'),
      error: err => this.error = err.error?.message ?? 'Login failed'
    });
  }
}
