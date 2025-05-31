export interface Permission {
  id: number;
  parentPermissionId?: number;
  name: string;
  type?: string;
}