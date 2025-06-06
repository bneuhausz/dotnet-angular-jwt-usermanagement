export function toKebab(str: string): string {
  return str.toLowerCase().replace(/\s+/g, '-');
}