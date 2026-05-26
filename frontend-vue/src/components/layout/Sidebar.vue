<script setup lang="ts">
import { 
  LayoutDashboard, 
  Compass, 
  MessageSquare, 
  Cpu
} from '@lucide/vue'
</script>

<template>
  <aside class="w-64 shrink-0 glass-panel border-r border-[#e4e2d8] flex flex-col justify-between hidden md:flex z-20">
    <div class="p-6 space-y-6">
      <!-- Logo -->
      <div class="flex items-center gap-3">
        <div class="h-8 w-8 rounded-lg bg-[#1c1b17] flex items-center justify-center shadow-sm">
          <Cpu class="w-5 h-5 text-white" />
        </div>
        <span class="font-extrabold tracking-widest text-base text-[#1c1b17] uppercase">
          SmartLogix
        </span>
      </div>

      <!-- Navigation Links -->
      <nav class="space-y-1.5 pt-2">
        <router-link 
          to="/" 
          v-slot="{ isActive }" 
          class="block"
        >
          <span 
            class="flex items-center gap-3 px-4 py-2.5 rounded-lg text-xs font-bold uppercase tracking-wider transition-all duration-200"
            :class="isActive 
              ? 'bg-white text-[#1c1b17] border border-[#e4e2d8] shadow-sm' 
              : 'text-[#4a4943] hover:text-[#1c1b17] border border-transparent hover:bg-black/[0.02]'"
          >
            <LayoutDashboard class="w-4 h-4" />
            Dashboard
          </span>
        </router-link>

        <router-link 
          to="/predict" 
          v-slot="{ isActive }" 
          class="block"
        >
          <span 
            class="flex items-center gap-3 px-4 py-2.5 rounded-lg text-xs font-bold uppercase tracking-wider transition-all duration-200"
            :class="isActive 
              ? 'bg-white text-[#1c1b17] border border-[#e4e2d8] shadow-sm' 
              : 'text-[#4a4943] hover:text-[#1c1b17] border border-transparent hover:bg-black/[0.02]'"
          >
            <Compass class="w-4 h-4" />
            Risk Predictor
          </span>
        </router-link>

        <router-link 
          to="/chat" 
          v-slot="{ isActive }" 
          class="block"
        >
          <span 
            class="flex items-center gap-3 px-4 py-2.5 rounded-lg text-xs font-bold uppercase tracking-wider transition-all duration-200"
            :class="isActive 
              ? 'bg-white text-[#1c1b17] border border-[#e4e2d8] shadow-sm' 
              : 'text-[#4a4943] hover:text-[#1c1b17] border border-transparent hover:bg-black/[0.02]'"
          >
            <MessageSquare class="w-4 h-4" />
            AI Copilot
          </span>
        </router-link>
      </nav>
    </div>
  </aside>
</template>
