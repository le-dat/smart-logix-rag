<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Plus, RefreshCw } from '@lucide/vue'
import { useLogisticsStore } from '../stores/logistics'
import type { Shipment, CreateShipmentInput } from '../types'

// Modular Components
import KpiRow from '../components/dashboard/KpiRow.vue'
import ShipmentTable from '../components/dashboard/ShipmentTable.vue'
import AddShipmentModal from '../components/dashboard/AddShipmentModal.vue'
import DelayTrendChart from '../components/dashboard/DelayTrendChart.vue'
import RiskFactorChart from '../components/dashboard/RiskFactorChart.vue'

const router = useRouter()
const store = useLogisticsStore()

// View Filter & Dialog State
const searchQuery = ref('')
const selectedStatus = ref('All')
const isAddModalOpen = ref(false)

onMounted(async () => {
  await store.fetchShipments()
  await store.fetchCustomers()
})

// Computed KPI Stats & UI Risk Enrichment
const totalShipments = computed(() => store.shipments.length)

const shipmentsWithRisk = computed(() => {
  return store.shipments.map(s => {
    let score = 0.15
    if (s.routeId === 4) score = 0.78 // PVG to LAX (longer duration, high risk)
    else if (s.routeId === 5) score = 0.42 // HAN to FRA (medium duration)
    else if (s.weight > 3000) score = 0.55 // Heavy weight raises risk slightly
    
    if (s.status === 'Delivered') score = 0.05
    
    let level = 'Low'
    if (score > 0.7) level = 'High'
    else if (score > 0.3) level = 'Medium'

    return {
      ...s,
      riskScore: s.riskScore ?? score,
      riskLevel: s.riskLevel ?? level
    } as Shipment
  })
})

const avgRiskIndex = computed(() => {
  if (shipmentsWithRisk.value.length === 0) return 0
  const total = shipmentsWithRisk.value.reduce((acc, curr) => acc + (curr.riskScore ?? 0.15), 0)
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

// Action Event Handlers
const handleRefresh = async () => {
  await store.fetchShipments()
}

const handleAddShipment = async (payload: CreateShipmentInput) => {
  try {
    await store.addShipment(payload)
    isAddModalOpen.value = false
  } catch (err: any) {
    alert('Failed to add shipment: ' + err.message)
  }
}

const handleDelete = async (id: number) => {
  if (confirm('Are you sure you want to delete this shipment?')) {
    try {
      await store.deleteShipment(id)
    } catch (err: any) {
      alert('Failed to delete shipment: ' + err.message)
    }
  }
}

const navigateToPredict = (shipment: Shipment) => {
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
  <div class="space-y-6 text-text-primary">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 border-b border-brand-border pb-5 select-none">
      <div>
        <h1 class="text-2xl font-brand font-black tracking-tight text-text-primary">
          Logistics Control Center
        </h1>
        <p class="text-text-secondary text-xs mt-1">Real-time container logs, tracking, and predictive AI dispatch.</p>
      </div>
      <div class="flex items-center gap-3">
        <button 
          @click="handleRefresh" 
          class="glass-card p-2 rounded-xl text-text-secondary hover:text-brand-accent flex items-center justify-center cursor-pointer border border-brand-border bg-card-bg shadow-sm"
          :class="{ 'animate-spin': store.loading }"
          aria-label="Refresh shipments data"
          title="Refresh"
        >
          <RefreshCw class="w-4.5 h-4.5" />
        </button>
        <button 
          @click="isAddModalOpen = true" 
          class="btn-capsule-primary flex items-center gap-2 py-2 px-4 shadow-sm text-xs cursor-pointer select-none font-bold"
        >
          <Plus class="w-4.5 h-4.5 shrink-0" /> Add Shipment
        </button>
      </div>
    </div>

    <!-- KPIs Row -->
    <KpiRow 
      :total-shipments="totalShipments"
      :avg-risk-index="avgRiskIndex"
      :high-risk-count="highRiskCount"
    />

    <!-- Data Controls & Table -->
    <ShipmentTable 
      v-model:searchQuery="searchQuery"
      v-model:selectedStatus="selectedStatus"
      :shipments="filteredShipments"
      @diagnose="navigateToPredict"
      @delete="handleDelete"
    />

    <!-- Add Shipment Modal Container -->
    <AddShipmentModal 
      :is-open="isAddModalOpen"
      :customers="store.customers"
      :loading="store.loading"
      @close="isAddModalOpen = false"
      @submit="handleAddShipment"
    />

    <!-- ── Interactive Analytics Section ───────────────────────────────── -->
    <div class="space-y-2 select-none">
      <div class="flex items-baseline gap-2">
        <h2 class="text-base font-brand font-black tracking-tight text-text-primary">Predictive Analytics</h2>
        <span class="text-[9px] font-black font-mono text-brand-accent bg-brand-panel border border-brand-border px-2.5 py-0.5 rounded-full select-none">
          Powered by XGBoost + ChromaDB
        </span>
      </div>
      <p class="text-xs text-text-secondary leading-relaxed font-medium">Live visualization of delay trends and machine-learning feature importance derived from your shipment data.</p>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <DelayTrendChart :shipments="store.shipments" />
      <RiskFactorChart :shipments="store.shipments" />
    </div>

  </div>
</template>
