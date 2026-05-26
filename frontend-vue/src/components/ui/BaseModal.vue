<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import { X } from '@lucide/vue'

interface Props {
  isOpen: boolean
  title?: string
}

const props = defineProps<Props>()
const emit = defineEmits<{
  close: []
}>()

// Keyboard event listener for escape key
const handleKeyDown = (e: KeyboardEvent) => {
  if (e.key === 'Escape' && props.isOpen) {
    emit('close')
  }
}

onMounted(() => {
  window.addEventListener('keydown', handleKeyDown)
})

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeyDown)
})
</script>

<template>
  <teleport to="body">
    <transition name="modal-fade">
      <div 
        v-if="isOpen" 
        class="fixed inset-0 z-50 flex items-center justify-center p-4"
        role="dialog"
        aria-modal="true"
      >
        <!-- Backdrop -->
        <div 
          class="absolute inset-0 bg-black/30 backdrop-blur-xs" 
          @click="emit('close')"
        ></div>
        
        <!-- Modal Wrapper Content -->
        <div class="relative w-full max-w-lg bg-white border border-[#e4e2d8] rounded-2xl shadow-xl p-6 z-10 text-[#1c1b17]">
          <!-- Header -->
          <div class="flex items-center justify-between border-b border-[#e4e2d8] pb-4 mb-4">
            <h2 class="text-lg font-bold text-[#1c1b17] flex items-center gap-2">
              <slot name="header-icon" />
              {{ title }}
            </h2>
            <button 
              type="button"
              @click="emit('close')" 
              class="text-slate-400 hover:text-black cursor-pointer p-1 rounded hover:bg-slate-50 transition"
              aria-label="Close modal"
            >
              <X class="w-5 h-5" />
            </button>
          </div>

          <!-- Body -->
          <div class="mt-2">
            <slot />
          </div>
        </div>
      </div>
    </transition>
  </teleport>
</template>

<style scoped>
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.25s ease;
}

.modal-fade-enter-active .relative,
.modal-fade-leave-active .relative {
  transition: transform 0.25s cubic-bezier(0.16, 1, 0.3, 1), opacity 0.25s ease;
}

.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}

.modal-fade-enter-from .relative {
  transform: scale(0.96) translateY(4px);
  opacity: 0;
}

.modal-fade-leave-to .relative {
  transform: scale(0.96) translateY(4px);
  opacity: 0;
}
</style>
