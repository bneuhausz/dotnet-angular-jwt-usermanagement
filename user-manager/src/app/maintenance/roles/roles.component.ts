import { Component, inject } from "@angular/core";
import { RolesService } from "./data-access/roles.service";
import { MatTableModule } from "@angular/material/table";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatTreeModule } from "@angular/material/tree";
import { PermissionNode } from "./interfaces/permission-node";

@Component({
  selector: "app-roles",
  providers: [RolesService],
  imports: [MatTableModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatCheckboxModule, MatTreeModule, MatIconModule
  ],
  template: `
    <h1>Roles Management</h1>

    <form [formGroup]="form" class="create-form" (ngSubmit)="rolesService.create$.next(form.controls.name.value!)">
      <mat-form-field>
        <mat-label>Name</mat-label>
        <input matInput formControlName="name" type="name" autocomplete="name" />
      </mat-form-field>

      <button mat-raised-button class="create-button" type="submit" [disabled]="form.invalid">Create Role</button>
    </form>

    @if (!rolesService.roles.isLoading()) {
      <table mat-table [dataSource]="rolesService.roles.value()" class="mat-elevation-z8">
        <ng-container matColumnDef="name">
          <th mat-header-cell *matHeaderCellDef>Name</th>
          <td mat-cell *matCellDef="let role">{{ role.name }}</td>
        </ng-container>

        <ng-container matColumnDef="isDeleted">
          <th mat-header-cell *matHeaderCellDef>Deleted</th>
          <td mat-cell *matCellDef="let role">{{ role.isDeleted }}</td>
        </ng-container>

        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef></th>
          <td mat-cell *matCellDef="let role">
            <button
              mat-raised-button
              (click)="rolesService.toggleDeleted$.next(role.id)"
            >
              {{ role.isDeleted ? 'Restore' : 'Delete' }}
            </button>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;" (click)="rolesService.selectedRoleId.set(row.id)"></tr>
      </table>
    }
    @else {
      <p>Loading roles...</p>
    }

    @if (rolesService.permissionTree().length) {
      <h2>User Roles</h2>

      <mat-tree
        #tree
        [dataSource]="rolesService.permissionTree()"
        [childrenAccessor]="getChildren"
        class="tree"
      >
        <mat-nested-tree-node *matTreeNodeDef="let node">
          <span>
            <mat-checkbox [checked]="node.isAssigned" (change)="togglePermission(node)">
            </mat-checkbox>
            {{ node.name }}
          </span>
        </mat-nested-tree-node>
        <mat-nested-tree-node *matTreeNodeDef="let node; when: hasChildren">
          <div class="mat-tree-node">
            <button mat-icon-button matTreeNodeToggle
                [attr.aria-label]="'Toggle ' + node.name">
              <mat-icon>
                {{tree.isExpanded(node) ? 'expand_more' : 'chevron_right'}}
              </mat-icon>
            </button>
            <mat-checkbox [checked]="node.isAssigned" (change)="togglePermission(node)">
            </mat-checkbox>
            {{ node.name }}
          </div>
          <div [class.tree-invisible]="!tree.isExpanded(node)"
              role="group">
            <ng-container matTreeNodeOutlet cl></ng-container>
          </div>
        </mat-nested-tree-node>
      </mat-tree>
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

    .tree-invisible {
      display: none;
    }

    .tree ul,
    .tree li {
      margin-top: 0;
      margin-bottom: 0;
      list-style-type: none;
    }

    .tree .mat-nested-tree-node div[role=group] {
      padding-left: 40px;
    }

    .tree div[role=group] > .mat-nested-tree-node {
      padding-left: 40px;
      display: flex;
      flex-direction: column;
    }
  `],
})
export default class RolesComponent {
  rolesService = inject(RolesService);

  displayedColumns: string[] = ['name', 'isDeleted', 'actions'];

  form = new FormGroup({
    name: new FormControl('', Validators.required),
  });

  readonly getChildren = (node: PermissionNode) => node.children;

  hasChildren = (_: number, node: PermissionNode) => !!node.children?.length;

  togglePermission(node: PermissionNode) {
    this.rolesService.togglePermission$.next(node.id);
  }
}