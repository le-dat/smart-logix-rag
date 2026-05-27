<script setup lang="ts">
import { BookOpen } from '@lucide/vue'
import type { Citation } from '../../types'

interface Props {
  citations: Citation[]
  hoveredIndex: number | null
}

defineProps<Props>()
</script>

<template>
  <div class="space-y-2 mt-2 w-full">
    <!-- Sources label -->
    <div class="flex items-center gap-1.5 text-sm font-extrabold uppercase tracking-widest text-text-secondary">
      <BookOpen class="w-3.5 h-3.5 text-brand-accent" />
      <span>Sources & References</span>
    </div>

    <!-- Perplexity-style Grid of Source Cards -->
    <div class="flex flex-col gap-2.5">
      <div 
        v-for="(c, idx) in citations" 
        :key="idx"
        class="p-2.5 rounded-xl border text-left bg-card-bg transition-all duration-300 relative overflow-hidden group select-none shadow-[0_1px_2px_rgba(0,0,0,0.01)]"
        :class="hoveredIndex === idx + 1 
          ? 'border-brand-accent bg-brand-accent-glow ring-2 ring-brand-accent-glow scale-[1.01]' 
          : 'border-brand-border hover:border-brand-accent/60 hover:scale-[1.01]'"
        :title="c.content_snippet"
      >
        <!-- Top header info -->
        <div class="flex items-center justify-between gap-1.5 mb-1.5 border-b border-brand-border/30 pb-1">
          <div class="flex items-center gap-1.5 min-w-0">
            <span 
              class="w-3.5 h-3.5 rounded-full text-sm font-bold flex items-center justify-center font-mono transition-colors duration-300"
              :class="hoveredIndex === idx + 1 ? 'bg-brand-accent text-white' : 'bg-brand-panel text-brand-accent'"
            >
              {{ idx + 1 }}
            </span>
            <span class="text-sm font-bold text-text-primary truncate font-mono">{{ c.source }}</span>
          </div>
          <span class="text-sm text-text-secondary uppercase font-bold tracking-wider shrink-0 font-mono">
            RAG Match
          </span>
        </div>
        <!-- Snippet content preview -->
        <p class="text-sm text-text-secondary line-clamp-2 leading-relaxed italic group-hover:text-text-primary transition-colors">
          "{{ c.content_snippet }}"
        </p>
      </div>
    </div>
  </div>
</template>
