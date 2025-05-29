import { Permission } from "./permission";

export interface RolePermission extends Permission {
  isAssigned: boolean;
}