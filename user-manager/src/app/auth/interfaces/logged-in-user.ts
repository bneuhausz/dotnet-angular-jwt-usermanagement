import { Menu } from "./menu";

export interface LoggedInUser {
  id: number;
  name: string;
  email: string;
  token: string;
  permissions: string[];
  menus: Menu[]
}