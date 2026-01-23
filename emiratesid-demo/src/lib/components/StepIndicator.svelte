<script lang="ts">
  import { cn } from '$lib/utils/cn';

  let {
    currentStep = 1,
    totalSteps = 2,
    labels = []
  }: {
    currentStep?: number;
    totalSteps?: number;
    labels?: string[];
  } = $props();

  const steps = $derived(
    Array.from({ length: totalSteps }, (_, i) => ({
      number: i + 1,
      label: labels[i] || `Step ${i + 1}`,
      status: i + 1 < currentStep ? 'completed' : i + 1 === currentStep ? 'current' : 'pending'
    }))
  );
</script>

<nav aria-label="Progress" class="w-full">
  <ol class="flex items-center justify-center gap-2" role="list">
    {#each steps as step, index (step.number)}
      <li class="flex items-center">
        <!-- Step circle -->
        <div
          class={cn(
            'flex items-center justify-center w-8 h-8 rounded-full text-sm font-medium transition-all duration-300',
            step.status === 'completed' && 'bg-success text-white',
            step.status === 'current' && 'bg-primary text-white ring-4 ring-primary/20',
            step.status === 'pending' && 'bg-neutral-200 text-neutral-500'
          )}
          aria-current={step.status === 'current' ? 'step' : undefined}
        >
          {#if step.status === 'completed'}
            <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd" />
            </svg>
          {:else}
            {step.number}
          {/if}
        </div>

        <!-- Step label -->
        <span
          class={cn(
            'ml-2 text-sm font-medium hidden sm:block',
            step.status === 'completed' && 'text-success',
            step.status === 'current' && 'text-dark',
            step.status === 'pending' && 'text-muted'
          )}
        >
          {step.label}
        </span>

        <!-- Connector line -->
        {#if index < steps.length - 1}
          <div
            class={cn(
              'w-8 sm:w-12 h-0.5 mx-2 sm:mx-4 transition-colors duration-300',
              step.status === 'completed' ? 'bg-success' : 'bg-neutral-200'
            )}
            aria-hidden="true"
          ></div>
        {/if}
      </li>
    {/each}
  </ol>
</nav>
