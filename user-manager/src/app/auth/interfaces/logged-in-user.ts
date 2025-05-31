import { Menu } from "./menu";

export interface LoggedInUser {
  id: number;
  name: string;
  email: string;
  accessToken: string;
  refreshToken?: string;
  permissions: string[];
  menus: Menu[];
  expiresAt: Date;
}