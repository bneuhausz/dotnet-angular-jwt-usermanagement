import { jwtDecode } from 'jwt-decode';
import { LoggedInUser } from '../interfaces/logged-in-user';
import { Menu } from '../interfaces/menu';

interface JwtPayload {
  sub: string;
  name: string;
  email: string;
  permissions: string;
  menus: string;
  [key: string]: any;
}

export function mapJwtToUser(token: string): LoggedInUser {
  const payload = jwtDecode<JwtPayload>(token);

  return {
    id: payload.sub,
    name: payload.name,
    email: payload.email,
    token,
    permissions: JSON.parse(payload.permissions || '[]'),
    menus: JSON.parse(payload.menus || '[]') as Menu[]
  };
}
