<script lang="ts">
  import { cn } from '$lib/utils/cn';
  import { isPdfFile } from '$lib/utils/validation';

  let {
    file,
    onRemove,
    class: className = ''
  }: {
    file: File;
    onRemove?: () => void;
    class?: string;
  } = $props();

  const isPdf = $derived(isPdfFile(file));
  const previewUrl = $derived(!isPdf ? URL.createObjectURL(file) : null);

  // Cleanup object URL on unmount
  $effect(() => {
    return () => {
      if (previewUrl) {
        URL.revokeObjectURL(previewUrl);
      }
    };
  });

  function formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }
</script>

<div
  class={cn(
    'relative group rounded-lg border border-neutral-200 bg-white overflow-hidden',
    className
  )}
>
  {#if isPdf}
    <!-- PDF Preview -->
    <div class="flex items-center gap-3 p-3">
      <div class="w-10 h-10 bg-red-100 rounded-lg flex items-center justify-center shrink-0">
        <svg class="w-5 h-5 text-red-600" fill="currentColor" viewBox="0 0 24 24">
          <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8l-6-6zM6 20V4h7v5h5v11H6zm2-6h8v2H8v-2zm0 4h6v2H8v-2z"/>
        </svg>
      </div>
      <div class="flex-1 min-w-0">
        <p class="text-sm font-medium text-neutral-900 truncate">{file.name}</p>
        <p class="text-xs text-neutral-500">{formatSize(file.size)}</p>
      </div>
    </div>
  {:else}
    <!-- Image Preview -->
    <div class="id-card-silhouette">
      <img
        src={previewUrl}
        alt="Preview of {file.name}"
        class="w-full h-full object-cover"
      />
    </div>
    <div class="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/60 to-transparent p-2">
      <p class="text-xs text-white truncate">{file.name}</p>
    </div>
  {/if}

  <!-- Remove button -->
  {#if onRemove}
    <button
      type="button"
      onclick={onRemove}
      class={cn(
        'absolute top-2 right-2 w-6 h-6 rounded-full bg-neutral-900/70 text-white',
        'flex items-center justify-center opacity-0 group-hover:opacity-100',
        'transition-opacity duration-200 hover:bg-neutral-900',
        'focus:opacity-100 focus:outline-none focus:ring-2 focus:ring-white'
      )}
      aria-label="Remove file"
    >
      <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
      </svg>
    </button>
  {/if}
</div>
