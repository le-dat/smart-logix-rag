<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useLogisticsStore } from '../stores/logistics'
import { 
  Package, 
  Navigation, 
  TrendingUp, 
  AlertTriangle, 
  Search, 
  Filter, 
  Plus, 
  Trash2, 
  ArrowUpRight, 
  Activity,
  User,
  X,
  RefreshCw
} from '@lucide/vue'

const router = useRouter()
const store = useLogisticsStore()

// State
const searchQuery = ref('')
const selectedStatus = ref('All')
const isAddModalOpen = ref(false)

// New Shipment Form State
const newShipment = ref({
  trackingNo: '',
  sender: '',
  receiver: '',
  routeId: 1,
  customerId: 1,
  weight: 150.0,
  status: 'InTransit'
})

// Hardcoded routes and customers for high-fidelity fallback/creation
const routes = [
  { id: 1, name: "Tan Son Nhat (SGN), VN ➔ Changi (SIN), SG (24h)" },
  { id: 2, name: "Noi Bai (HAN), VN ➔ Incheon (ICN), KR (48h)" },
  { id: 3, name: "Taoyuan (TPE), TW ➔ Noi Bai (HAN), VN (36h)" },
  { id: 4, name: "Shanghai Pudong (PVG), CN ➔ Los Angeles (LAX), US (120h)" },
  { id: 5, name: "Noi Bai (HAN), VN ➔ Frankfurt (FRA), DE (96h)" }
]

onMounted(async () => {
  await store.fetchShipments()
  await store.fetchCustomers()
  
  // Set default customer from store if available
  if (store.customers.length > 0) {
    newShipment.value.customerId = store.customers[0].id
  }
})

// Computed KPI Stats
const totalShipments = computed(() => store.shipments.length)

// We can assign or compute a risk score for each shipment.
// For seed shipments we can simulate or query them. Let's add simulated risk mapping
const shipmentsWithRisk = computed(() => {
  return store.shipments.map(s => {
    // Generate high-fidelity simulated risk based on route and weight if not exists
    let score = 0.15;
    if (s.routeId === 4) score = 0.78; // PVG to LAX (longer duration, high risk)
    else if (s.routeId === 5) score = 0.42; // HAN to FRA (medium duration)
    else if (s.weight > 3000) score = 0.55; // Heavy weight raises risk slightly
    
    // Status completed has 0 risk
    if (s.status === 'Delivered') score = 0.05;
    
    let level = 'Low';
    if (score > 0.7) level = 'High';
    else if (score > 0.3) level = 'Medium';

    return {
      ...s,
      riskScore: s.riskScore || score,
      riskLevel: s.riskLevel || level
    }
  })
})

const avgRiskIndex = computed(() => {
  if (shipmentsWithRisk.value.length === 0) return 0
  const total = shipmentsWithRisk.value.reduce((acc, curr) => acc + curr.riskScore, 0)
  return Math.round((total / shipmentsWithRisk.value.length) * 100)
})

const highRiskCount = computed(() => {
  return shipmentsWithRisk.value.filter(s => s.riskLevel === 'High').length
})

// Filtered Shipments
const filteredShipments = computed(() => {
  return shipmentsWithRisk.value.filter(s => {
    const matchesSearch = 
      s.trackingNo.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      s.sender.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      s.receiver.toLowerCase().includes(searchQuery.value.toLowerCase())
    
    const matchesStatus = selectedStatus.value === 'All' || s.status === selectedStatus.value
    
    return matchesSearch && matchesStatus
  })
})

// Actions
const handleRefresh = async () => {
  await store.fetchShipments()
}

const openAddModal = () => {
  // Generate random tracking number
  const rand = Math.floor(1000 + Math.random() * 9000)
  newShipment.value.trackingNo = `DMCO-VN-2026${rand}`
  isAddModalOpen.value = true
}

const handleAddShipment = async () => {
  try {
    await store.addShipment(newShipment.value)
    isAddModalOpen.value = false
    // Reset form
    newShipment.value = {
      trackingNo: '',
      sender: '',
      receiver: '',
      routeId: 1,
      customerId: store.customers.length > 0 ? store.customers[0].id : 1,
      weight: 150.0,
      status: 'InTransit'
    }
  } catch (err) {
    alert('Failed to add shipment: ' + (err as Error).message)
  }
}

const handleDelete = async (id: number) => {
  if (confirm('Are you sure you want to delete this shipment?')) {
    try {
      await store.deleteShipment(id)
    } catch (err) {
      alert('Failed to delete shipment')
    }
  }
}

const navigateToPredict = (shipment: any) => {
  // Navigate to Risk Predictor and pass shipment details via router state/query
  router.push({
    path: '/predict',
    query: {
      route_id: shipment.routeId,
      weight: shipment.weight,
      sender: shipment.sender,
      tracking: shipment.trackingNo
    }
  })
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 border-b border-white/5 pb-5">
      <div>
        <h1 class="text-3xl font-extrabold tracking-tight bg-gradient-to-r from-white via-slate-200 to-indigo-400 bg-clip-text text-transparent">
          Logistics Control Center
        </h1>
        <p class="text-slate-400 text-sm mt-1">Real-time container logs, tracking, and predictive AI dispatch.</p>
      </div>
      <div class="flex items-center gap-3">
        <button 
          @click="handleRefresh" 
          class="glass-card p-2.5 rounded-lg text-slate-400 hover:text-white flex items-center justify-center cursor-pointer"
          :class="{ 'animate-spin': store.loading }"
        >
          <RefreshCw class="w-5 h-5" />
        </button>
        <button 
          @click="openAddModal" 
          class="flex items-center gap-2 bg-indigo-600 hover:bg-indigo-500 text-white font-semibold py-2.5 px-4 rounded-lg shadow-lg hover:shadow-indigo-500/20 transition duration-200 cursor-pointer text-sm"
        >
          <Plus class="w-4 h-4" /> Add Shipment
        </button>
      </div>
    </div>

    <!-- KPIs Row -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
      <!-- KPI 1 -->
      <div class="glass-card p-5 rounded-xl glow-indigo relative overflow-hidden">
        <div class="absolute -right-4 -bottom-4 text-indigo-500/5">
          <Package class="w-32 h-32" />
        </div>
        <div class="flex justify-between items-start">
          <div class="bg-indigo-500/10 p-2.5 rounded-lg text-indigo-400">
            <Package class="w-6 h-6" />
          </div>
          <span class="text-xs font-semibold text-emerald-400 bg-emerald-500/10 py-1 px-2 rounded-full flex items-center gap-1">
            Active
          </span>
        </div>
        <div class="mt-4">
          <h3 class="text-slate-400 text-xs font-bold uppercase tracking-wider">Total Shipments</h3>
          <p class="text-3xl font-extrabold mt-1 text-white">{{ totalShipments }}</p>
        </div>
      </div>

      <!-- KPI 2 -->
      <div class="glass-card p-5 rounded-xl glow-emerald relative overflow-hidden">
        <div class="absolute -right-4 -bottom-4 text-emerald-500/5">
          <TrendingUp class="w-32 h-32" />
        </div>
        <div class="flex justify-between items-start">
          <div class="bg-emerald-500/10 p-2.5 rounded-lg text-emerald-400">
            <TrendingUp class="w-6 h-6" />
          </div>
          <span class="text-xs font-semibold text-emerald-400 bg-emerald-500/10 py-1 px-2 rounded-full">
            Healthy
          </span>
        </div>
        <div class="mt-4">
          <h3 class="text-slate-400 text-xs font-bold uppercase tracking-wider">Average Risk Score</h3>
          <p class="text-3xl font-extrabold mt-1 text-white">{{ avgRiskIndex }}%</p>
        </div>
      </div>

      <!-- KPI 3 -->
      <div class="glass-card p-5 rounded-xl glow-crimson relative overflow-hidden">
        <div class="absolute -right-4 -bottom-4 text-red-500/5">
          <AlertTriangle class="w-32 h-32" />
        </div>
        <div class="flex justify-between items-start">
          <div class="bg-red-500/10 p-2.5 rounded-lg text-red-400">
            <AlertTriangle class="w-6 h-6" />
          </div>
          <span class="text-xs font-semibold text-red-400 bg-red-500/10 py-1 px-2 rounded-full">
            Alert
          </span>
        </div>
        <div class="mt-4">
          <h3 class="text-slate-400 text-xs font-bold uppercase tracking-wider">High Risk Warnings</h3>
          <p class="text-3xl font-extrabold mt-1 text-white">{{ highRiskCount }}</p>
        </div>
      </div>

      <!-- KPI 4 -->
      <div class="glass-card p-5 rounded-xl relative overflow-hidden">
        <div class="absolute -right-4 -bottom-4 text-purple-500/5">
          <Activity class="w-32 h-32" />
        </div>
        <div class="flex justify-between items-start">
          <div class="bg-purple-500/10 p-2.5 rounded-lg text-purple-400">
            <Activity class="w-6 h-6" />
          </div>
          <span class="text-xs font-semibold text-purple-400 bg-purple-500/10 py-1 px-2 rounded-full">
            RAG Online
          </span>
        </div>
        <div class="mt-4">
          <h3 class="text-slate-400 text-xs font-bold uppercase tracking-wider">Active Services</h3>
          <p class="text-3xl font-extrabold mt-1 text-white">4 / 4</p>
        </div>
      </div>
    </div>

    <!-- Data Controls & Table -->
    <div class="glass-panel rounded-xl overflow-hidden shadow-2xl">
      <!-- Search & Filters -->
      <div class="p-5 border-b border-white/5 bg-slate-900/30 flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div class="relative flex-1 max-w-md">
          <span class="absolute inset-y-0 left-0 pl-3 flex items-center text-slate-500">
            <Search class="w-4 h-4" />
          </span>
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="Search by tracking number, sender, or receiver..." 
            class="glass-input pl-10 pr-4 py-2 w-full text-sm"
          />
        </div>
        <div class="flex items-center gap-3">
          <div class="flex items-center gap-2 text-slate-400 text-sm">
            <Filter class="w-4 h-4" />
            <span>Status:</span>
          </div>
          <select 
            v-model="selectedStatus" 
            class="glass-input px-3 py-1.5 text-sm font-medium cursor-pointer"
          >
            <option value="All">All Statuses</option>
            <option value="InTransit">InTransit</option>
            <option value="CustomsClearance">CustomsClearance</option>
            <option value="Delivered">Delivered</option>
          </select>
        </div>
      </div>

      <!-- Table View -->
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="border-b border-white/5 bg-slate-900/50 text-slate-400 text-xs uppercase tracking-wider">
              <th class="py-4 px-6 font-semibold">Tracking Number</th>
              <th class="py-4 px-6 font-semibold">Route</th>
              <th class="py-4 px-6 font-semibold">Sender / Receiver</th>
              <th class="py-4 px-6 font-semibold">Weight (kg)</th>
              <th class="py-4 px-6 font-semibold">Status</th>
              <th class="py-4 px-6 font-semibold">AI Risk Diagnosis</th>
              <th class="py-4 px-6 font-semibold text-right">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-white/5">
            <tr v-if="filteredShipments.length === 0" class="text-center text-slate-500">
              <td colspan="7" class="py-12 text-sm font-medium">No shipments found. Try adjusting your filters.</td>
            </tr>
            <tr 
              v-for="shipment in filteredShipments" 
              :key="shipment.id"
              class="hover:bg-white/[0.02] transition group"
            >
              <!-- Tracking -->
              <td class="py-4 px-6">
                <span class="font-mono text-sm font-semibold text-white tracking-wider flex items-center gap-2">
                  <Package class="w-4 h-4 text-indigo-400" />
                  {{ shipment.trackingNo }}
                </span>
              </td>
              <!-- Route -->
              <td class="py-4 px-6">
                <div class="text-sm font-medium text-slate-200 flex items-center gap-1.5">
                  <Navigation class="w-3.5 h-3.5 text-slate-500" />
                  {{ shipment.route?.source.split('(')[0] || 'Local' }}
                  <span class="text-slate-500">➔</span>
                  {{ shipment.route?.destination.split('(')[0] || 'Destination' }}
                </div>
                <div class="text-xs text-slate-500 mt-0.5">Duration: {{ shipment.route?.averageDuration || 0 }} hrs</div>
              </td>
              <!-- Sender / Receiver -->
              <td class="py-4 px-6">
                <div class="text-sm text-slate-200 font-semibold">{{ shipment.sender }}</div>
                <div class="text-xs text-slate-400 mt-0.5 flex items-center gap-1">
                  <User class="w-3 h-3 text-slate-500" />
                  {{ shipment.receiver }}
                </div>
              </td>
              <!-- Weight -->
              <td class="py-4 px-6 text-sm text-slate-300 font-mono">
                {{ shipment.weight.toLocaleString('en-US') }} kg
              </td>
              <!-- Status -->
              <td class="py-4 px-6">
                <span 
                  class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold uppercase tracking-wider"
                  :class="{
                    'bg-amber-500/10 text-amber-400 border border-amber-500/20': shipment.status === 'InTransit',
                    'bg-purple-500/10 text-purple-400 border border-purple-500/20': shipment.status === 'CustomsClearance',
                    'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20': shipment.status === 'Delivered'
                  }"
                >
                  {{ shipment.status }}
                </span>
              </td>
              <!-- AI Risk -->
              <td class="py-4 px-6">
                <div class="flex items-center gap-2">
                  <span 
                    class="h-2.5 w-2.5 rounded-full"
                    :class="{
                      'bg-emerald-400 shadow-emerald-400/50 shadow-[0_0_8px]': shipment.riskLevel === 'Low',
                      'bg-amber-400 shadow-amber-400/50 shadow-[0_0_8px]': shipment.riskLevel === 'Medium',
                      'bg-rose-500 shadow-rose-500/50 shadow-[0_0_8px]': shipment.riskLevel === 'High',
                    }"
                  ></span>
                  <span 
                    class="text-xs font-bold uppercase tracking-wider"
                    :class="{
                      'text-emerald-400': shipment.riskLevel === 'Low',
                      'text-amber-400': shipment.riskLevel === 'Medium',
                      'text-rose-400': shipment.riskLevel === 'High',
                    }"
                  >
                    {{ shipment.riskLevel }} ({{ Math.round((shipment.riskScore || 0.15) * 100) }}%)
                  </span>
                </div>
              </td>
              <!-- Actions -->
              <td class="py-4 px-6 text-right">
                <div class="flex items-center justify-end gap-2.5">
                  <button 
                    @click="navigateToPredict(shipment)" 
                    class="bg-indigo-500/10 hover:bg-indigo-500/20 text-indigo-400 hover:text-white text-xs font-bold py-1.5 px-3 rounded-lg flex items-center gap-1 transition duration-200 cursor-pointer"
                  >
                    Diagnose AI <ArrowUpRight class="w-3 h-3" />
                  </button>
                  <button 
                    @click="handleDelete(shipment.id)" 
                    class="text-slate-500 hover:text-red-400 p-1.5 rounded-lg hover:bg-red-500/5 transition cursor-pointer"
                  >
                    <Trash2 class="w-4 h-4" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Add Shipment Modal -->
    <div v-if="isAddModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <!-- Backdrop -->
      <div @click="isAddModalOpen = false" class="absolute inset-0 bg-black/60 backdrop-blur-sm"></div>
      
      <!-- Modal Content -->
      <div class="relative w-full max-w-lg glass-panel rounded-2xl shadow-2xl p-6 glow-indigo z-10 animate-in fade-in zoom-in-95 duration-200">
        <div class="flex items-center justify-between border-b border-white/5 pb-4 mb-4">
          <h2 class="text-xl font-bold text-white flex items-center gap-2">
            <Package class="w-5 h-5 text-indigo-400" /> Dispatch New Shipment
          </h2>
          <button @click="isAddModalOpen = false" class="text-slate-400 hover:text-white cursor-pointer">
            <X class="w-5 h-5" />
          </button>
        </div>

        <form @submit.prevent="handleAddShipment" class="space-y-4">
          <!-- Tracking No (ReadOnly Auto-generated) -->
          <div>
            <label class="block text-xs font-bold uppercase tracking-wider text-slate-400 mb-1.5">Tracking Number</label>
            <input 
              v-model="newShipment.trackingNo" 
              type="text" 
              readonly 
              class="glass-input w-full px-3 py-2 text-sm font-mono bg-slate-900/60 text-slate-300"
            />
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <!-- Sender -->
            <div>
              <label class="block text-xs font-bold uppercase tracking-wider text-slate-400 mb-1.5">Sender Name</label>
              <input 
                v-model="newShipment.sender" 
                type="text" 
                required 
                placeholder="e.g. Foxconn Factory"
                class="glass-input w-full px-3 py-2 text-sm"
              />
            </div>
            <!-- Receiver -->
            <div>
              <label class="block text-xs font-bold uppercase tracking-wider text-slate-400 mb-1.5">Receiver Name</label>
              <input 
                v-model="newShipment.receiver" 
                type="text" 
                required 
                placeholder="e.g. Apple Warehouse"
                class="glass-input w-full px-3 py-2 text-sm"
              />
            </div>
          </div>

          <!-- Route Selection -->
          <div>
            <label class="block text-xs font-bold uppercase tracking-wider text-slate-400 mb-1.5">Logistics Route</label>
            <select 
              v-model="newShipment.routeId" 
              class="glass-input w-full px-3 py-2 text-sm cursor-pointer"
            >
              <option v-for="route in routes" :key="route.id" :value="route.id">
                {{ route.name }}
              </option>
            </select>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <!-- Weight -->
            <div>
              <label class="block text-xs font-bold uppercase tracking-wider text-slate-400 mb-1.5">Weight (kg)</label>
              <input 
                v-model.number="newShipment.weight" 
                type="number" 
                step="0.01" 
                required
                class="glass-input w-full px-3 py-2 text-sm font-mono"
              />
            </div>
            <!-- Status -->
            <div>
              <label class="block text-xs font-bold uppercase tracking-wider text-slate-400 mb-1.5">Initial Status</label>
              <select 
                v-model="newShipment.status" 
                class="glass-input w-full px-3 py-2 text-sm cursor-pointer"
              >
                <option value="InTransit">InTransit</option>
                <option value="CustomsClearance">CustomsClearance</option>
              </select>
            </div>
          </div>

          <!-- Buttons -->
          <div class="flex justify-end gap-3 border-top border-white/5 pt-4 mt-6">
            <button 
              type="button" 
              @click="isAddModalOpen = false" 
              class="glass-card px-4 py-2 text-sm font-semibold text-slate-400 hover:text-white cursor-pointer"
            >
              Cancel
            </button>
            <button 
              type="submit" 
              class="bg-indigo-600 hover:bg-indigo-500 text-white font-semibold px-5 py-2 text-sm rounded-lg shadow-lg hover:shadow-indigo-500/20 transition cursor-pointer"
            >
              Confirm & Dispatch
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>
