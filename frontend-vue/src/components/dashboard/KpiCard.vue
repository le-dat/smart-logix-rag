<script setup lang="ts">
import BaseCard from '../ui/BaseCard.vue'

interface Props {
  title: string
  value: string | number
  badgeText: string
  badgeClass?: string
}

withDefaults(defineProps<Props>(), {
  badgeClass: 'text-brand-accent bg-brand-accent-glow border-brand-accent/20'
})
</script>

<template>
  <BaseCard>
    <div class="flex justify-between items-start">
      <div class="bg-brand-panel p-2.5 rounded-xl text-text-secondary border border-brand-border/40">
        <slot name="icon" />
      </div>
      <span 
        class="text-sm font-black uppercase tracking-wider py-0.5 px-2 rounded-full border border-brand-border/60"
        :class="badgeClass"
      >
        {{ badgeText }}
      </span>
    </div>
    <div class="mt-4">
      <h3 class="text-text-secondary text-sm font-black uppercase tracking-widest font-mono">
        {{ title }}
      </h3>
      <p class="text-2xl font-black mt-1 text-text-primary">
        {{ value }}
      </p>
    </div>
  </BaseCard>
</template>
