import { Menu } from "./menu";

export interface LoggedInUser {
  id: string;
  name: string;
  email: string;
  token: string;
  permissions: string[];
  menus: Menu[]
}