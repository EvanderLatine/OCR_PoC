<script lang="ts">
  import { cn } from '$lib/utils/cn';

  let {
    side = 'front',
    showUploadHint = true,
    class: className = ''
  }: {
    side?: 'front' | 'back';
    showUploadHint?: boolean;
    class?: string;
  } = $props();
</script>

<div
  class={cn(
    'id-card-silhouette w-full rounded-lg border-2 border-dashed border-neutral-300 bg-neutral-100 overflow-hidden',
    className
  )}
>
  <div class="relative w-full h-full p-4 flex flex-col">
    {#if side === 'front'}
      <!-- Front side layout -->
      <div class="flex-1 flex gap-4">
        <!-- Photo placeholder (left side) -->
        <div class="w-1/3 flex flex-col gap-2">
          <div class="aspect-[3/4] bg-neutral-200 rounded-md flex items-center justify-center">
            <svg class="w-8 h-8 text-neutral-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
          </div>
        </div>

        <!-- Text lines (right side) -->
        <div class="flex-1 flex flex-col justify-center gap-3">
          <!-- UAE Emblem placeholder -->
          <div class="w-12 h-12 bg-neutral-200 rounded-full mx-auto mb-2 flex items-center justify-center">
            <svg class="w-6 h-6 text-neutral-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20.488V18a2 2 0 012-2h3.064M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <!-- Name lines -->
          <div class="h-3 bg-neutral-200 rounded-full w-full"></div>
          <div class="h-3 bg-neutral-200 rounded-full w-3/4"></div>
          <div class="h-3 bg-neutral-200 rounded-full w-1/2"></div>
          <!-- ID Number -->
          <div class="h-3 bg-neutral-200 rounded-full w-2/3 mt-2"></div>
        </div>
      </div>
    {:else}
      <!-- Back side layout -->
      <div class="flex-1 flex flex-col">
        <!-- Top content area -->
        <div class="flex-1 flex flex-col justify-center gap-3 px-2">
          <div class="h-3 bg-neutral-200 rounded-full w-full"></div>
          <div class="h-3 bg-neutral-200 rounded-full w-4/5"></div>
          <div class="h-3 bg-neutral-200 rounded-full w-3/5"></div>
        </div>

        <!-- MRZ Zone (bottom) -->
        <div class="mt-auto pt-4 space-y-1.5 bg-neutral-200/50 p-3 rounded-md">
          <div class="h-2.5 bg-neutral-300 rounded-sm w-full font-mono"></div>
          <div class="h-2.5 bg-neutral-300 rounded-sm w-full font-mono"></div>
          <div class="h-2.5 bg-neutral-300 rounded-sm w-full font-mono"></div>
        </div>
      </div>
    {/if}

    {#if showUploadHint}
      <!-- Upload hint overlay -->
      <div class="absolute inset-0 flex items-center justify-center bg-neutral-100/80">
        <div class="text-center">
          <svg class="w-10 h-10 mx-auto text-neutral-400 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
          </svg>
          <p class="text-sm font-medium text-neutral-600">
            {side === 'front' ? 'Upload Front Side' : 'Upload Back Side'}
          </p>
          <p class="text-xs text-neutral-400 mt-1">
            Drag & drop or click
          </p>
        </div>
      </div>
    {/if}
  </div>
</div>
