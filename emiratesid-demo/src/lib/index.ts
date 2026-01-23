// Utilities
export { cn } from './utils/cn';
export {
  validateFile,
  isPdfFile,
  isImageFile,
  formatFileSize,
  MAX_FILE_SIZE,
  ACCEPTED_IMAGE_TYPES,
  ACCEPTED_PDF_TYPE,
  type ValidationResult,
  type ImageMimeType,
  type AcceptedMimeType
} from './utils/validation';

// API
export {
  parseEmiratesId,
  parseEmiratesIdPdf,
  errorToApiResponse,
  isApiResponse,
  isExtractedData,
  ApiError,
  TimeoutError,
  NetworkError,
  type ExtractedData,
  type ApiResponse
} from './api';

// Components
export { default as UploadZone } from './components/UploadZone.svelte';
export { default as ResultCard } from './components/ResultCard.svelte';
export { default as ErrorAlert } from './components/ErrorAlert.svelte';
export { default as IDCardSilhouette } from './components/IDCardSilhouette.svelte';
export { default as IDCardFlip } from './components/IDCardFlip.svelte';
export { default as StepIndicator } from './components/StepIndicator.svelte';
export { default as FilePreview } from './components/FilePreview.svelte';
export { default as Skeleton } from './components/Skeleton.svelte';
