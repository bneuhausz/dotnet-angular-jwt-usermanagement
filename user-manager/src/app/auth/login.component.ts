import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from './data-access/auth.service';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
    <mat-card class="login-card" style="width: 400px;">
      <h2>Login</h2>
      <form [formGroup]="form" class="login-form" (ngSubmit)="authService.login$.next(form.getRawValue())">
        <mat-form-field>
          <mat-label>Email</mat-label>
          <input matInput formControlName="email" type="email" autocomplete="email" />
          @if (form.get('email')?.hasError('required')) {
            <mat-error>Email is required</mat-error>
          }
        </mat-form-field>

        <mat-form-field>
          <mat-label>Password</mat-label>
          <input matInput formControlName="password" type="password" autocomplete="current-password" />
          @if (form.get('password')?.hasError('password')) {
            <mat-error>Password is required</mat-error>
          }
        </mat-form-field>

        <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid">Login</button>
      </form>
    </mat-card>
  `,
  styles: [`
    :host {
      display: flex;
      flex-direction: column;
      align-items: center;
      height: 100%;
    }

    .login-card {
      width: 400px;
      padding: 20px;
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .login-form {
      display: flex;
      flex-direction: column;
      width: 100%;
    }
  `],
})
export default class LoginComponent {
  private readonly fb = inject(FormBuilder);
  readonly authService = inject(AuthService);

  form = this.fb.nonNullable.group({
    email: ['', Validators.required],
    password: ['', Validators.required],
  });

  onSubmit(event: any) {
    console.log('Form submitted:', this.form.value);
  }
}