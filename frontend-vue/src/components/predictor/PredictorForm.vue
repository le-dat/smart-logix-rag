<script setup lang="ts">
import { Compass, Info, RefreshCw, CloudRain } from '@lucide/vue'
import { ROUTES } from '../../constants/routes'
import { CARRIERS } from '../../constants/carriers'
import BaseCard from '../ui/BaseCard.vue'
import BaseSelect from '../ui/BaseSelect.vue'
import BaseInput from '../ui/BaseInput.vue'
import BaseButton from '../ui/BaseButton.vue'
import type { PredictionInput } from '../../types'

interface Props {
  loading?: boolean
  trackingNo?: string
  senderName?: string
}

defineProps<Props>()
const emit = defineEmits<{
  submit: []
}>()

const form = defineModel<PredictionInput>({ required: true })
</script>

<template>
  <BaseCard no-hover class="space-y-5 border border-brand-border bg-card-bg">
    <div>
      <h2 class="text-xl font-black text-text-primary flex items-center gap-2">
        <Compass class="w-5.5 h-5.5 text-brand-accent animate-spin-slow" /> AI Risk Profiler
      </h2>
      <p class="text-text-secondary text-xs mt-1">Configure shipment attributes to run XGBoost diagnostics.</p>
    </div>

    <!-- Info alert if shipment context is loaded -->
    <div 
      v-if="trackingNo" 
      class="bg-brand-panel border border-brand-border rounded-xl p-3.5 flex gap-3 text-xs text-text-secondary"
    >
      <Info class="w-4.5 h-4.5 text-brand-accent shrink-0 mt-0.5" />
      <div>
        <span class="font-bold text-text-primary">Context Loaded:</span> Diagnosing shipment <span class="font-mono text-text-primary font-semibold">{{ trackingNo }}</span> from <span class="text-text-primary font-semibold">{{ senderName || 'Unknown' }}</span>.
      </div>
    </div>

    <form @submit.prevent="emit('submit')" class="space-y-4">
      <!-- Route -->
      <BaseSelect
        v-model.number="form.route_id"
        label="Route Selection"
      >
        <option v-for="r in ROUTES" :key="r.id" :value="r.id">
          {{ r.name }}
        </option>
      </BaseSelect>

      <!-- Carrier -->
      <BaseSelect
        v-model="form.carrier"
        label="Logistics Carrier"
      >
        <option v-for="c in CARRIERS" :key="c" :value="c">
          {{ c }}
        </option>
      </BaseSelect>

      <!-- Weight -->
      <BaseInput
        v-model.number="form.weight"
        type="number"
        label="Net weight (kg)"
        required
      />

      <!-- Weather Index -->
      <div class="space-y-1.5 select-none">
        <div class="flex justify-between items-center">
          <label class="text-[10px] font-bold uppercase tracking-wider text-text-secondary flex items-center gap-1.5">
            <CloudRain class="w-3.5 h-3.5 text-text-secondary/70" /> Weather severity index
          </label>
          <span class="text-[10px] font-bold text-text-primary font-mono bg-brand-panel border border-brand-border px-2 py-0.5 rounded">
            {{ form.weather_index.toFixed(1) }} / 5.0
          </span>
        </div>
        <input 
          v-model.number="form.weather_index" 
          type="range" 
          min="1.0" 
          max="5.0" 
          step="0.1" 
          class="w-full h-1 bg-brand-border rounded-lg appearance-none cursor-pointer accent-brand-accent"
        />
        <div class="flex justify-between text-[8px] text-text-secondary/70 font-bold px-1 mt-1 font-mono">
          <span>Optimal</span>
          <span>Moderate</span>
          <span>Severe Risk</span>
        </div>
      </div>

      <!-- Submit Button -->
      <BaseButton 
        type="submit"
        :loading="loading"
        class="w-full !rounded-xl py-2.5 bg-text-primary text-brand-bg hover:bg-brand-accent hover:text-white transition shadow-sm select-none"
      >
        <template #icon>
          <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': loading }" />
        </template>
        {{ loading ? 'Computing XGBoost Weights...' : 'Run Diagnostics' }}
      </BaseButton>
    </form>
  </BaseCard>
</template>
