<script setup lang="ts">
import { 
  Package, 
  TrendingUp, 
  AlertTriangle, 
  Activity 
} from '@lucide/vue'
import KpiCard from './KpiCard.vue'

interface Props {
  totalShipments: number
  avgRiskIndex: number
  highRiskCount: number
}

defineProps<Props>()
</script>

<template>
  <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
    <!-- KPI 1 -->
    <KpiCard 
      title="Total Shipments" 
      :value="totalShipments" 
      badge-text="Active"
    >
      <template #icon>
        <Package class="w-5 h-5" />
      </template>
    </KpiCard>

    <!-- KPI 2 -->
    <KpiCard 
      title="Average Risk Score" 
      :value="`${avgRiskIndex}%`" 
      badge-text="Healthy"
    >
      <template #icon>
        <TrendingUp class="w-5 h-5" />
      </template>
    </KpiCard>

    <!-- KPI 3 -->
    <KpiCard 
      title="High Risk Warnings" 
      :value="highRiskCount" 
      badge-text="Alert"
      badge-class="text-amber-700 bg-amber-100/50 border-amber-200"
    >
      <template #icon>
        <AlertTriangle class="w-5 h-5" />
      </template>
    </KpiCard>

    <!-- KPI 4 -->
    <KpiCard 
      title="Active Services" 
      value="4 / 4" 
      badge-text="RAG Online"
      badge-class="text-purple-700 bg-purple-100/50 border-purple-200"
    >
      <template #icon>
        <Activity class="w-5 h-5" />
      </template>
    </KpiCard>
  </div>
</template>
