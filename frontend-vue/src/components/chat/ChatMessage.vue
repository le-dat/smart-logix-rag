<script setup lang="ts">
import { Bot, User, Layers } from '@lucide/vue'
import MarkdownIt from 'markdown-it'
import type { Message } from '../../types'
import CitationList from './CitationList.vue'

interface Props {
  message: Message
}

defineProps<Props>()
const emit = defineEmits<{
  toggleCitations: []
}>()

const md = new MarkdownIt({
  html: false,
  linkify: true,
  typographer: true
})
</script>

<template>
  <div 
    class="flex items-start gap-3.5 group"
    :class="message.role === 'user' ? 'justify-end' : 'justify-start'"
  >
    <!-- Bot Avatar -->
    <div 
      v-if="message.role === 'assistant'" 
      class="h-8 w-8 rounded bg-[#f3f2eb] text-slate-600 flex items-center justify-center border border-[#e4e2d8] shrink-0"
    >
      <Bot class="w-4.5 h-4.5" />
    </div>

    <!-- Message Bubble -->
    <div class="max-w-[80%] space-y-2">
      <div 
        class="rounded-2xl p-4 text-xs leading-relaxed"
        :class="message.role === 'user'
          ? 'bg-[#f3f2eb] text-[#1c1b17] rounded-tr-none border border-[#e4e2d8]'
          : 'bg-white border border-[#e4e2d8] text-[#1c1b17] rounded-tl-none font-medium'"
      >
        <!-- Rendered text -->
        <div 
          v-if="message.role === 'assistant'"
          class="prose-custom" 
          v-html="md.render(message.displayText || '')"
        ></div>
        <div v-else class="whitespace-pre-line font-bold">{{ message.displayText }}</div>
        
        <!-- Model provider used tag -->
        <div 
          v-if="message.role === 'assistant' && message.provider" 
          class="text-[9px] text-slate-400 font-bold uppercase tracking-wider mt-3 flex items-center gap-1 font-mono"
        >
          <Layers class="w-3 h-3 text-slate-400" /> Powered by {{ message.provider }}
        </div>
      </div>

      <!-- RAG Citations Section (AI replies only) -->
      <CitationList 
        v-if="message.role === 'assistant' && message.citations && message.citations.length > 0"
        :citations="message.citations"
        :show-citations="!!message.showCitations"
        @toggle="emit('toggleCitations')"
      />
    </div>

    <!-- User Avatar -->
    <div 
      v-if="message.role === 'user'" 
      class="h-8 w-8 rounded bg-white border border-[#e4e2d8] text-slate-600 flex items-center justify-center shrink-0"
    >
      <User class="w-4.5 h-4.5" />
    </div>
  </div>
</template>
