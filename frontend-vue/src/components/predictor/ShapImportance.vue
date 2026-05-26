<script setup lang="ts">
import { TrendingUp } from '@lucide/vue'

interface Props {
  contributingFactors: Record<string, number>
  factors: string[]
}

defineProps<Props>()

const getFeatureLabel = (key: string) => {
  if (key === 'weather_index') return 'Weather Severity'
  if (key === 'route_id') return 'Logistics Route'
  if (key === 'carrier') return 'Carrier History'
  if (key === 'weight') return 'Shipment Weight'
  return key
}

const getFeaturePercentage = (val: number) => {
  return Math.round(val * 100)
}
</script>

<template>
  <div class="space-y-6 select-none">
    <!-- Feature Importance Explainers (Perplexity style dark gray bars) -->
    <div class="space-y-4">
      <h4 class="text-sm font-black uppercase tracking-widest text-text-secondary flex items-center gap-2 font-mono">
        <TrendingUp class="w-4 h-4 text-brand-accent animate-pulse" /> Explainable AI (SHAP Weights)
      </h4>
      
      <div class="space-y-3.5">
        <div 
          v-for="(val, key) in contributingFactors" 
          :key="key"
          class="space-y-1.5"
        >
          <div class="flex justify-between items-center text-sm font-semibold">
            <span class="text-text-secondary font-bold">{{ getFeatureLabel(String(key)) }}</span>
            <span class="text-text-primary font-mono font-bold">{{ getFeaturePercentage(val) }}%</span>
          </div>
          <!-- Visual horizontal bar -->
          <div class="h-2 bg-brand-panel rounded-full overflow-hidden border border-brand-border/40">
            <div 
              class="h-full bg-brand-accent rounded-full transition-all duration-1000 ease-out"
              :style="{ width: `${val * 100}%` }"
            ></div>
          </div>
        </div>
      </div>
    </div>

    <!-- Trigger Factors -->
    <div class="bg-brand-panel/40 rounded-2xl p-4 border border-brand-border space-y-2.5">
      <h4 class="text-sm font-black uppercase tracking-widest text-text-secondary font-mono">Diagnosis Justifications</h4>
      <ul class="list-disc pl-4 text-sm text-text-secondary space-y-1.5 leading-relaxed font-medium">
        <li v-for="(f, i) in factors" :key="i" class="marker:text-brand-accent">{{ f }}</li>
      </ul>
    </div>
  </div>
</template>
