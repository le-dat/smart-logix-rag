<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { 
  Check, 
  Database, 
  FileText,
  RefreshCw, 
  Search, 
  Sparkles, 
  Zap 
} from '@lucide/vue'
import type { StepInfo } from '../../types'

interface Props {
  steps?: StepInfo[]
}

const props = withDefaults(defineProps<Props>(), {
  steps: () => []
})

// Track accordion open/close state — starts open
const isOpen = ref(true)

// Whether all steps have completed
const allDone = computed(() => {
  const s = props.steps
  return s.length > 0 && s.every(step => step.status === 'completed')
})

const hasRunningStep = computed(() =>
  props.steps.some(s => s.status === 'running')
)

const accordionTitle = computed(() => {
  const steps = props.steps
  const completed = steps.filter(s => s.status === 'completed').length
  const total = steps.length

  if (completed === total && total > 0) return `Completed ${total} steps`

  const active = steps.find(s => s.status === 'running')
  if (active) return active.title

  return 'Processing RAG research steps...'
})

// Auto-close 600ms after all steps are done
watch(allDone, (done) => {
  if (done) {
    setTimeout(() => { isOpen.value = false }, 600)
  }
})

// Toggle manually
const toggle = () => { isOpen.value = !isOpen.value }

// --- Smooth height animation via JS hooks ---
const onBeforeEnter = (el: Element) => {
  (el as HTMLElement).style.maxHeight = '0'
  ;(el as HTMLElement).style.opacity = '0'
}

const onEnter = (el: Element, done: () => void) => {
  const htmlEl = el as HTMLElement
  // Force reflow
  htmlEl.offsetHeight
  htmlEl.style.transition = 'max-height 0.35s cubic-bezier(0.4, 0, 0.2, 1), opacity 0.25s ease'
  htmlEl.style.maxHeight = htmlEl.scrollHeight + 'px'
  htmlEl.style.opacity = '1'
  htmlEl.addEventListener('transitionend', done, { once: true })
}

const onAfterEnter = (el: Element) => {
  (el as HTMLElement).style.maxHeight = ''
  ;(el as HTMLElement).style.opacity = ''
  ;(el as HTMLElement).style.transition = ''
}

const onBeforeLeave = (el: Element) => {
  const htmlEl = el as HTMLElement
  htmlEl.style.maxHeight = htmlEl.scrollHeight + 'px'
  htmlEl.style.opacity = '1'
  // Force reflow
  htmlEl.offsetHeight
}

const onLeave = (el: Element, done: () => void) => {
  const htmlEl = el as HTMLElement
  htmlEl.style.transition = 'max-height 0.3s cubic-bezier(0.4, 0, 0.2, 1), opacity 0.2s ease'
  htmlEl.style.maxHeight = '0'
  htmlEl.style.opacity = '0'
  htmlEl.addEventListener('transitionend', done, { once: true })
}

const onAfterLeave = (el: Element) => {
  const htmlEl = el as HTMLElement
  htmlEl.style.maxHeight = ''
  htmlEl.style.opacity = ''
  htmlEl.style.transition = ''
}

// Strip path from citation source to just filename
const shortSource = (source: string): string => {
  const name = source.split('/').pop() ?? source
  return name.length > 32 ? name.slice(0, 29) + '…' : name
}
</script>

<template>
  <div v-if="steps && steps.length > 0" class="mb-4">
    <!-- Accordion Header (clickable) -->
    <button
      type="button"
      class="w-full flex items-center gap-2 px-3 py-2.5 rounded-xl font-bold text-xs text-text-secondary select-none cursor-pointer hover:bg-brand-panel transition-colors duration-200 bg-brand-panel/40 border border-brand-border/60"
      :class="{ 'rounded-b-none border-b-0': isOpen }"
      @click="toggle"
      :aria-expanded="isOpen"
    >
      <!-- Status icon -->
      <RefreshCw
        v-if="hasRunningStep"
        class="w-3.5 h-3.5 text-brand-accent shrink-0 animate-spin"
      />
      <Check v-else-if="allDone" class="w-3.5 h-3.5 text-brand-accent shrink-0" />
      <RefreshCw v-else class="w-3.5 h-3.5 text-text-secondary/50 shrink-0" />

      <span class="flex-1 text-left transition-colors duration-300" :class="{ 'text-brand-accent': hasRunningStep }">
        {{ accordionTitle }}
      </span>

      <!-- Chevron rotates on open -->
      <span
        class="text-text-secondary/50 transition-transform duration-300 text-[10px]"
        :class="{ 'rotate-180': isOpen }"
      >▼</span>
    </button>

    <!-- Animated content area -->
    <Transition
      :css="false"
      @before-enter="onBeforeEnter"
      @enter="onEnter"
      @after-enter="onAfterEnter"
      @before-leave="onBeforeLeave"
      @leave="onLeave"
      @after-leave="onAfterLeave"
    >
      <div
        v-if="isOpen"
        class="overflow-hidden border border-t-0 border-brand-border/60 rounded-b-xl bg-brand-panel/20"
      >
        <div class="px-4 pb-4 pt-3 space-y-3 relative">
          <!-- Vertical Timeline connecting line -->
          <div class="absolute left-[26px] top-6 bottom-8 w-px bg-brand-border/50 pointer-events-none" />

          <!-- Step rows -->
          <div
            v-for="step in steps"
            :key="step.id"
            class="flex flex-col gap-1.5 relative z-10"
          >
            <!-- Step header row -->
            <div class="flex items-center gap-3">
              <!-- Step icon -->
              <div
                class="h-6 w-6 rounded-full flex items-center justify-center shrink-0 border transition-all duration-300 bg-brand-panel"
                :class="step.status === 'completed'
                  ? 'text-brand-accent border-brand-accent shadow-[0_0_8px_rgba(16,185,129,0.2)]'
                  : step.status === 'running'
                    ? 'text-brand-accent border-brand-accent bg-brand-accent-glow animate-pulse shadow-[0_0_12px_rgba(16,185,129,0.35)]'
                    : 'text-text-secondary/40 border-brand-border'"
              >
                <Search v-if="step.icon === 'search'" class="w-3 h-3" />
                <Database v-else-if="step.icon === 'db'" class="w-3 h-3" />
                <Sparkles v-else-if="step.icon === 'insight'" class="w-3 h-3" />
                <Zap v-else-if="step.icon === 'llm'" class="w-3 h-3" />
              </div>

              <!-- Step label -->
              <span
                class="text-xs font-bold transition-colors duration-300 flex-1"
                :class="step.status === 'completed'
                  ? 'text-text-primary'
                  : step.status === 'running'
                    ? 'text-brand-accent'
                    : 'text-text-secondary/50'"
              >
                {{ step.title }}
              </span>

              <!-- Status marker -->
              <div class="ml-auto flex items-center shrink-0">
                <div
                  v-if="step.status === 'running'"
                  class="h-2 w-2 rounded-full bg-brand-accent animate-ping"
                />
                <Check v-else-if="step.status === 'completed'" class="w-3 h-3 text-brand-accent" />
              </div>
            </div>

            <!-- Inline citations sub-list (only when step is completed & has citations) -->
            <div
              v-if="step.status === 'completed' && step.citations && step.citations.length > 0"
              class="ml-9 flex flex-col md:flex-row gap-x-2 gap-y-1"
            >
              <div
                v-for="(cit, ci) in step.citations"
                :key="ci"
                class="flex items-center gap-1.5 px-2 py-1 rounded-lg bg-brand-panel/60 border border-brand-border/40 group/cit"
              >
                <FileText class="w-2.5 h-2.5 text-brand-accent/70 shrink-0" />
                <span class="text-xs text-text-secondary/80 font-mono truncate group-hover/cit:text-text-primary transition-colors">
                  {{ shortSource(cit.source) }}
                </span>
                <span class="ml-auto text-[10px] font-bold text-brand-accent/60 shrink-0 uppercase tracking-wider">
                  RAG
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>
