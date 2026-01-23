/**
 * File validation utilities for upload handling
 */

export const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB
export const ACCEPTED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp'] as const;
export const ACCEPTED_PDF_TYPE = 'application/pdf' as const;

export type ImageMimeType = (typeof ACCEPTED_IMAGE_TYPES)[number];
export type AcceptedMimeType = ImageMimeType | typeof ACCEPTED_PDF_TYPE;

export interface ValidationResult {
  valid: boolean;
  error?: string;
}

/**
 * Validates a file for size and MIME type
 */
export function validateFile(file: File): ValidationResult {
  if (file.size > MAX_FILE_SIZE) {
    const sizeMB = (file.size / (1024 * 1024)).toFixed(1);
    return {
      valid: false,
      error: `File size (${sizeMB}MB) exceeds maximum allowed size of 10MB`
    };
  }

  const isImage = isImageFile(file);
  const isPdf = isPdfFile(file);

  if (!isImage && !isPdf) {
    return {
      valid: false,
      error: `Invalid file type "${file.type}". Accepted types: JPEG, PNG, WebP, PDF`
    };
  }

  return { valid: true };
}

/**
 * Checks if file is a PDF
 */
export function isPdfFile(file: File): boolean {
  return file.type === ACCEPTED_PDF_TYPE;
}

/**
 * Checks if file is an accepted image type
 */
export function isImageFile(file: File): boolean {
  return (ACCEPTED_IMAGE_TYPES as readonly string[]).includes(file.type);
}

/**
 * Returns a human-readable file size string
 */
export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}
