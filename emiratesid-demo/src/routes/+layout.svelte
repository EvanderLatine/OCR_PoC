<script>
  import '../app.css';
  let { children } = $props();
</script>

<a href="#main-content" class="skip-link">
  Skip to main content
</a>

<div class="min-h-screen flex flex-col">
  <header class="bg-dark text-white py-4 px-6 flex justify-between items-center">
    <div class="flex items-center gap-3">
      <img src="/ferrari_world_favicon.svg" alt="Logo" class="w-8 h-8" />
      <span class="text-xl font-semibold tracking-tight">ID Extractor</span>
    </div>
    <span class="text-muted text-sm">Demo POC</span>
  </header>

  <main id="main-content" class="flex-1">
    {@render children()}
  </main>

  <footer class="bg-dark text-muted py-4 px-6 text-center text-sm">
    <p>Emirates ID Extractor Demo - Powered by Azure AI Document Intelligence</p>
    <p class="text-xs mt-1 text-neutral-500">
      For demonstration purposes only. Do not upload real identity documents.
    </p>
  </footer>
</div>
