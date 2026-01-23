<script lang="ts">
  import UploadZone from '$lib/components/UploadZone.svelte';
  import ResultCard from '$lib/components/ResultCard.svelte';
  import ErrorAlert from '$lib/components/ErrorAlert.svelte';
  import IDCardSilhouette from '$lib/components/IDCardSilhouette.svelte';
  import IDCardFlip from '$lib/components/IDCardFlip.svelte';
  import StepIndicator from '$lib/components/StepIndicator.svelte';
  import FilePreview from '$lib/components/FilePreview.svelte';
  import Skeleton from '$lib/components/Skeleton.svelte';
  import { parseEmiratesId, parseEmiratesIdPdf, errorToApiResponse, type ApiResponse } from '$lib/api';
  import { isPdfFile } from '$lib/utils/validation';
  import { cn } from '$lib/utils/cn';

  // State types
  type UploadMode = 'initial' | 'pdf' | 'image';
  type FlowStep = 'front' | 'back' | 'processing' | 'result';

  // State
  let mode = $state<UploadMode>('initial');
  let step = $state<FlowStep>('front');
  let frontFile = $state<File | null>(null);
  let backFile = $state<File | null>(null);
  let pdfFile = $state<File | null>(null);
  let frontError = $state<string | null>(null);
  let backError = $state<string | null>(null);
  let pdfError = $state<string | null>(null);
  let result = $state<ApiResponse | null>(null);
  let isFlipped = $state(false);

  // Derived state
  const isProcessing = $derived(step === 'processing');
  const showResult = $derived(step === 'result' && result !== null);
  const canProceedToBack = $derived(mode === 'image' && !!frontFile && !frontError);
  const canSubmitPdf = $derived(mode === 'pdf' && !!pdfFile && !pdfError);
  const canSubmitImages = $derived(mode === 'image' && !!frontFile && !!backFile && !frontError && !backError);

  const stepLabels = ['Front Side', 'Back Side'];
  const currentStepNumber = $derived(step === 'front' ? 1 : step === 'back' ? 2 : 2);

  // Handle initial file upload - detect mode based on file type
  function handleInitialFileAccepted(file: File) {
    if (isPdfFile(file)) {
      mode = 'pdf';
      pdfFile = file;
      frontFile = null;
    } else {
      mode = 'image';
      frontFile = file;
    }
  }

  // Proceed to back side
  function proceedToBack() {
    if (!canProceedToBack) return;
    step = 'back';
    isFlipped = true;
  }

  // Go back to front side
  function goToFront() {
    step = 'front';
    isFlipped = false;
  }

  // Submit for processing
  async function handleSubmit() {
    step = 'processing';
    result = null;

    try {
      if (mode === 'pdf' && pdfFile) {
        result = await parseEmiratesIdPdf(pdfFile);
      } else if (mode === 'image' && frontFile && backFile) {
        result = await parseEmiratesId(frontFile, backFile);
      }
    } catch (e) {
      result = errorToApiResponse(e);
    } finally {
      step = 'result';
    }
  }

  // Reset to initial state
  function reset() {
    mode = 'initial';
    step = 'front';
    frontFile = null;
    backFile = null;
    pdfFile = null;
    frontError = null;
    backError = null;
    pdfError = null;
    result = null;
    isFlipped = false;
  }
</script>

<svelte:head>
  <title>Emirates ID Extractor - Demo</title>
  <meta name="description" content="Extract data from UAE Emirates ID cards using AI-powered document intelligence" />
</svelte:head>

<div class="max-w-2xl mx-auto px-4 py-12">
  <div class="text-center mb-10">
    <h1 class="text-3xl font-bold text-dark mb-2">
      Emirates ID Extractor
    </h1>
    <p class="text-muted">
      {#if mode === 'initial'}
        Upload your Emirates ID to extract information
      {:else if mode === 'pdf'}
        PDF document detected - ready to extract
      {:else}
        Upload front and back images
      {/if}
    </p>
  </div>

  {#if mode === 'image' && step !== 'result'}
    <div class="mb-6">
      <StepIndicator
        currentStep={currentStepNumber}
        totalSteps={2}
        labels={stepLabels}
      />
    </div>
  {/if}

  <div class="bg-white rounded-2xl shadow-lg p-8 mb-6">
    <h2 class="text-sm font-semibold text-dark uppercase tracking-wide mb-4">
      Emirates ID
    </h2>

    {#if step === 'processing'}
      <!-- Processing state -->
      <div class="space-y-4" aria-live="polite" aria-busy="true">
        <Skeleton variant="card" />
        <div class="flex items-center justify-center gap-3 py-4">
          <span class="w-5 h-5 border-2 border-primary/30 border-t-primary rounded-full animate-spin"></span>
          <span class="text-muted">Analyzing document...</span>
        </div>
      </div>

    {:else if step === 'result' && result}
      <!-- Result state -->
      <div class="animate-fade-in">
        {#if result.response === 200 && result.data}
          <ResultCard data={result.data} />
        {:else if result.response === 206 && result.data}
          <ResultCard data={result.data} partial />
          {#if result.errMsg}
            <p class="text-sm text-warning mt-3 text-center">{result.errMsg}</p>
          {/if}
        {:else}
          <ErrorAlert message={result.errMsg} code={result.response} />
        {/if}

        <button
          type="button"
          class="mt-6 text-sm text-primary hover:underline mx-auto block focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2 rounded"
          onclick={reset}
        >
          Start Over
        </button>
      </div>

    {:else if mode === 'initial'}
      <!-- Initial upload state -->
      <div class="relative">
        <IDCardSilhouette side="front" showUploadHint={false} class="mb-4 opacity-50" />
        <div class="absolute inset-0 flex items-center justify-center">
          <div class="w-full max-w-sm">
            <UploadZone
              label="Upload Emirates ID"
              bind:file={frontFile}
              bind:error={frontError}
              onFileAccepted={handleInitialFileAccepted}
            />
          </div>
        </div>
      </div>
      <p class="text-xs text-center text-muted mt-4">
        Accepts: JPEG, PNG, WebP images or PDF documents
      </p>


    {:else if mode === 'pdf'}
      <!-- PDF mode -->
      <div class="space-y-4">
        {#if pdfFile}
          <FilePreview file={pdfFile} onRemove={() => { pdfFile = null; mode = 'initial'; }} />
        {:else}
          <UploadZone
            label="Upload PDF Document"
            accept="application/pdf"
            bind:file={pdfFile}
            bind:error={pdfError}
          />
        {/if}

        <button
          type="button"
          class={cn(
            'w-full py-3 px-6 rounded-lg font-semibold text-white transition-colors duration-200',
            'flex items-center justify-center gap-2',
            'bg-primary hover:bg-primary-hover',
            'disabled:bg-neutral-300 disabled:cursor-not-allowed',
            'focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2'
          )}
          disabled={!canSubmitPdf}
          onclick={handleSubmit}
        >
          Extract Data
        </button>
      </div>

    {:else if mode === 'image'}
      <!-- Image mode with flip animation -->
      <IDCardFlip flipped={isFlipped}>
        {#snippet children()}
          <!-- Front side -->
          <div class="space-y-4">
            {#if frontFile}
              <FilePreview file={frontFile} onRemove={() => { frontFile = null; }} />
            {:else}
              <div class="relative">
                <IDCardSilhouette side="front" showUploadHint={false} />
                <div class="absolute inset-0">
                  <UploadZone
                    label="Upload Front Side"
                    accept="image/jpeg,image/png,image/webp"
                    bind:file={frontFile}
                    bind:error={frontError}
                  />
                </div>
              </div>
            {/if}

            <button
              type="button"
              class={cn(
                'w-full py-3 px-6 rounded-lg font-semibold text-white transition-colors duration-200',
                'flex items-center justify-center gap-2',
                'bg-primary hover:bg-primary-hover',
                'disabled:bg-neutral-300 disabled:cursor-not-allowed',
                'focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2'
              )}
              disabled={!canProceedToBack}
              onclick={proceedToBack}
            >
              Continue to Back Side
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>
            </button>
          </div>
        {/snippet}

        {#snippet back()}
          <!-- Back side -->
          <div class="space-y-4">
            {#if backFile}
              <FilePreview file={backFile} onRemove={() => { backFile = null; }} />
            {:else}
              <div class="relative">
                <IDCardSilhouette side="back" showUploadHint={false} />
                <div class="absolute inset-0">
                  <UploadZone
                    label="Upload Back Side"
                    accept="image/jpeg,image/png,image/webp"
                    bind:file={backFile}
                    bind:error={backError}
                  />
                </div>
              </div>
            {/if}

            <div class="flex gap-3">
              <button
                type="button"
                class={cn(
                  'flex-1 py-3 px-6 rounded-lg font-semibold transition-colors duration-200',
                  'flex items-center justify-center gap-2',
                  'border-2 border-neutral-300 text-dark hover:border-primary hover:text-primary',
                  'focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2'
                )}
                onclick={goToFront}
              >
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
                </svg>
                Back
              </button>
              <button
                type="button"
                class={cn(
                  'flex-1 py-3 px-6 rounded-lg font-semibold text-white transition-colors duration-200',
                  'flex items-center justify-center gap-2',
                  'bg-primary hover:bg-primary-hover',
                  'disabled:bg-neutral-300 disabled:cursor-not-allowed',
                  'focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2'
                )}
                disabled={!canSubmitImages}
                onclick={handleSubmit}
              >
                Extract Data
              </button>
            </div>
          </div>
        {/snippet}
      </IDCardFlip>
    {/if}
  </div>
</div>
