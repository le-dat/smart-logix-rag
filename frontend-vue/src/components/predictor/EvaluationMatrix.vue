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
  <div class="flex flex-col sm:flex-row items-center gap-6 border-b border-[#e4e2d8] pb-6">
    <!-- Dial Circular Progress -->
    <div class="relative flex items-center justify-center shrink-0">
      <svg class="w-32 h-32 transform -rotate-90">
        <circle cx="64" cy="64" r="52" class="text-slate-100" stroke-width="6" stroke="currentColor" fill="transparent" />
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
        <p class="text-2xl font-extrabold text-[#1c1b17] font-mono">{{ Math.round(riskScore * 100) }}%</p>
        <p class="text-[9px] text-slate-400 uppercase font-bold tracking-widest mt-0.5">Risk Factor</p>
      </div>
    </div>

    <!-- Risk Badge details -->
    <div class="text-center sm:text-left space-y-2">
      <div class="flex flex-wrap items-center justify-center sm:justify-start gap-2">
        <span 
          class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded text-[10px] font-bold uppercase tracking-wider"
          :class="{
            'bg-emerald-100 text-emerald-800 border border-emerald-200/50': riskLevel === 'Low',
            'bg-amber-100 text-amber-800 border border-amber-200/50': riskLevel === 'Medium',
            'bg-rose-100 text-rose-800 border border-rose-200/50': riskLevel === 'High'
          }"
        >
          <ShieldAlert class="w-3.5 h-3.5" />
          {{ riskLevel }} Threat level
        </span>
        
        <!-- Fallback Warning Indicator -->
        <span 
          v-if="isFallback"
          class="inline-flex items-center gap-1 px-2 py-0.5 rounded text-[9px] font-bold uppercase tracking-wider bg-amber-100 text-amber-800 border border-amber-200/50"
        >
          Rule Fallback
        </span>
      </div>
      <h3 class="text-lg font-extrabold text-[#1c1b17]">Risk Evaluation Matrix</h3>
      <p class="text-slate-500 text-xs leading-relaxed">
        XGBoost classifier mapped safety indicators. Core factors contribute to standard operational metrics.
      </p>
    </div>
  </div>
</template>
