import { RolePermission } from "./role-permission";

export interface PermissionNode extends RolePermission {
  children: PermissionNode[];
}