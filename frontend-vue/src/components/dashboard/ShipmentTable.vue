<script setup lang="ts">
import { 
  Package, 
  Navigation, 
  Search, 
  Filter, 
  ArrowUpRight, 
  Trash2, 
  User 
} from '@lucide/vue'
import type { Shipment } from '../../types'
import BaseCard from '../ui/BaseCard.vue'

interface Props {
  shipments: Shipment[]
}

defineProps<Props>()

const emit = defineEmits<{
  diagnose: [shipment: Shipment]
  delete: [id: number]
}>()

const searchQuery = defineModel<string>('searchQuery', { default: '' })
const selectedStatus = defineModel<string>('selectedStatus', { default: 'All' })
</script>

<template>
  <BaseCard no-hover class="overflow-hidden shadow-sm !p-0 bg-card-bg border border-brand-border">
    <!-- Search & Filters -->
    <div class="p-4 border-b border-brand-border bg-brand-panel/30 flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div class="relative flex-1 max-w-md">
        <span class="absolute inset-y-0 left-0 pl-3 flex items-center text-text-secondary/70">
          <Search class="w-4 h-4" />
        </span>
        <input 
          v-model="searchQuery" 
          type="text" 
          placeholder="Search by tracking number, sender, or receiver..." 
          class="glass-input pl-9 pr-4 py-1.5 w-full text-sm"
        />
      </div>
      <div class="flex items-center gap-3">
        <div class="flex items-center gap-1.5 text-text-secondary text-sm font-semibold">
          <Filter class="w-3.5 h-3.5 text-brand-accent" />
          <span>Status:</span>
        </div>
        <select 
          v-model="selectedStatus" 
          class="glass-input px-3 py-1.5 text-sm font-bold cursor-pointer select-none bg-card-bg text-text-primary"
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
          <tr class="border-b border-brand-border bg-brand-panel text-text-primary text-sm uppercase tracking-widest font-black font-mono">
            <th class="py-3 px-5">Tracking Number</th>
            <th class="py-3 px-5">Route</th>
            <th class="py-3 px-5">Sender / Receiver</th>
            <th class="py-3 px-5">Weight</th>
            <th class="py-3 px-5">Status</th>
            <th class="py-3 px-5">AI Risk Diagnosis</th>
            <th class="py-3 px-5 text-right">Actions</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-brand-border">
          <tr v-if="shipments.length === 0" class="text-center text-text-secondary">
            <td colspan="7" class="py-10 text-sm font-bold">No shipments found. Try adjusting your filters.</td>
          </tr>
          <tr 
            v-for="shipment in shipments" 
            :key="shipment.id"
            class="hover:bg-brand-panel/20 transition group bg-card-bg text-text-primary"
          >
            <!-- Tracking -->
            <td class="py-3 px-5">
              <span class="font-mono text-sm font-bold text-text-primary tracking-wider flex items-center gap-2">
                <Package class="w-4 h-4 text-text-secondary/70 shrink-0" />
                {{ shipment.trackingNo }}
              </span>
            </td>
            <!-- Route -->
            <td class="py-3 px-5">
              <div class="text-sm font-bold text-text-primary flex items-center gap-1.5">
                <Navigation class="w-3.5 h-3.5 text-text-secondary/60 shrink-0" />
                {{ shipment.route?.source.split('(')[0] || 'Local' }}
                <span class="text-brand-accent font-bold">➔</span>
                {{ shipment.route?.destination.split('(')[0] || 'Destination' }}
              </div>
              <div class="text-sm text-text-secondary mt-0.5 font-mono">Duration: {{ shipment.route?.averageDuration || 0 }} hrs</div>
            </td>
            <!-- Sender / Receiver -->
            <td class="py-3 px-5">
              <div class="text-sm text-text-primary font-bold">{{ shipment.sender }}</div>
              <div class="text-sm text-text-secondary mt-0.5 flex items-center gap-1">
                <User class="w-3 h-3 text-text-secondary/60 shrink-0" />
                {{ shipment.receiver }}
              </div>
            </td>
            <!-- Weight -->
            <td class="py-3 px-5 text-sm text-text-secondary font-mono">
              {{ shipment.weight.toLocaleString('en-US') }} kg
            </td>
            <!-- Status -->
            <td class="py-3 px-5">
              <span 
                class="inline-flex items-center px-2 py-0.5 rounded text-sm font-black uppercase tracking-wider border"
                :class="{
                  'bg-amber-500/10 text-amber-500 border-amber-500/25': shipment.status === 'InTransit',
                  'bg-purple-500/10 text-purple-500 border-purple-500/25': shipment.status === 'CustomsClearance',
                  'bg-emerald-500/10 text-emerald-500 border-emerald-500/25': shipment.status === 'Delivered'
                }"
              >
                {{ shipment.status }}
              </span>
            </td>
            <!-- AI Risk -->
            <td class="py-3 px-5">
              <div class="flex items-center gap-1.5">
                <span 
                  class="h-2 w-2 rounded-full shadow-sm"
                  :class="{
                    'bg-emerald-500 shadow-emerald-500/30': shipment.riskLevel === 'Low',
                    'bg-amber-500 shadow-amber-500/30': shipment.riskLevel === 'Medium',
                    'bg-rose-500 shadow-rose-500/30': shipment.riskLevel === 'High',
                  }"
                ></span>
                <span 
                  class="text-sm font-black uppercase tracking-wider font-mono"
                  :class="{
                    'text-emerald-500': shipment.riskLevel === 'Low',
                    'text-amber-500': shipment.riskLevel === 'Medium',
                    'text-rose-500': shipment.riskLevel === 'High',
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
                  @click="emit('diagnose', shipment)" 
                  class="btn-capsule-secondary py-1 px-2.5 text-sm font-bold flex items-center gap-0.5 cursor-pointer shadow-sm select-none"
                >
                  Diagnose <ArrowUpRight class="w-3 h-3" />
                </button>
                <button 
                  @click="emit('delete', shipment.id)" 
                  class="text-text-secondary hover:text-red-500 p-1.5 rounded-lg hover:bg-brand-panel transition-colors cursor-pointer"
                  aria-label="Delete shipment"
                >
                  <Trash2 class="w-3.5 h-3.5" />
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </BaseCard>
</template>
