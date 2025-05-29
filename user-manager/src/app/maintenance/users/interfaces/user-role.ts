import { Role } from "../../roles/interfaces/role";

export interface UserRole extends Role {
  isAssigned: boolean;
}