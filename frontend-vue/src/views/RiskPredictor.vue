<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute as useVueRoute, useRouter as useVueRouter } from 'vue-router'
import { ShieldCheck, ArrowLeft, RefreshCw } from '@lucide/vue'
import { predictionService } from '../services/predictionService'
import type { PredictionInput, PredictionResult } from '../types'

// Modular Components
import PredictorForm from '../components/predictor/PredictorForm.vue'
import EvaluationMatrix from '../components/predictor/EvaluationMatrix.vue'
import ShapImportance from '../components/predictor/ShapImportance.vue'

const route = useVueRoute()
const router = useVueRouter()

// Form parameters configuration state
const form = ref<PredictionInput>({
  route_id: 1,
  carrier: 'Dimerco Express',
  weight: 250.0,
  weather_index: 2.5
})

const trackingNo = ref('')
const senderName = ref('')

// ML Inference diagnostics state
const loading = ref(false)
const predicted = ref(false)
const error = ref<string | null>(null)
const result = ref<PredictionResult | null>(null)

// Load query parameters passed from Dashboard context
onMounted(() => {
  if (route.query.route_id) {
    form.value.route_id = Number(route.query.route_id)
  }
  if (route.query.weight) {
    form.value.weight = Number(route.query.weight)
  }
  if (route.query.tracking) {
    trackingNo.value = String(route.query.tracking)
  }
  if (route.query.sender) {
    senderName.value = String(route.query.sender)
  }
})

// Trigger FastAPI inference or offline ML simulation fallback
const handlePrediction = async () => {
  loading.value = true
  error.value = null
  predicted.value = false
  
  try {
    result.value = await predictionService.predict(form.value)
    predicted.value = true
  } catch (err: any) {
    console.error('API prediction failed, running offline simulation.', err)
    simulateFallback()
  } finally {
    loading.value = false
  }
}

// Resilient fallback estimator based on structural logic
const simulateFallback = () => {
  let score = 0.15
  if (form.value.route_id === 4) score += 0.35
  if (form.value.weather_index > 3.5) score += 0.30
  if (form.value.weight > 3000) score += 0.15
  if (form.value.carrier === 'DHL Logistics') score -= 0.05
  
  score = Math.min(Math.max(score, 0.05), 0.95)
  
  let level = 'Low'
  if (score > 0.65) level = 'High'
  else if (score > 0.30) level = 'Medium'
  
  const wIdx = form.value.weather_index > 3.5 ? 0.45 : 0.20
  const rIdx = form.value.route_id === 4 ? 0.35 : 0.15
  const cIdx = 0.20
  const wtIdx = 0.15
  const total = wIdx + rIdx + cIdx + wtIdx
  
  const factors: string[] = []
  if (form.value.weather_index > 3.5) {
    factors.push('Severe weather indices encountered along flyway routing.')
  }
  if (form.value.route_id === 4) {
    factors.push('Cross-Pacific long-haul routes exhibit higher average congestion indices.')
  }
  if (form.value.weight > 2000) {
    factors.push('Heavy-weight freight requires complex container balancing checks.')
  }
  if (factors.length === 0) {
    factors.push('Standard freight parameters meet optimal seasonal safety profiles.')
  }

  result.value = {
    risk_score: score,
    risk_level: level,
    contributing_factors: {
      'weather_index': wIdx / total,
      'route_id': rIdx / total,
      'carrier': cIdx / total,
      'weight': wtIdx / total
    },
    is_fallback: true,
    factors
  }
  predicted.value = true
}
</script>

<template>
  <div class="space-y-6 text-[#1c1b17]">
    <!-- Back Navigation -->
    <div class="flex items-center gap-3">
      <button 
        @click="router.push('/')" 
        class="glass-card p-2 rounded-lg text-slate-500 hover:text-black flex items-center justify-center cursor-pointer transition"
        aria-label="Navigate back to dashboard"
      >
        <ArrowLeft class="w-4 h-4" />
      </button>
      <h2 class="text-sm font-bold text-[#1c1b17]">Back to Dashboard</h2>
    </div>

    <!-- Main Container -->
    <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
      
      <!-- Left Side: Config Parameters Form -->
      <div class="lg:col-span-5">
        <PredictorForm 
          v-model="form"
          :loading="loading"
          :tracking-no="trackingNo"
          :sender-name="senderName"
          @submit="handlePrediction"
        />
      </div>

      <!-- Right Side: Diagnostics Explainer Dashboard -->
      <div class="lg:col-span-7 glass-card rounded-2xl p-6 shadow-sm flex flex-col justify-center min-h-[450px]">
        
        <!-- Welcome Screen if not predicted yet -->
        <div v-if="!predicted && !loading" class="text-center py-12 space-y-4 max-w-sm mx-auto">
          <div class="inline-flex bg-[#f3f2eb] p-5 rounded-full text-slate-500 border border-[#e4e2d8]">
            <ShieldCheck class="w-12 h-12" />
          </div>
          <h3 class="text-lg font-extrabold text-[#1c1b17]">Diagnostics Awaiting Run</h3>
          <p class="text-slate-500 text-xs leading-relaxed">
            Select route and shipment attributes on the left panel, and click <span class="text-black font-bold">"Run Diagnostics"</span> to evaluate logistical risks with XGBoost machine learning model.
          </p>
        </div>

        <!-- Loading Spinner -->
        <div v-if="loading" class="text-center py-12 space-y-4">
          <div class="inline-flex animate-spin text-slate-700">
            <RefreshCw class="w-10 h-10" />
          </div>
          <h3 class="text-base font-bold text-[#1c1b17]">Running XGBoost Inference...</h3>
          <p class="text-slate-400 text-xs font-medium">Extracting feature importance splits from tree nodes.</p>
        </div>

        <!-- Result Content -->
        <div v-if="predicted && !loading && result" class="space-y-6 animate-in fade-in duration-300">
          <!-- Circular Evaluation Matrix -->
          <EvaluationMatrix 
            :risk-score="result.risk_score"
            :risk-level="result.risk_level"
            :is-fallback="result.is_fallback"
          />

          <!-- SHAP Feature Importance Bars -->
          <ShapImportance 
            :contributing-factors="result.contributing_factors"
            :factors="result.factors"
          />
        </div>

      </div>

    </div>
  </div>
</template>
