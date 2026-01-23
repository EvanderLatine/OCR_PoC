<script lang="ts">
  import { cn } from '$lib/utils/cn';

  let {
    message,
    code = 0,
    class: className = ''
  }: {
    message: string;
    code?: number;
    class?: string;
  } = $props();

  function getTitle(statusCode: number): string {
    if (statusCode === 400) return 'Bad Request';
    if (statusCode === 408) return 'Request Timeout';
    if (statusCode === 422) return 'Validation Error';
    if (statusCode >= 400 && statusCode < 500) return 'Client Error';
    if (statusCode >= 500) return 'Server Error';
    if (statusCode === 0) return 'Connection Error';
    return 'Processing Failed';
  }
</script>

<div
  class={cn(
    'bg-red-50 border border-red-200 rounded-lg p-4 animate-shake',
    className
  )}
  role="alert"
  aria-live="assertive"
>
  <div class="flex items-start gap-3">
    <svg class="w-5 h-5 text-error shrink-0 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
      <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"/>
    </svg>
    <div>
      <h4 class="font-semibold text-error">
        {getTitle(code)}
      </h4>
      <p class="text-sm text-neutral-600 mt-1">{message}</p>
      {#if code > 0}
        <p class="text-xs text-neutral-400 mt-2">Error code: {code}</p>
      {/if}
    </div>
  </div>
</div>
