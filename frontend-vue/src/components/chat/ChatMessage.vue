<script setup lang="ts">
import { ref, computed } from 'vue'
import { Bot, User, Layers } from '@lucide/vue'
import MarkdownIt from 'markdown-it'
import type { Message } from '../../types'
import CitationList from './CitationList.vue'

interface Props {
  message: Message
}

const props = defineProps<Props>()

const hoveredIndex = ref<number | null>(null)

const md = new MarkdownIt({
  html: false,
  linkify: true,
  typographer: true
})

// Parse citation foot references e.g. [1] into beautiful interactive capsules
const renderedText = computed(() => {
  const html = md.render(props.message.displayText || '')
  return html.replace(/\[([0-9]+)\]/g, (_, num) => {
    return `<span class="footnote-ref cursor-help inline-flex items-center justify-center text-[9px] font-black h-4 w-4 rounded-full bg-brand-panel hover:bg-brand-accent hover:text-white border border-brand-border text-brand-accent transition-colors ml-0.5" data-index="${num}">${num}</span>`
  })
})

// Event delegation to capture mouse hovers on superscript footnotes
const handleMouseOver = (event: MouseEvent) => {
  const target = event.target as HTMLElement
  if (target.classList.contains('footnote-ref')) {
    const idx = target.getAttribute('data-index')
    if (idx) {
      hoveredIndex.value = Number(idx)
    }
  }
}

const handleMouseLeave = (event: MouseEvent) => {
  const target = event.target as HTMLElement
  if (target.classList.contains('footnote-ref')) {
    hoveredIndex.value = null
  }
}
</script>

<template>
  <div 
    class="flex items-start gap-3.5 group select-text"
    :class="message.role === 'user' ? 'justify-end' : 'justify-start'"
  >
    <!-- Bot Avatar -->
    <div 
      v-if="message.role === 'assistant'" 
      class="h-8 w-8 rounded-lg bg-brand-panel text-text-secondary flex items-center justify-center border border-brand-border shrink-0"
    >
      <Bot class="w-4.5 h-4.5" />
    </div>

    <!-- Message Bubble Container -->
    <div class="max-w-[85%] space-y-1.5 flex flex-col">
      <div 
        class="rounded-2xl p-4 text-xs leading-relaxed transition-all duration-300"
        :class="message.role === 'user'
          ? 'bg-brand-panel text-text-primary rounded-tr-none border border-brand-border font-semibold shadow-[0_1px_2px_rgba(0,0,0,0.01)]'
          : 'bg-card-bg border border-brand-border text-text-primary rounded-tl-none shadow-sm'"
      >
        <!-- RAG Citations Section (Assistant replies only) -->
        <CitationList 
          v-if="message.role === 'assistant' && message.citations && message.citations.length > 0"
          :citations="message.citations"
          :hovered-index="hoveredIndex"
          class="mb-3 border-b border-brand-border/40 pb-3"
        />

        <!-- Rendered Text -->
        <div 
          v-if="message.role === 'assistant'"
          class="prose-custom break-words" 
          v-html="renderedText"
          @mouseover="handleMouseOver"
          @mouseout="handleMouseLeave"
        ></div>
        
        <!-- User raw text -->
        <div v-else class="whitespace-pre-line font-bold tracking-wide">{{ message.displayText }}</div>
        
        <!-- Model provider used tag -->
        <div 
          v-if="message.role === 'assistant' && message.provider" 
          class="text-[9px] text-text-secondary/70 font-extrabold uppercase tracking-wider mt-3.5 flex items-center gap-1.5 font-mono select-none"
        >
          <Layers class="w-3.5 h-3.5 text-brand-accent" /> Powered by {{ message.provider }}
        </div>
      </div>
    </div>

    <!-- User Avatar -->
    <div 
      v-if="message.role === 'user'" 
      class="h-8 w-8 rounded-lg bg-card-bg border border-brand-border text-text-secondary flex items-center justify-center shrink-0"
    >
      <User class="w-4.5 h-4.5" />
    </div>
  </div>
</template>
