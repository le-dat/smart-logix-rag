<script setup lang="ts">
import BaseCard from '../ui/BaseCard.vue'

interface Props {
  title: string
  value: string | number
  badgeText: string
  badgeClass?: string
}

withDefaults(defineProps<Props>(), {
  badgeClass: 'text-emerald-700 bg-emerald-100/50 border-emerald-200'
})
</script>

<template>
  <BaseCard>
    <div class="flex justify-between items-start">
      <div class="bg-black/5 p-2 rounded-lg text-slate-700">
        <slot name="icon" />
      </div>
      <span 
        class="text-[10px] font-bold py-0.5 px-2 rounded-full border"
        :class="badgeClass"
      >
        {{ badgeText }}
      </span>
    </div>
    <div class="mt-4">
      <h3 class="text-[#4a4943] text-[10px] font-bold uppercase tracking-wider">
        {{ title }}
      </h3>
      <p class="text-2xl font-extrabold mt-1 text-[#1c1b17]">
        {{ value }}
      </p>
    </div>
  </BaseCard>
</template>
