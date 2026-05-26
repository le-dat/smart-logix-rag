<script setup lang="ts">
import { useId } from 'vue'

interface Option {
  value: string | number
  label: string
}

interface Props {
  label?: string
  options?: Option[]
  disabled?: boolean
}

withDefaults(defineProps<Props>(), {
  options: () => [],
  disabled: false
})

const modelValue = defineModel<string | number>()
const selectId = useId()
</script>

<template>
  <div class="space-y-1.5 w-full">
    <label 
      v-if="label" 
      :for="selectId"
      class="block text-sm font-bold uppercase tracking-wider text-slate-500"
    >
      {{ label }}
    </label>
    <select
      :id="selectId"
      v-model="modelValue"
      :disabled="disabled"
      class="glass-input px-3.5 py-2 w-full text-sm font-semibold cursor-pointer focus:outline-none"
      :class="{ 'bg-slate-50 text-slate-600 cursor-not-allowed': disabled }"
    >
      <slot>
        <option 
          v-for="opt in options" 
          :key="opt.value" 
          :value="opt.value"
        >
          {{ opt.label }}
        </option>
      </slot>
    </select>
  </div>
</template>
