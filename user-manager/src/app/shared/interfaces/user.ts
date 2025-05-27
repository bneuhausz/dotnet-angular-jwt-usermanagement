import { Menu } from "./menu";

export interface User {
  id: string;
  name: string;
  email: string;
  token: string;
  permissions: string[];
  menus: Menu[]
}