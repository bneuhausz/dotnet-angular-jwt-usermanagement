import { httpResource } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Role } from "../interfaces/role";

@Injectable()
export class RolesService {
  roles = httpResource<Role[]>(
    () => {
      return '/api/roles';
    },
    {
      defaultValue: [],
    }
  );
}