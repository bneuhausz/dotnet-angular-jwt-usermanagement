import { HttpClient, httpResource } from "@angular/common/http";
import { computed, inject, Injectable, signal } from "@angular/core";
import { Role } from "../interfaces/role";
import { Subject, switchMap } from "rxjs";
import { RolePermission } from "../interfaces/role-permission";
import { PermissionNode } from "../interfaces/permission-node";

@Injectable()
export class RolesService {
  readonly #http = inject(HttpClient);
  selectedRoleId = signal<string | null>(null);

  readonly roles = httpResource<Role[]>(
    () => {
      return '/api/roles';
    },
    {
      defaultValue: [],
    }
  );

  rolePermissions = httpResource<RolePermission[]>(
    () => {
      const id = this.selectedRoleId();
      if (!id) {
        return;
      }
      return `/api/roles/${this.selectedRoleId()}/permissions`;
    },
    {
      defaultValue: [],
    }
  );

  permissionTree = computed(() => {
    const map = new Map<string, PermissionNode>();
    const roots: PermissionNode[] = [];
    const permissions = this.rolePermissions.value();

    permissions.forEach(p => map.set(p.id, { ...p, children: [] }));
    permissions.forEach(p => {
      const node = map.get(p.id)!;
      if (p.parentPermissionId) {
        map.get(p.parentPermissionId)?.children?.push(node);
      } else {
        roots.push(node);
      }
    });

    return roots;
  });

  readonly create$ = new Subject<string>();
  readonly #create$ = this.create$.pipe(
    switchMap((name) => this.createRole(name))
  );

  readonly toggleDeleted$ = new Subject<string>();
  readonly #toggleDeleted$ = this.toggleDeleted$.pipe(
    switchMap((id) => this.toggleDeleted(id))
  );

  readonly togglePermission$ = new Subject<string>();
  readonly #togglePermission$ = this.togglePermission$.pipe(
    switchMap((id) => this.togglePermission(id))
  );

  constructor() {
    this.#create$.subscribe(
      () => this.roles.reload(),
    );

    this.#toggleDeleted$.subscribe(
      () => this.roles.reload(),
    );

    this.#togglePermission$.subscribe();
  }

  private createRole(name: string) {
    return this.#http.post('/api/roles', { name })
  }

  private toggleDeleted(id: string) {
    return this.#http.put(`/api/roles/${id}/toggledeleted`, {});
  }

  private togglePermission(permissionId: string) {
    return this.#http.put(`/api/roles/${this.selectedRoleId()}/togglepermission/${permissionId}`, {});
  }
}