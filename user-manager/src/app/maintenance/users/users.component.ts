import { Component, inject } from "@angular/core";
import { UsersService } from "./data-access/users.service";
import { MatTableModule } from '@angular/material/table';

@Component({
  selector: "app-users",
  imports: [MatTableModule],
  providers: [UsersService],
  template: `
    <h1>Users Management</h1>
    @if (!usersService.users.isLoading()) {
      <table mat-table [dataSource]="usersService.users.value()" class="mat-elevation-z8">
        <ng-container matColumnDef="userName">
          <th mat-header-cell *matHeaderCellDef>User Name</th>
          <td mat-cell *matCellDef="let element">{{ element.userName }}</td>
        </ng-container>

        <ng-container matColumnDef="email">
          <th mat-header-cell *matHeaderCellDef>Email</th>
          <td mat-cell *matCellDef="let element">{{ element.email }}</td>
        </ng-container>

        <ng-container matColumnDef="isDeleted">
          <th mat-header-cell *matHeaderCellDef>Deleted</th>
          <td mat-cell *matCellDef="let element">{{ element.isDeleted }}</td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
      </table>
    }
    @else {
      <p>Loading users...</p>
    }
  `,
})
export default class UsersComponent {
  usersService = inject(UsersService);

  displayedColumns: string[] = ['userName', 'email', 'isDeleted'];
}