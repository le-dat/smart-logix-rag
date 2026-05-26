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
  <div class="space-y-6">
    <!-- Feature Importance Explainers (Perplexity style dark gray bars) -->
    <div class="space-y-4">
      <h4 class="text-[10px] font-bold uppercase tracking-wider text-slate-400 flex items-center gap-2">
        <TrendingUp class="w-4 h-4 text-slate-600" /> Explainable AI (SHAP Weights)
      </h4>
      
      <div class="space-y-3.5">
        <div 
          v-for="(val, key) in contributingFactors" 
          :key="key"
          class="space-y-1.5"
        >
          <div class="flex justify-between items-center text-xs font-semibold">
            <span class="text-[#4a4943]">{{ getFeatureLabel(String(key)) }}</span>
            <span class="text-[#1c1b17] font-mono font-bold">{{ getFeaturePercentage(val) }}%</span>
          </div>
          <!-- Visual horizontal bar -->
          <div class="h-2 bg-[#f3f2eb] rounded-full overflow-hidden">
            <div 
              class="h-full bg-[#1c1b17] rounded-full transition-all duration-1000 ease-out"
              :style="{ width: `${val * 100}%` }"
            ></div>
          </div>
        </div>
      </div>
    </div>

    <!-- Trigger Factors -->
    <div class="bg-[#f3f2eb]/60 rounded-xl p-4 border border-[#e4e2d8] space-y-2">
      <h4 class="text-[10px] font-bold uppercase tracking-wider text-slate-500">Diagnosis Justifications</h4>
      <ul class="list-disc pl-4 text-xs text-slate-600 space-y-1.5 leading-relaxed">
        <li v-for="(f, i) in factors" :key="i">{{ f }}</li>
      </ul>
    </div>
  </div>
</template>
