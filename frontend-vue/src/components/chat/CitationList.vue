<script setup lang="ts">
import { BookOpen, ChevronUp, ChevronDown } from '@lucide/vue'
import type { Citation } from '../../types'

interface Props {
  citations: Citation[]
  showCitations: boolean
}

defineProps<Props>()
const emit = defineEmits<{
  toggle: []
}>()
</script>

<template>
  <div class="space-y-1.5">
    <button 
      type="button"
      @click="emit('toggle')"
      class="flex items-center gap-1 text-[10px] font-extrabold uppercase tracking-wider text-slate-500 hover:text-black transition cursor-pointer"
    >
      <BookOpen class="w-3.5 h-3.5" />
      {{ showCitations ? 'Hide reference sources' : `Show retrieved references (${citations.length})` }}
      <ChevronUp v-if="showCitations" class="w-3 h-3" />
      <ChevronDown v-else class="w-3 h-3" />
    </button>

    <!-- Expanded citations list -->
    <div 
      v-if="showCitations" 
      class="bg-[#f9f8f4] rounded-xl p-3 border border-[#e4e2d8] divide-y divide-[#e4e2d8] space-y-2.5 animate-in slide-in-from-top-2 duration-200"
    >
      <div 
        v-for="(c, idx) in citations" 
        :key="idx"
        class="text-[11px] space-y-1 pt-2 first:pt-0 text-slate-600"
      >
        <div class="flex justify-between items-center text-slate-500 font-bold font-mono text-[9px]">
          <span>Source: {{ c.source }}</span>
          <span class="text-slate-400">Match {{ idx + 1 }}</span>
        </div>
        <p class="text-slate-700 italic pl-2.5 border-l-2 border-slate-400 leading-normal bg-white/50 py-1 rounded">
          "{{ c.content_snippet }}"
        </p>
      </div>
    </div>
  </div>
</template>
