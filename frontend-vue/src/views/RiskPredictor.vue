<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { 
  ShieldAlert, 
  TrendingUp, 
  CloudRain, 
  Truck, 
  Scale, 
  ShieldCheck, 
  Compass, 
  Info, 
  ArrowLeft,
  RefreshCw
} from '@lucide/vue'

const route = useRoute()
const router = useRouter()

// Form State
const form = ref({
  route_id: 1,
  carrier: 'Dimerco Express',
  weight: 250.0,
  weather_index: 2.5
})

const trackingNo = ref('')
const senderName = ref('')

// Carriers List
const carriers = [
  'Dimerco Express',
  'DHL Logistics',
  'FedEx Supply Chain',
  'Cathay Pacific Cargo',
  'China Airlines Cargo'
]

// Routes List (seeded data matching)
const routes = [
  { id: 1, name: "Tan Son Nhat (SGN), VN ➔ Changi (SIN), SG (24h)", base: "SGN", dest: "SIN" },
  { id: 2, name: "Noi Bai (HAN), VN ➔ Incheon (ICN), KR (48h)", base: "HAN", dest: "ICN" },
  { id: 3, name: "Taoyuan (TPE), TW ➔ Noi Bai (HAN), VN (36h)", base: "TPE", dest: "HAN" },
  { id: 4, name: "Shanghai Pudong (PVG), CN ➔ Los Angeles (LAX), US (120h)", base: "PVG", dest: "LAX" },
  { id: 5, name: "Noi Bai (HAN), VN ➔ Frankfurt (FRA), DE (96h)", base: "HAN", dest: "FRA" }
]

// Prediction Result State
const loading = ref(false)
const predicted = ref(false)
const error = ref<string | null>(null)

interface PredictResult {
  risk_score: number // normalized (0.0 to 1.0)
  risk_level: string
  contributing_factors: Record<string, number>
  is_fallback: boolean
  factors: string[]
}

const result = ref<PredictResult | null>(null)

// Check for query parameters passed from Dashboard
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

// Computed circular dial parameters
const strokeDashoffset = computed(() => {
  if (!result.value) return 327
  const percent = result.value.risk_score
  return 327 - (327 * percent)
})

const dialColorClass = computed(() => {
  if (!result.value) return 'text-slate-400'
  const level = result.value.risk_level.toLowerCase()
  if (level === 'high') return 'text-rose-500'
  if (level === 'medium') return 'text-amber-500'
  return 'text-emerald-500'
})

// Call FastAPI backend
const handlePrediction = async () => {
  loading.value = true
  error.value = null
  predicted.value = false
  
  const apiPython = import.meta.env.VITE_API_PYTHON || 'http://localhost:8000'
  
  try {
    const response = await fetch(`${apiPython}/api/v1/risk/predict`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(form.value)
    })
    
    if (!response.ok) {
      throw new Error('API server returned error status')
    }
    
    const data = await response.json()
    result.value = {
      risk_score: data.risk_score / 100.0,
      risk_level: data.risk_level,
      contributing_factors: data.contributing_factors,
      is_fallback: data.is_fallback,
      factors: data.is_fallback
        ? ["Failed to read neural network splits. Rule-based estimates are active."]
        : Object.keys(data.contributing_factors).map(k => `${k} contributed significantly to this prediction.`)
    }
    predicted.value = true
  } catch (err: any) {
    console.error(err)
    simulateFallback()
  } finally {
    loading.value = false
  }
}

// Resilient Fallback Simulator
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
  
  const factors = []
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
  <div class="space-y-6 text-[#1c1b17]">
    <!-- Back Navigation -->
    <div class="flex items-center gap-3">
      <button 
        @click="router.push('/')" 
        class="glass-card p-2 rounded-lg text-slate-500 hover:text-black flex items-center justify-center cursor-pointer transition"
      >
        <ArrowLeft class="w-4 h-4" />
      </button>
      <h2 class="text-sm font-bold text-[#1c1b17]">Back to Dashboard</h2>
    </div>

    <!-- Main Container -->
    <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
      
      <!-- Left Side: Config Parameters Form -->
      <div class="lg:col-span-5 glass-card rounded-2xl p-6 shadow-sm space-y-5">
        <div>
          <h2 class="text-xl font-extrabold text-[#1c1b17] flex items-center gap-2">
            <Compass class="w-5.5 h-5.5 text-slate-600" /> AI Risk Profiler
          </h2>
          <p class="text-slate-500 text-xs mt-1">Configure shipment attributes to run XGBoost diagnostics.</p>
        </div>

        <!-- Info alert if shipment context is loaded -->
        <div v-if="trackingNo" class="bg-[#f3f2eb] border border-[#e4e2d8] rounded-xl p-3.5 flex gap-3 text-xs text-slate-600">
          <Info class="w-4.5 h-4.5 text-slate-600 shrink-0 mt-0.5" />
          <div>
            <span class="font-bold text-[#1c1b17]">Context Loaded:</span> Diagnosing shipment <span class="font-mono text-[#1c1b17] font-semibold">{{ trackingNo }}</span> from <span class="text-[#1c1b17] font-semibold">{{ senderName || 'Unknown' }}</span>.
          </div>
        </div>

        <form @submit.prevent="handlePrediction" class="space-y-4">
          <!-- Route -->
          <div>
            <label class="block text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1.5 flex items-center gap-1.5">
              <Compass class="w-3.5 h-3.5 text-slate-400" /> Route selection
            </label>
            <select 
              v-model="form.route_id" 
              class="glass-input w-full px-3 py-2 text-xs cursor-pointer"
            >
              <option v-for="r in routes" :key="r.id" :value="r.id">
                {{ r.name }}
              </option>
            </select>
          </div>

          <!-- Carrier -->
          <div>
            <label class="block text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1.5 flex items-center gap-1.5">
              <Truck class="w-3.5 h-3.5 text-slate-400" /> Logistics Carrier
            </label>
            <select 
              v-model="form.carrier" 
              class="glass-input w-full px-3 py-2 text-xs cursor-pointer"
            >
              <option v-for="c in carriers" :key="c" :value="c">
                {{ c }}
              </option>
            </select>
          </div>

          <!-- Weight -->
          <div>
            <label class="block text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1.5 flex items-center gap-1.5">
              <Scale class="w-3.5 h-3.5 text-slate-400" /> Net weight (kg)
            </label>
            <input 
              v-model.number="form.weight" 
              type="number" 
              step="0.01" 
              required
              class="glass-input w-full px-3 py-2 text-xs font-mono"
            />
          </div>

          <!-- Weather Index -->
          <div>
            <div class="flex justify-between items-center mb-1.5">
              <label class="text-[10px] font-bold uppercase tracking-wider text-slate-500 flex items-center gap-1.5">
                <CloudRain class="w-3.5 h-3.5 text-slate-400" /> Weather severity index
              </label>
              <span class="text-[10px] font-bold text-[#1c1b17] font-mono bg-[#f3f2eb] px-2 py-0.5 rounded">
                {{ form.weather_index.toFixed(1) }} / 5.0
              </span>
            </div>
            <input 
              v-model.number="form.weather_index" 
              type="range" 
              min="1.0" 
              max="5.0" 
              step="0.1" 
              class="w-full h-1 bg-[#e4e2d8] rounded-lg appearance-none cursor-pointer accent-[#1c1b17]"
            />
            <div class="flex justify-between text-[9px] text-slate-400 font-bold px-1 mt-1 font-mono">
              <span>Optimal</span>
              <span>Moderate</span>
              <span>Severe Risk</span>
            </div>
          </div>

          <!-- Submit Button -->
          <button 
            type="submit" 
            :disabled="loading"
            class="btn-capsule-primary w-full flex items-center justify-center gap-2 py-2.5 text-xs shadow-sm cursor-pointer disabled:opacity-50"
          >
            <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': loading }" />
            {{ loading ? 'Computing XGBoost Weights...' : 'Run Diagnostics' }}
          </button>
        </form>
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
            Select route and shipment attributes on the left panel, and click <span class="text-indigo-600 font-bold">"Run Diagnostics"</span> to evaluate logistical risks with XGBoost machine learning model.
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
                <p class="text-2xl font-extrabold text-[#1c1b17] font-mono">{{ Math.round(result.risk_score * 100) }}%</p>
                <p class="text-[9px] text-slate-400 uppercase font-bold tracking-widest mt-0.5">Risk Factor</p>
              </div>
            </div>

            <!-- Risk Badge details -->
            <div class="text-center sm:text-left space-y-2">
              <div class="flex flex-wrap items-center justify-center sm:justify-start gap-2">
                <span 
                  class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded text-[10px] font-bold uppercase tracking-wider"
                  :class="{
                    'bg-emerald-100 text-emerald-800 border border-emerald-200/50': result.risk_level === 'Low',
                    'bg-amber-100 text-amber-800 border border-amber-200/50': result.risk_level === 'Medium',
                    'bg-rose-100 text-rose-800 border border-rose-200/50': result.risk_level === 'High'
                  }"
                >
                  <ShieldAlert class="w-3.5 h-3.5" />
                  {{ result.risk_level }} Threat level
                </span>
                
                <!-- Fallback Warning Indicator -->
                <span 
                  v-if="result.is_fallback"
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

          <!-- Feature Importance Explainers (Perplexity style dark gray bars) -->
          <div class="space-y-4">
            <h4 class="text-[10px] font-bold uppercase tracking-wider text-slate-400 flex items-center gap-2">
              <TrendingUp class="w-4 h-4 text-slate-600" /> Explainable AI (SHAP Weights)
            </h4>
            
            <div class="space-y-3.5">
              <div 
                v-for="(val, key) in result.contributing_factors" 
                :key="key"
                class="space-y-1.5"
              >
                <div class="flex justify-between items-center text-xs font-semibold">
                  <span class="text-[#4a4943]">{{ getFeatureLabel(key) }}</span>
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
              <li v-for="(f, i) in result.factors" :key="i">{{ f }}</li>
            </ul>
          </div>

        </div>

      </div>

    </div>
  </div>
</template>
