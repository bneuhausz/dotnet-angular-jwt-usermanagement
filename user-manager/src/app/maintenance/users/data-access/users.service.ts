import { Injectable } from "@angular/core";
import { httpResource } from "@angular/common/http";
import { User } from "../interfaces/user";

@Injectable()
export class UsersService {
  users = httpResource<User[]>(
    () => {
      return '/api/users';
    },
    {
      defaultValue: [],
    }
  );
}