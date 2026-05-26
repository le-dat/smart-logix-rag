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
  <BaseCard no-hover class="overflow-hidden shadow-sm !p-0">
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
          <tr v-if="shipments.length === 0" class="text-center text-slate-400">
            <td colspan="7" class="py-10 text-xs font-bold">No shipments found. Try adjusting your filters.</td>
          </tr>
          <tr 
            v-for="shipment in shipments" 
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
              <div class="text-[10px] text-slate-400 mt-0.5 font-mono">Duration: {{ shipment.route?.averageDuration || 0 }} hrs</div>
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
                  class="text-[10px] font-bold uppercase tracking-wider font-mono"
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
                  @click="emit('diagnose', shipment)" 
                  class="btn-capsule-secondary py-1 px-2.5 text-[10px] font-bold flex items-center gap-0.5 cursor-pointer shadow-sm"
                >
                  Diagnose <ArrowUpRight class="w-3 h-3" />
                </button>
                <button 
                  @click="emit('delete', shipment.id)" 
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
  </BaseCard>
</template>
