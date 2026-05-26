<script setup lang="ts">
import { RefreshCw } from '@lucide/vue'

interface Props {
  variant?: 'primary' | 'secondary'
  loading?: boolean
  disabled?: boolean
  type?: 'button' | 'submit' | 'reset'
}

withDefaults(defineProps<Props>(), {
  variant: 'primary',
  loading: false,
  disabled: false,
  type: 'button'
})
</script>

<template>
  <button
    :type="type"
    :disabled="disabled || loading"
    class="flex items-center justify-center gap-2 py-2 px-4 text-xs font-semibold rounded-full border transition duration-200 cursor-pointer disabled:opacity-40 disabled:cursor-not-allowed"
    :class="variant === 'primary' 
      ? 'bg-[#1c1b17] hover:bg-[#36342e] text-white border-[#1c1b17] hover:border-[#36342e] hover:-translate-y-[1px] hover:shadow-sm'
      : 'bg-white hover:bg-[#fdfdfc] text-[#1c1b17] border-[#e4e2d8] hover:border-[#1c1b17] hover:-translate-y-[1px]'"
  >
    <RefreshCw v-if="loading" class="w-3.5 h-3.5 animate-spin" />
    <slot v-else name="icon" />
    <slot />
  </button>
</template>
