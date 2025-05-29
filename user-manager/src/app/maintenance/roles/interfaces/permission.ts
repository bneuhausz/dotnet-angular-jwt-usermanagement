export interface Permission {
  id: string;
  parentPermissionId?: string;
  name: string;
  type?: string;
}