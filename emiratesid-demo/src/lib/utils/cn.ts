/**
 * Utility for composing CSS class names (clsx-pattern)
 * Filters out falsy values and joins remaining strings
 */
export function cn(...inputs: (string | boolean | null | undefined)[]): string {
  return inputs.filter((input): input is string => typeof input === 'string' && input.length > 0).join(' ');
}
