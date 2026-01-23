<script lang="ts">
  import { cn } from '$lib/utils/cn';
  import { validateFile, isPdfFile, isImageFile } from '$lib/utils/validation';

  let {
    label,
    file = $bindable(),
    accept = 'image/jpeg,image/png,image/webp,application/pdf',
    error = $bindable(),
    disabled = false,
    id = crypto.randomUUID(),
    onFileAccepted
  }: {
    label: string;
    file?: File | null;
    accept?: string;
    error?: string | null;
    disabled?: boolean;
    id?: string;
    onFileAccepted?: (file: File) => void;
  } = $props();

  let isDragging = $state(false);
  let inputEl: HTMLInputElement;

  const errorId = $derived(`${id}-error`);

  function handleFile(newFile: File | null) {
    if (!newFile) {
      file = null;
      error = null;
      return;
    }

    const validation = validateFile(newFile);
    if (!validation.valid) {
      error = validation.error ?? 'Invalid file';
      file = null;
      return;
    }

    error = null;
    file = newFile;
    onFileAccepted?.(newFile);
  }

  function handleDrop(e: DragEvent) {
    e.preventDefault();
    isDragging = false;
    if (disabled) return;

    const dropped = e.dataTransfer?.files[0];
    if (dropped) handleFile(dropped);
  }

  function handleSelect(e: Event) {
    const target = e.target as HTMLInputElement;
    handleFile(target.files?.[0] ?? null);
    // Reset input so same file can be selected again
    target.value = '';
  }

  function handleKeydown(e: KeyboardEvent) {
    if (disabled) return;
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      inputEl.click();
    }
  }

  function handleRemove(e: Event) {
    e.stopPropagation();
    file = null;
    error = null;
  }

  function getFileIcon(f: File) {
    if (isPdfFile(f)) return 'pdf';
    if (isImageFile(f)) return 'image';
    return 'file';
  }
</script>

<div class="w-full">
  <div
    class={cn(
      'border-2 border-dashed rounded-lg p-6 cursor-pointer transition-all duration-200 bg-white',
      isDragging && 'border-primary bg-red-50/30',
      file && !error && 'border-primary border-solid bg-red-50/20',
      error && 'border-error bg-red-50/20',
      !isDragging && !file && !error && 'border-neutral-300 hover:border-primary hover:bg-red-50/30',
      disabled && 'opacity-50 cursor-not-allowed'
    )}
    ondragover={(e) => { e.preventDefault(); if (!disabled) isDragging = true; }}
    ondragleave={() => isDragging = false}
    ondrop={handleDrop}
    onclick={() => !disabled && inputEl.click()}
    onkeydown={handleKeydown}
    role="button"
    tabindex={disabled ? -1 : 0}
    aria-label={file ? `Selected file: ${file.name}. Press Enter to change` : label}
    aria-describedby={error ? errorId : undefined}
  >
    <input
      bind:this={inputEl}
      type="file"
      {accept}
      onchange={handleSelect}
      class="hidden"
      {disabled}
      aria-hidden="true"
    />

    <div class="text-center">
      {#if file}
        <div class="flex items-center justify-center gap-2 text-primary">
          {#if getFileIcon(file) === 'pdf'}
            <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
              <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8l-6-6zM6 20V4h7v5h5v11H6z"/>
            </svg>
          {:else}
            <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"/>
            </svg>
          {/if}
          <span class="font-medium truncate max-w-[200px]">{file.name}</span>
        </div>
        <button
          type="button"
          class="text-sm text-muted hover:text-dark mt-1 focus:outline-none focus:underline"
          onclick={handleRemove}
        >
          Remove
        </button>
      {:else}
        <div class="mb-2">
          <svg class="w-8 h-8 mx-auto text-muted" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12"/>
          </svg>
        </div>
        <p class="font-medium text-dark">{label}</p>
        <p class="text-xs text-muted mt-1">
          Drag & drop or click
        </p>
      {/if}
    </div>
  </div>

  {#if error}
    <p id={errorId} class="mt-2 text-sm text-error" role="alert">
      {error}
    </p>
  {/if}
</div>
