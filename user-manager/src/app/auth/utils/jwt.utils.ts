import { jwtDecode } from 'jwt-decode';
import { LoggedInUser } from '../interfaces/logged-in-user';
import { Menu } from '../interfaces/menu';

interface JwtPayload {
  sub: number;
  name: string;
  email: string;
  permissions: string;
  menus: string;
  exp: number;
  [key: string]: any;
}

export function mapJwtToUser(accessToken: string, refreshToken: string): LoggedInUser {
  const payload = jwtDecode<JwtPayload>(accessToken);

  return {
    id: payload.sub,
    name: payload.name,
    email: payload.email,
    accessToken,
    refreshToken,
    permissions: JSON.parse(payload.permissions || '[]'),
    menus: JSON.parse(payload.menus || '[]') as Menu[],
    expiresAt: new Date(payload.exp * 1000),
  };
}
