<script setup lang="ts">
import { computed } from 'vue'
import { ShieldAlert } from '@lucide/vue'

interface Props {
  riskScore: number // 0.0 to 1.0
  riskLevel: string
  isFallback?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  isFallback: false
})

// Circular dial SVG math
const strokeDashoffset = computed(() => {
  const percent = props.riskScore
  return 327 - (327 * percent)
})

const dialColorClass = computed(() => {
  const level = props.riskLevel.toLowerCase()
  if (level === 'high') return 'text-rose-500'
  if (level === 'medium') return 'text-amber-500'
  return 'text-emerald-500'
})
</script>

<template>
  <div class="flex flex-col sm:flex-row items-center gap-6 border-b border-brand-border pb-6 select-none">
    <!-- Dial Circular Progress -->
    <div class="relative flex items-center justify-center shrink-0">
      <svg class="w-32 h-32 transform -rotate-90">
        <circle cx="64" cy="64" r="52" class="text-brand-panel border border-brand-border/40" stroke-width="6" stroke="currentColor" fill="transparent" />
        <circle 
          cx="64" 
          cy="64" 
          r="52" 
          :class="dialColorClass" 
          stroke-width="6" 
          stroke="currentColor" 
          fill="transparent" 
          stroke-linecap="round"
          :stroke-dasharray="327"
          :stroke-dashoffset="strokeDashoffset"
          class="transition-all duration-1000 ease-out"
        />
      </svg>
      <div class="absolute text-center">
        <p class="text-2xl font-black text-text-primary font-mono leading-none">{{ Math.round(riskScore * 100) }}%</p>
        <p class="text-[8px] text-text-secondary uppercase font-extrabold tracking-widest mt-1 font-mono">Risk Factor</p>
      </div>
    </div>

    <!-- Risk Badge details -->
    <div class="text-center sm:text-left space-y-2.5">
      <div class="flex flex-wrap items-center justify-center sm:justify-start gap-2">
        <span 
          class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded text-[9px] font-black uppercase tracking-wider border"
          :class="{
            'bg-emerald-500/10 text-emerald-500 border-emerald-500/25': riskLevel === 'Low',
            'bg-amber-500/10 text-amber-500 border-amber-500/25': riskLevel === 'Medium',
            'bg-rose-500/10 text-rose-500 border-rose-500/25': riskLevel === 'High'
          }"
        >
          <ShieldAlert class="w-3.5 h-3.5 shrink-0" />
          {{ riskLevel }} Threat level
        </span>
        
        <!-- Fallback Warning Indicator -->
        <span 
          v-if="isFallback"
          class="inline-flex items-center gap-1 px-2 py-0.5 rounded text-[9px] font-black uppercase tracking-wider bg-amber-500/10 text-amber-500 border border-amber-500/25"
        >
          Rule Fallback
        </span>
      </div>
      <h3 class="text-lg font-black text-text-primary">Risk Evaluation Matrix</h3>
      <p class="text-text-secondary text-xs leading-relaxed max-w-sm">
        XGBoost classifier mapped safety indicators. Core factors contribute to standard operational metrics.
      </p>
    </div>
  </div>
</template>
