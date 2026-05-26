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
const shipmentsWithRisk = computed(() => {
  return store.shipments.map(s => {
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
  const rand = Math.floor(1000 + Math.random() * 9000)
  newShipment.value.trackingNo = `DMCO-VN-2026${rand}`
  isAddModalOpen.value = true
}

const handleAddShipment = async () => {
  try {
    await store.addShipment(newShipment.value)
    isAddModalOpen.value = false
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
    <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 border-b border-[#e4e2d8] pb-5">
      <div>
        <h1 class="text-2xl font-extrabold tracking-tight text-[#1c1b17]">
          Logistics Control Center
        </h1>
        <p class="text-[#4a4943] text-xs mt-1">Real-time container logs, tracking, and predictive AI dispatch.</p>
      </div>
      <div class="flex items-center gap-3">
        <button 
          @click="handleRefresh" 
          class="glass-card p-2 rounded-lg text-slate-500 hover:text-black flex items-center justify-center cursor-pointer"
          :class="{ 'animate-spin': store.loading }"
        >
          <RefreshCw class="w-4.5 h-4.5" />
        </button>
        <button 
          @click="openAddModal" 
          class="btn-capsule-primary flex items-center gap-2 py-2 px-4 shadow-sm text-xs cursor-pointer"
        >
          <Plus class="w-4.5 h-4.5" /> Add Shipment
        </button>
      </div>
    </div>

    <!-- KPIs Row -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
      <!-- KPI 1 -->
      <div class="glass-card p-5 rounded-xl relative overflow-hidden">
        <div class="flex justify-between items-start">
          <div class="bg-black/5 p-2 rounded-lg text-slate-700">
            <Package class="w-5 h-5" />
          </div>
          <span class="text-[10px] font-bold text-emerald-700 bg-emerald-100/50 py-0.5 px-2 rounded-full border border-emerald-200">
            Active
          </span>
        </div>
        <div class="mt-4">
          <h3 class="text-[#4a4943] text-[10px] font-bold uppercase tracking-wider">Total Shipments</h3>
          <p class="text-2xl font-extrabold mt-1 text-[#1c1b17]">{{ totalShipments }}</p>
        </div>
      </div>

      <!-- KPI 2 -->
      <div class="glass-card p-5 rounded-xl relative overflow-hidden">
        <div class="flex justify-between items-start">
          <div class="bg-black/5 p-2 rounded-lg text-slate-700">
            <TrendingUp class="w-5 h-5" />
          </div>
          <span class="text-[10px] font-bold text-emerald-700 bg-emerald-100/50 py-0.5 px-2 rounded-full border border-emerald-200">
            Healthy
          </span>
        </div>
        <div class="mt-4">
          <h3 class="text-[#4a4943] text-[10px] font-bold uppercase tracking-wider">Average Risk Score</h3>
          <p class="text-2xl font-extrabold mt-1 text-[#1c1b17]">{{ avgRiskIndex }}%</p>
        </div>
      </div>

      <!-- KPI 3 -->
      <div class="glass-card p-5 rounded-xl relative overflow-hidden">
        <div class="flex justify-between items-start">
          <div class="bg-black/5 p-2 rounded-lg text-slate-700">
            <AlertTriangle class="w-5 h-5" />
          </div>
          <span class="text-[10px] font-bold text-amber-700 bg-amber-100/50 py-0.5 px-2 rounded-full border border-amber-200">
            Alert
          </span>
        </div>
        <div class="mt-4">
          <h3 class="text-[#4a4943] text-[10px] font-bold uppercase tracking-wider">High Risk Warnings</h3>
          <p class="text-2xl font-extrabold mt-1 text-[#1c1b17]">{{ highRiskCount }}</p>
        </div>
      </div>

      <!-- KPI 4 -->
      <div class="glass-card p-5 rounded-xl relative overflow-hidden">
        <div class="flex justify-between items-start">
          <div class="bg-black/5 p-2 rounded-lg text-slate-700">
            <Activity class="w-5 h-5" />
          </div>
          <span class="text-[10px] font-bold text-purple-700 bg-purple-100/50 py-0.5 px-2 rounded-full border border-purple-200">
            RAG Online
          </span>
        </div>
        <div class="mt-4">
          <h3 class="text-[#4a4943] text-[10px] font-bold uppercase tracking-wider">Active Services</h3>
          <p class="text-2xl font-extrabold mt-1 text-[#1c1b17]">4 / 4</p>
        </div>
      </div>
    </div>

    <!-- Data Controls & Table -->
    <div class="glass-card rounded-xl overflow-hidden shadow-sm">
      <!-- Search & Filters -->
      <div class="p-4 border-b border-[#e4e2d8] bg-black/[0.01] flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div class="relative flex-1 max-w-md">
          <span class="absolute inset-y-0 left-0 pl-3 flex items-center text-slate-400">
            <Search class="w-4 h-4" />
          </span>
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="Search by tracking number, sender, or receiver..." 
            class="glass-input pl-9 pr-4 py-1.5 w-full text-xs"
          />
        </div>
        <div class="flex items-center gap-3">
          <div class="flex items-center gap-1.5 text-slate-500 text-xs">
            <Filter class="w-3.5 h-3.5" />
            <span>Status:</span>
          </div>
          <select 
            v-model="selectedStatus" 
            class="glass-input px-3 py-1.5 text-xs font-semibold cursor-pointer"
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
            <tr class="border-b border-[#e4e2d8] bg-[#f3f2eb] text-[#1c1b17] text-[10px] uppercase tracking-wider font-bold">
              <th class="py-3 px-5 font-bold">Tracking Number</th>
              <th class="py-3 px-5 font-bold">Route</th>
              <th class="py-3 px-5 font-bold">Sender / Receiver</th>
              <th class="py-3 px-5 font-bold">Weight (kg)</th>
              <th class="py-3 px-5 font-bold">Status</th>
              <th class="py-3 px-5 font-bold">AI Risk Diagnosis</th>
              <th class="py-3 px-5 font-bold text-right">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-[#e4e2d8]">
            <tr v-if="filteredShipments.length === 0" class="text-center text-slate-400">
              <td colspan="7" class="py-10 text-xs font-bold">No shipments found. Try adjusting your filters.</td>
            </tr>
            <tr 
              v-for="shipment in filteredShipments" 
              :key="shipment.id"
              class="hover:bg-black/[0.01] transition group bg-white text-[#1c1b17]"
            >
              <!-- Tracking -->
              <td class="py-3 px-5">
                <span class="font-mono text-xs font-bold text-[#1c1b17] tracking-wider flex items-center gap-2">
                  <Package class="w-4 h-4 text-slate-500" />
                  {{ shipment.trackingNo }}
                </span>
              </td>
              <!-- Route -->
              <td class="py-3 px-5">
                <div class="text-xs font-bold text-[#1c1b17] flex items-center gap-1.5">
                  <Navigation class="w-3.5 h-3.5 text-slate-400" />
                  {{ shipment.route?.source.split('(')[0] || 'Local' }}
                  <span class="text-slate-400">➔</span>
                  {{ shipment.route?.destination.split('(')[0] || 'Destination' }}
                </div>
                <div class="text-[10px] text-slate-400 mt-0.5">Duration: {{ shipment.route?.averageDuration || 0 }} hrs</div>
              </td>
              <!-- Sender / Receiver -->
              <td class="py-3 px-5">
                <div class="text-xs text-[#1c1b17] font-bold">{{ shipment.sender }}</div>
                <div class="text-[10px] text-slate-400 mt-0.5 flex items-center gap-1">
                  <User class="w-3 h-3 text-slate-400" />
                  {{ shipment.receiver }}
                </div>
              </td>
              <!-- Weight -->
              <td class="py-3 px-5 text-xs text-slate-600 font-mono">
                {{ shipment.weight.toLocaleString('en-US') }} kg
              </td>
              <!-- Status -->
              <td class="py-3 px-5">
                <span 
                  class="inline-flex items-center px-2 py-0.5 rounded text-[10px] font-bold uppercase tracking-wider"
                  :class="{
                    'bg-amber-100 text-amber-800 border border-amber-200/50': shipment.status === 'InTransit',
                    'bg-purple-100 text-purple-800 border border-purple-200/50': shipment.status === 'CustomsClearance',
                    'bg-emerald-100 text-emerald-800 border border-emerald-200/50': shipment.status === 'Delivered'
                  }"
                >
                  {{ shipment.status }}
                </span>
              </td>
              <!-- AI Risk -->
              <td class="py-3 px-5">
                <div class="flex items-center gap-1.5">
                  <span 
                    class="h-2 w-2 rounded-full"
                    :class="{
                      'bg-emerald-500': shipment.riskLevel === 'Low',
                      'bg-amber-500': shipment.riskLevel === 'Medium',
                      'bg-rose-500': shipment.riskLevel === 'High',
                    }"
                  ></span>
                  <span 
                    class="text-[10px] font-bold uppercase tracking-wider"
                    :class="{
                      'text-emerald-700': shipment.riskLevel === 'Low',
                      'text-amber-700': shipment.riskLevel === 'Medium',
                      'text-rose-700': shipment.riskLevel === 'High',
                    }"
                  >
                    {{ shipment.riskLevel }} ({{ Math.round((shipment.riskScore || 0.15) * 100) }}%)
                  </span>
                </div>
              </td>
              <!-- Actions -->
              <td class="py-3 px-5 text-right">
                <div class="flex items-center justify-end gap-2">
                  <button 
                    @click="navigateToPredict(shipment)" 
                    class="btn-capsule-secondary py-1 px-2.5 text-[10px] font-bold flex items-center gap-0.5 cursor-pointer shadow-sm"
                  >
                    Diagnose <ArrowUpRight class="w-3 h-3" />
                  </button>
                  <button 
                    @click="handleDelete(shipment.id)" 
                    class="text-slate-400 hover:text-red-600 p-1 rounded hover:bg-red-50 transition cursor-pointer"
                  >
                    <Trash2 class="w-3.5 h-3.5" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Add Shipment Modal (Perplexity style light) -->
    <div v-if="isAddModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <!-- Backdrop -->
      <div @click="isAddModalOpen = false" class="absolute inset-0 bg-black/30 backdrop-blur-xs"></div>
      
      <!-- Modal Content -->
      <div class="relative w-full max-w-lg bg-white border border-[#e4e2d8] rounded-2xl shadow-xl p-6 z-10 animate-in fade-in zoom-in-95 duration-200 text-[#1c1b17]">
        <div class="flex items-center justify-between border-b border-[#e4e2d8] pb-4 mb-4">
          <h2 class="text-lg font-bold text-[#1c1b17] flex items-center gap-2">
            <Package class="w-5 h-5 text-slate-600" /> Dispatch New Shipment
          </h2>
          <button @click="isAddModalOpen = false" class="text-slate-400 hover:text-black cursor-pointer">
            <X class="w-5 h-5" />
          </button>
        </div>

        <form @submit.prevent="handleAddShipment" class="space-y-4">
          <!-- Tracking No -->
          <div>
            <label class="block text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1.5">Tracking Number</label>
            <input 
              v-model="newShipment.trackingNo" 
              type="text" 
              readonly 
              class="glass-input w-full px-3 py-2 text-xs font-mono bg-slate-50 text-slate-600"
            />
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <!-- Sender -->
            <div>
              <label class="block text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1.5">Sender Name</label>
              <input 
                v-model="newShipment.sender" 
                type="text" 
                required 
                placeholder="e.g. Foxconn Factory"
                class="glass-input w-full px-3 py-2 text-xs"
              />
            </div>
            <!-- Receiver -->
            <div>
              <label class="block text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1.5">Receiver Name</label>
              <input 
                v-model="newShipment.receiver" 
                type="text" 
                required 
                placeholder="e.g. Apple Warehouse"
                class="glass-input w-full px-3 py-2 text-xs"
              />
            </div>
          </div>

          <!-- Route Selection -->
          <div>
            <label class="block text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1.5">Logistics Route</label>
            <select 
              v-model="newShipment.routeId" 
              class="glass-input w-full px-3 py-2 text-xs cursor-pointer"
            >
              <option v-for="route in routes" :key="route.id" :value="route.id">
                {{ route.name }}
              </option>
            </select>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <!-- Weight -->
            <div>
              <label class="block text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1.5">Weight (kg)</label>
              <input 
                v-model.number="newShipment.weight" 
                type="number" 
                step="0.01" 
                required
                class="glass-input w-full px-3 py-2 text-xs font-mono"
              />
            </div>
            <!-- Status -->
            <div>
              <label class="block text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1.5">Initial Status</label>
              <select 
                v-model="newShipment.status" 
                class="glass-input w-full px-3 py-2 text-xs cursor-pointer"
              >
                <option value="InTransit">InTransit</option>
                <option value="CustomsClearance">CustomsClearance</option>
              </select>
            </div>
          </div>

          <!-- Buttons -->
          <div class="flex justify-end gap-3 border-t border-[#e4e2d8] pt-4 mt-6">
            <button 
              type="button" 
              @click="isAddModalOpen = false" 
              class="btn-capsule-secondary px-4 py-2 text-xs cursor-pointer"
            >
              Cancel
            </button>
            <button 
              type="submit" 
              class="btn-capsule-primary px-5 py-2 text-xs cursor-pointer"
            >
              Confirm & Dispatch
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>
