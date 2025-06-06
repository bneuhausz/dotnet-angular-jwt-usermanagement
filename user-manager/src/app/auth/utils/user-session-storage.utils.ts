import { LoggedInUser } from "../interfaces/logged-in-user";

const appKey = 'user-manager';
const userKey = `${appKey}.user`;

export function getUserFromSessionStorage() : LoggedInUser | undefined {
  const rawUser = sessionStorage.getItem(userKey);
  if (!rawUser) {
    return undefined;
  }
  
  return JSON.parse(rawUser) as LoggedInUser;
}

export function setUserInSessionStorage(user: LoggedInUser) {
  sessionStorage.setItem(userKey, JSON.stringify(user));
}

export function clearUserFromSessionStorage() {
  sessionStorage.removeItem(userKey);
}