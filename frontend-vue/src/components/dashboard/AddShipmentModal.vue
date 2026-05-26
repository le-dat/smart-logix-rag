<script setup lang="ts">
import { ref, watch } from 'vue'
import { Package } from '@lucide/vue'
import type { Customer, CreateShipmentInput } from '../../types'
import { ROUTES } from '../../constants/routes'
import BaseModal from '../ui/BaseModal.vue'
import BaseInput from '../ui/BaseInput.vue'
import BaseSelect from '../ui/BaseSelect.vue'
import BaseButton from '../ui/BaseButton.vue'

interface Props {
  isOpen: boolean
  customers: Customer[]
  loading?: boolean
}

const props = defineProps<Props>()
const emit = defineEmits<{
  close: []
  submit: [shipment: CreateShipmentInput]
}>()

// Internal Form State
const form = ref<CreateShipmentInput>({
  trackingNo: '',
  sender: '',
  receiver: '',
  routeId: 1,
  customerId: 1,
  weight: 150.0,
  status: 'InTransit'
})

const generateTrackingNo = () => {
  const rand = Math.floor(1000 + Math.random() * 9000)
  return `DMCO-VN-2026${rand}`
}

// Reset form and seed new tracking number whenever modal opens
watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    form.value = {
      trackingNo: generateTrackingNo(),
      sender: '',
      receiver: '',
      routeId: 1,
      customerId: props.customers.length > 0 ? props.customers[0].id : 1,
      weight: 150.0,
      status: 'InTransit'
    }
  }
})

const handleSubmit = () => {
  emit('submit', { ...form.value })
}
</script>

<template>
  <BaseModal 
    :is-open="isOpen" 
    title="Dispatch New Shipment"
    @close="emit('close')"
  >
    <template #header-icon>
      <Package class="w-5 h-5 text-slate-600" />
    </template>

    <form @submit.prevent="handleSubmit" class="space-y-4">
      <!-- Tracking No -->
      <BaseInput 
        v-model="form.trackingNo"
        label="Tracking Number"
        readonly
      />

      <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <!-- Sender -->
        <BaseInput 
          v-model="form.sender"
          label="Sender Name"
          required
          placeholder="e.g. Foxconn Factory"
        />
        
        <!-- Receiver -->
        <BaseInput 
          v-model="form.receiver"
          label="Receiver Name"
          required
          placeholder="e.g. Apple Warehouse"
        />
      </div>

      <!-- Route Selection -->
      <BaseSelect
        v-model.number="form.routeId"
        label="Logistics Route"
      >
        <option v-for="r in ROUTES" :key="r.id" :value="r.id">
          {{ r.name }}
        </option>
      </BaseSelect>

      <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <!-- Weight -->
        <BaseInput 
          v-model.number="form.weight"
          type="number"
          label="Weight (kg)"
          required
        />

        <!-- Status -->
        <BaseSelect
          v-model="form.status"
          label="Initial Status"
        >
          <option value="InTransit">InTransit</option>
          <option value="CustomsClearance">CustomsClearance</option>
        </BaseSelect>
      </div>

      <!-- Customer context dropdown (if needed) -->
      <BaseSelect
        v-model.number="form.customerId"
        label="Associated Customer Contact"
        v-if="customers.length > 0"
      >
        <option v-for="c in customers" :key="c.id" :value="c.id">
          {{ c.name }} ({{ c.email }})
        </option>
      </BaseSelect>

      <!-- Buttons -->
      <div class="flex justify-end gap-3 border-t border-[#e4e2d8] pt-4 mt-6">
        <BaseButton 
          variant="secondary"
          @click="emit('close')"
        >
          Cancel
        </BaseButton>
        <BaseButton 
          type="submit"
          :loading="loading"
        >
          Confirm & Dispatch
        </BaseButton>
      </div>
    </form>
  </BaseModal>
</template>
