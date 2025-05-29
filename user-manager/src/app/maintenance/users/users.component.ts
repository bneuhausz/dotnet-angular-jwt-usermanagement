import { Component, inject } from "@angular/core";
import { UsersService } from "./data-access/users.service";
import { MatTableModule } from '@angular/material/table';
import { ReactiveFormsModule, Validators, FormBuilder } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatCheckboxModule } from "@angular/material/checkbox";

@Component({
  selector: "app-users",
  imports: [MatTableModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatCheckboxModule],
  providers: [UsersService],
  template: `
    <h1>Users Management</h1>

    <form [formGroup]="form" class="create-form" (ngSubmit)="usersService.create$.next(form.getRawValue())">
      <mat-form-field>
        <mat-label>User Name</mat-label>
        <input matInput formControlName="userName" type="name" autocomplete="name" />
      </mat-form-field>

      <mat-form-field>
        <mat-label>Email</mat-label>
        <input matInput formControlName="email" type="email" autocomplete="email" />
      </mat-form-field>

      <mat-form-field>
        <mat-label>Password</mat-label>
        <input matInput formControlName="password" type="password" autocomplete="current-password" />
      </mat-form-field>

      <button mat-raised-button class="create-button" type="submit" [disabled]="form.invalid">Create User</button>
    </form>

    @if (!usersService.users.isLoading()) {
      <table mat-table [dataSource]="usersService.users.value()" class="mat-elevation-z8">
        <ng-container matColumnDef="userName">
          <th mat-header-cell *matHeaderCellDef>User Name</th>
          <td mat-cell *matCellDef="let user">{{ user.userName }}</td>
        </ng-container>

        <ng-container matColumnDef="email">
          <th mat-header-cell *matHeaderCellDef>Email</th>
          <td mat-cell *matCellDef="let user">{{ user.email }}</td>
        </ng-container>

        <ng-container matColumnDef="isDeleted">
          <th mat-header-cell *matHeaderCellDef>Deleted</th>
          <td mat-cell *matCellDef="let user">{{ user.isDeleted }}</td>
        </ng-container>

        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef></th>
          <td mat-cell *matCellDef="let user">
            <button
              mat-raised-button
              (click)="usersService.toggleDeleted$.next(user.id)"
            >
              {{ user.isDeleted ? 'Restore' : 'Delete' }}
            </button>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;" (click)="usersService.selectedUserId.set(row.id)"></tr>
      </table>
    }
    @else {
      <p>Loading users...</p>
    }

    @if (usersService.userRoles.value().length) {
      <h2>User Roles</h2>
      @for (item of usersService.userRoles.value(); track item.id) {
        <ul>
          <li>
            <mat-checkbox [checked]="item.isAssigned" (change)="usersService.toggleRole$.next(item.id)">
              {{ item.name }}
            </mat-checkbox>
          </li>
        </ul>
      }
    }
  `,
  styles: [`
    .create-form {
      display: flex;
      gap: 10px;
      justify-content: center;
    }

    .create-button {
      margin-top: 10px;
    }

    .mat-column-actions {
      width: 140px;
      text-align: center;
    }

    ul {
      list-style-type: none;
      padding: 0;
    }
  `]
})
export default class UsersComponent {
  usersService = inject(UsersService);
  fb = inject(FormBuilder);
  
  displayedColumns: string[] = ['userName', 'email', 'isDeleted', 'actions'];

  form = this.fb.nonNullable.group({
    userName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });
}