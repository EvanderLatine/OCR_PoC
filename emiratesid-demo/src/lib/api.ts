/**
 * API layer for Emirates ID extraction service
 */

const API_URL = import.meta.env.PUBLIC_API_URL || 'http://localhost:5154/api';
const DEFAULT_TIMEOUT = 60000; // 60 seconds

// ============================================================================
// Types
// ============================================================================

export interface ExtractedData {
  name: string;
  nameAR: string;
  idNumber: string;
  occupation: string;
  expiryDate: string;
  birthDate: string;
}

export interface ApiResponse {
  response: number;
  errMsg: string;
  data: ExtractedData | null;
}

// ============================================================================
// Error Classes
// ============================================================================

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly code: number,
    public readonly originalError?: Error
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

export class TimeoutError extends ApiError {
  constructor(timeoutMs: number) {
    super(`Request timed out after ${timeoutMs / 1000} seconds`, 408);
    this.name = 'TimeoutError';
  }
}

export class NetworkError extends ApiError {
  constructor(message: string = 'Network connection failed') {
    super(message, 0);
    this.name = 'NetworkError';
  }
}

// ============================================================================
// Response Validation
// ============================================================================

/**
 * Type guard to validate API response structure
 */
export function isApiResponse(data: unknown): data is ApiResponse {
  if (typeof data !== 'object' || data === null) {
    return false;
  }

  const obj = data as Record<string, unknown>;

  if (typeof obj.response !== 'number') {
    return false;
  }

  if (typeof obj.errMsg !== 'string') {
    return false;
  }

  if (obj.data !== null && typeof obj.data !== 'object') {
    return false;
  }

  return true;
}

/**
 * Validates extracted data has expected fields
 */
export function isExtractedData(data: unknown): data is ExtractedData {
  if (typeof data !== 'object' || data === null) {
    return false;
  }

  const obj = data as Record<string, unknown>;
  const requiredFields = ['name', 'nameAR', 'idNumber', 'occupation', 'expiryDate', 'birthDate'];

  return requiredFields.every((field) => typeof obj[field] === 'string');
}

// ============================================================================
// Fetch Utilities
// ============================================================================

/**
 * Fetch with timeout support using AbortController
 */
async function fetchWithTimeout(
  url: string,
  options: RequestInit,
  timeoutMs: number = DEFAULT_TIMEOUT
): Promise<Response> {
  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), timeoutMs);

  try {
    const response = await fetch(url, {
      ...options,
      signal: controller.signal
    });
    return response;
  } catch (error) {
    if (error instanceof Error && error.name === 'AbortError') {
      throw new TimeoutError(timeoutMs);
    }
    throw new NetworkError(
      error instanceof Error ? error.message : 'Network request failed'
    );
  } finally {
    clearTimeout(timeoutId);
  }
}

// ============================================================================
// API Functions
// ============================================================================

/**
 * Parse Emirates ID from front and back images
 */
export async function parseEmiratesId(front: File, back: File): Promise<ApiResponse> {
  const formData = new FormData();
  formData.append('Front', front);
  formData.append('Back', back);

  const response = await fetchWithTimeout(
    `${API_URL}/id-document/parse/emirates-id`,
    {
      method: 'POST',
      body: formData
    }
  );

  if (!response.ok && response.status >= 500) {
    throw new ApiError(
      `Server error: ${response.statusText || 'Internal Server Error'}`,
      response.status
    );
  }

  let data: unknown;
  try {
    data = await response.json();
  } catch {
    throw new ApiError('Invalid response format from server', response.status);
  }

  if (!isApiResponse(data)) {
    throw new ApiError('Unexpected response structure from server', response.status);
  }

  return data;
}

/**
 * Parse Emirates ID from a single PDF file containing both sides
 */
export async function parseEmiratesIdPdf(pdf: File): Promise<ApiResponse> {
  const formData = new FormData();
  formData.append('Front', pdf);

  const response = await fetchWithTimeout(
    `${API_URL}/id-document/parse/emirates-id`,
    {
      method: 'POST',
      body: formData
    }
  );

  if (!response.ok && response.status >= 500) {
    throw new ApiError(
      `Server error: ${response.statusText || 'Internal Server Error'}`,
      response.status
    );
  }

  let data: unknown;
  try {
    data = await response.json();
  } catch {
    throw new ApiError('Invalid response format from server', response.status);
  }

  if (!isApiResponse(data)) {
    throw new ApiError('Unexpected response structure from server', response.status);
  }

  return data;
}

/**
 * Convert API error to user-friendly ApiResponse
 */
export function errorToApiResponse(error: unknown): ApiResponse {
  if (error instanceof TimeoutError) {
    return {
      response: 408,
      errMsg: 'Request timed out. The server took too long to respond. Please try again.',
      data: null
    };
  }

  if (error instanceof NetworkError) {
    return {
      response: 0,
      errMsg: 'Connection failed. Please check if the API server is running and try again.',
      data: null
    };
  }

  if (error instanceof ApiError) {
    return {
      response: error.code,
      errMsg: error.message,
      data: null
    };
  }

  return {
    response: 500,
    errMsg: error instanceof Error ? error.message : 'An unexpected error occurred',
    data: null
  };
}
