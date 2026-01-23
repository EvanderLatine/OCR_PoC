<script lang="ts">
  import { cn } from '$lib/utils/cn';

  let {
    variant = 'text',
    class: className = ''
  }: {
    variant?: 'text' | 'card' | 'circle';
    class?: string;
  } = $props();

  const baseClasses = 'animate-shimmer rounded';

  const variantClasses = {
    text: 'h-4 w-full rounded-md',
    card: 'id-card-silhouette w-full rounded-lg',
    circle: 'w-12 h-12 rounded-full'
  };
</script>

<div
  class={cn(baseClasses, variantClasses[variant], className)}
  role="status"
  aria-label="Loading..."
>
  <span class="sr-only">Loading...</span>
</div>
