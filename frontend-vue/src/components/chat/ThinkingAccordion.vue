<script setup lang="ts">
import { RefreshCw } from '@lucide/vue'

interface Props {
  thinking: string
  isThinking?: boolean
}

withDefaults(defineProps<Props>(), {
  isThinking: false
})
</script>

<template>
  <div class="mb-3">
    <details 
      class="group bg-brand-panel/30 border border-brand-border/60 rounded-xl overflow-hidden" 
      :open="isThinking"
    >
      <summary class="flex items-center gap-2 p-3 font-bold text-xs text-text-secondary select-none cursor-pointer hover:bg-brand-panel transition">
        <RefreshCw 
          class="w-3.5 h-3.5 text-brand-accent shrink-0" 
          :class="{ 'animate-spin': isThinking }" 
        />
        <span>AI Thinking Process...</span>
        <span class="ml-auto text-text-secondary/60 group-open:rotate-180 transition-transform">▼</span>
      </summary>
      <div class="px-4 pb-4 pt-1 text-xs text-text-secondary/80 font-mono whitespace-pre-wrap leading-relaxed border-t border-brand-border/30 max-h-60 overflow-y-auto">
        {{ thinking }}
      </div>
    </details>
  </div>
</template>
