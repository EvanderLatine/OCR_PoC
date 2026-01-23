<script lang="ts">
  import { slide, fade } from 'svelte/transition';
  import { cn } from '$lib/utils/cn';
  import type { ExtractedData } from '$lib/api';

  let {
    data,
    partial = false,
    class: className = ''
  }: {
    data: ExtractedData;
    partial?: boolean;
    class?: string;
  } = $props();

  type FieldConfig = {
    key: keyof ExtractedData;
    label: string;
    icon: string;
    rtl?: boolean;
  };

  const fields: FieldConfig[] = [
    { key: 'name', label: 'Full Name', icon: 'user' },
    { key: 'nameAR', label: 'Name (Arabic)', icon: 'globe', rtl: true },
    { key: 'idNumber', label: 'ID Number', icon: 'id' },
    { key: 'occupation', label: 'Occupation', icon: 'briefcase' },
    { key: 'birthDate', label: 'Date of Birth', icon: 'calendar' },
    { key: 'expiryDate', label: 'Expiry Date', icon: 'clock' },
  ];

  function getIcon(type: string) {
    return type;
  }
</script>

<div
  class={cn(
    'bg-white rounded-xl p-6 shadow-sm border animate-slide-in',
    partial ? 'border-warning bg-yellow-50/30' : 'border-neutral-200',
    className
  )}
  transition:slide={{ duration: 300 }}
>
  <div class="flex items-center gap-2 mb-4">
    <div class={cn('w-2 h-2 rounded-full', partial ? 'bg-warning' : 'bg-success')}></div>
    <h3 class="font-semibold text-lg">
      {partial ? 'Partial Extraction' : 'Extraction Complete'}
    </h3>
    {#if partial}
      <span class="ml-auto text-xs text-warning bg-warning/10 px-2 py-1 rounded-full">
        Low confidence
      </span>
    {/if}
  </div>

  <div class="grid gap-3">
    {#each fields as { key, label, icon, rtl }, index (key)}
      {@const value = data[key]}
      <div
        class="flex justify-between items-center py-2 border-b border-neutral-100 last:border-0"
        in:fade={{ delay: 50 * index, duration: 200 }}
      >
        <span class="flex items-center gap-2 text-muted text-sm">
          {#if icon === 'user'}
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
          {:else if icon === 'globe'}
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20.488V18a2 2 0 012-2h3.064M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          {:else if icon === 'id'}
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V8a2 2 0 00-2-2h-5m-4 0V5a2 2 0 114 0v1m-4 0a2 2 0 104 0m-5 8a2 2 0 100-4 2 2 0 000 4zm0 0c1.306 0 2.417.835 2.83 2M9 14a3.001 3.001 0 00-2.83 2M15 11h3m-3 4h2" />
            </svg>
          {:else if icon === 'briefcase'}
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 13.255A23.931 23.931 0 0112 15c-3.183 0-6.22-.62-9-1.745M16 6V4a2 2 0 00-2-2h-4a2 2 0 00-2 2v2m4 6h.01M5 20h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
            </svg>
          {:else if icon === 'calendar'}
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
          {:else if icon === 'clock'}
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          {/if}
          {label}
        </span>
        <span
          class={cn(
            'font-medium',
            !value && 'text-neutral-400',
            rtl && value && 'rtl'
          )}
          dir={rtl && value ? 'rtl' : undefined}
        >
          {value || '—'}
        </span>
      </div>
    {/each}
  </div>
</div>
