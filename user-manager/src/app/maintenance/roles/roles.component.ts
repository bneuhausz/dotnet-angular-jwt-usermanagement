import { Component, inject } from "@angular/core";
import { RolesService } from "./data-access/roles.service";
import { MatTableModule } from "@angular/material/table";

@Component({
  selector: "app-roles",
  providers: [RolesService],
  imports: [MatTableModule],
  template: `
    <h1>Users Management</h1>
    @if (!usersService.roles.isLoading()) {
      <table mat-table [dataSource]="usersService.roles.value()" class="mat-elevation-z8">
        <ng-container matColumnDef="name">
          <th mat-header-cell *matHeaderCellDef>Name</th>
          <td mat-cell *matCellDef="let element">{{ element.name }}</td>
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
export default class RolesComponent {
  usersService = inject(RolesService);

  displayedColumns: string[] = ['name', 'isDeleted'];
}