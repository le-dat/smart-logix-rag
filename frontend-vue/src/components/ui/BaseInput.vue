<script setup lang="ts">
import { useId } from 'vue'

interface Props {
  label?: string
  type?: string
  placeholder?: string
  required?: boolean
  disabled?: boolean
  readonly?: boolean
}

withDefaults(defineProps<Props>(), {
  type: 'text',
  placeholder: '',
  required: false,
  disabled: false,
  readonly: false
})

const modelValue = defineModel<string | number>()
const inputId = useId()
</script>

<template>
  <div class="space-y-1.5 w-full">
    <label 
      v-if="label" 
      :for="inputId"
      class="block text-[10px] font-bold uppercase tracking-wider text-slate-500"
    >
      {{ label }}
    </label>
    <input
      :id="inputId"
      v-model="modelValue"
      :type="type"
      :placeholder="placeholder"
      :required="required"
      :disabled="disabled"
      :readonly="readonly"
      class="glass-input px-3.5 py-2 w-full text-xs placeholder-slate-400 focus:outline-none"
      :class="{ 'bg-slate-50 text-slate-600 cursor-not-allowed': disabled || readonly }"
    />
  </div>
</template>
