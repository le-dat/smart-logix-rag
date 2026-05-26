<script setup lang="ts">
interface Props {
  tag?: string
  noHover?: boolean
}

withDefaults(defineProps<Props>(), {
  tag: 'div',
  noHover: false
})
</script>

<template>
  <component 
    :is="tag"
    class="glass-card rounded-2xl p-5"
    :class="{ 'hover:border-[#e4e2d8] hover:translate-y-0 hover:shadow-sm': noHover }"
  >
    <slot />
  </component>
</template>
