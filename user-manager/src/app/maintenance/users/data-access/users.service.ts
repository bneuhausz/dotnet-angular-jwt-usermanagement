import { inject, Injectable, signal } from "@angular/core";
import { HttpClient, httpResource } from "@angular/common/http";
import { User } from "../interfaces/user";
import { Subject, switchMap } from "rxjs";
import { UserRole } from "../interfaces/user-role";

type CreateUser = {
  userName: string;
  email: string;
  password: string;
}

@Injectable()
export class UsersService {
  readonly #http = inject(HttpClient);
  selectedUserId = signal<string | null>(null);
  
  users = httpResource<User[]>(
    () => {
      return '/api/users';
    },
    {
      defaultValue: [],
    }
  );

  userRoles = httpResource<UserRole[]>(
    () => {
      const id = this.selectedUserId();
      if (!id) {
        return;
      }
      return `/api/users/${this.selectedUserId()}/roles`;
    },
    {
      defaultValue: [],
    }
  );

  readonly create$ = new Subject<CreateUser>();
  readonly #create$ = this.create$.pipe(
    switchMap((user) => this.createUser(user))
  );
  
  readonly toggleDeleted$ = new Subject<string>();
  readonly #toggleDeleted$ = this.toggleDeleted$.pipe(
    switchMap((id) => this.toggleDeleted(id))
  );

  readonly toggleRole$ = new Subject<string>();
  readonly #toggleRole$ = this.toggleRole$.pipe(
    switchMap((id) => this.toggleRole(id))
  );

  constructor() {
    this.#create$.subscribe(
      () => this.users.reload(),
    );

    this.#toggleDeleted$.subscribe(
      () => this.users.reload(),
    );

    this.#toggleRole$.subscribe(
      () => this.userRoles.reload(),
    );
  }

  private createUser(user: CreateUser) {
    return this.#http.post('/api/users', user);
  }

  private toggleDeleted(id: string) {
    return this.#http.put(`/api/users/${id}/toggledeleted`, {});
  }

  private toggleRole(roleId: string) {
    return this.#http.put(`/api/users/${this.selectedUserId()}/togglerole/${roleId}`, {});
  }
}