<script setup lang="ts">
import MarkdownIt from 'markdown-it'
import { computed, ref } from 'vue'
import type { Message } from '../../types'

import ResearchSteps from './ResearchSteps.vue'

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

const parsedMessage = computed(() => {
  const text = props.message.displayText || ''
  
  // Extract content inside <think>...</think> (or to the end of string if still streaming)
  const thinkMatch = text.match(/<think>([\s\S]*?)(?:<\/think>|$)/)
  
  let thinkingContent = ''
  let answerContent = text
  
  if (thinkMatch) {
    thinkingContent = thinkMatch[1].trim()
    answerContent = text.replace(/<think>[\s\S]*?<\/think>/g, '').trim()
    
    // If the assistant is still in the middle of thinking, do not render the empty answer yet
    if (!text.includes('</think>')) {
      answerContent = ''
    }
  }
  
  // Render clean markdown
  const html = md.render(answerContent)
  
  // Post-process footnote citations [1], [2]... into premium styled inline pills
  const renderedAnswer = html.replace(/\[([0-9]+)\]/g, (_, num) => {
    const index = Number(num)
    const citation = props.message.citations?.[index - 1]
    
    // Clean citation source (e.g. "dimerco_faq.txt" -> base filename)
    let sourceName = 'Source'
    if (citation && citation.source) {
      sourceName = citation.source.split('/').pop() || citation.source
      if (sourceName.length > 25) {
        sourceName = sourceName.slice(0, 22) + '...'
      }
    }
    
    return `<span class="footnote-pill inline-flex items-center gap-1.5 px-2 py-0.5 mx-0.5 rounded-full text-xs font-black bg-brand-panel hover:bg-brand-accent/20 hover:text-brand-accent border border-brand-border/60 hover:border-brand-accent/80 text-text-secondary transition-all cursor-pointer select-none font-sans" data-index="${index}">${sourceName} <span class="text-brand-accent text-[10px] ml-0.5 font-bold">+${index}</span></span>`
  })
  
  return {
    thinking: thinkingContent,
    answer: renderedAnswer,
    isThinking: text.includes('<think>') && !text.includes('</think>')
  }
})

// Event delegation to capture mouse hovers on superscript footnotes or footnote pills
const handleMouseOver = (event: MouseEvent) => {
  const target = event.target as HTMLElement
  const pill = target.closest('.footnote-pill')
  if (pill) {
    const idx = pill.getAttribute('data-index')
    if (idx) {
      hoveredIndex.value = Number(idx)
    }
  } else if (target.classList.contains('footnote-ref')) {
    // Fallback for legacy [1] superscript footnotes if any
    const idx = target.getAttribute('data-index')
    if (idx) {
      hoveredIndex.value = Number(idx)
    }
  }
}

const handleMouseLeave = () => {
  hoveredIndex.value = null
}

</script>

<template>
  <div 
    class="flex items-start gap-3.5 group select-text"
    :class="message.role === 'user' ? 'justify-end' : 'justify-start'"
  >
    <!-- Message Bubble Container -->
    <div 
      class="w-full space-y-1.5 flex flex-col pt-0.5"
      :class="message.role === 'user' ? 'ml-auto' : 'mr-auto'"
    >
      <div
        class="p-4 text-sm leading-relaxed transition-all duration-300"
        :class="message.role === 'user' ? 'ml-auto' : ''"
      >
        <!-- <CitationList 
          v-if="message.role === 'assistant' && message.citations && message.citations.length > 0"
          :citations="message.citations"
          :hovered-index="hoveredIndex"
          class="mb-3 border-b border-brand-border/40 pb-3"
        /> -->

        <ResearchSteps 
          v-if="message.role === 'assistant'"
          :steps="message.steps"
        />

        <!-- <ThinkingAccordion 
          v-if="parsedMessage.thinking"
          :thinking="parsedMessage.thinking"
          :is-thinking="parsedMessage.isThinking"
        /> -->

        <!-- Rendered Text -->
        <div 
          v-if="message.role === 'assistant' && parsedMessage.answer"
          class="prose-custom break-words" 
          v-html="parsedMessage.answer"
          @mouseover="handleMouseOver"
          @mouseout="handleMouseLeave"
        ></div>
        
        <div v-else-if="message.role === 'user'" class="whitespace-pre-line font-bold tracking-wide text-right">{{ message.displayText }}</div>
      </div>
    </div>

  </div>
</template>
