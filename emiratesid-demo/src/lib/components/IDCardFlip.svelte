<script lang="ts">
  import type { Snippet } from 'svelte';
  import { cn } from '$lib/utils/cn';

  let {
    flipped = false,
    children,
    back,
    class: className = ''
  }: {
    flipped?: boolean;
    children: Snippet;
    back: Snippet;
    class?: string;
  } = $props();
</script>

<div class={cn('flip-container w-full', className)}>
  <div class={cn('flip-card relative w-full', flipped && 'flipped')}>
    <!-- Front face -->
    <div class="flip-face w-full" aria-hidden={flipped}>
      {@render children()}
    </div>

    <!-- Back face -->
    <div
      class="flip-face flip-face-back absolute inset-0 w-full"
      aria-hidden={!flipped}
    >
      {@render back()}
    </div>
  </div>
</div>
