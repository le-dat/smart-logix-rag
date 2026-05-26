<script setup lang="ts">
import { 
  LayoutDashboard, 
  Compass, 
  MessageSquare, 
  Database, 
  Server, 
  Cpu
} from '@lucide/vue'
</script>

<template>
  <div class="min-h-screen flex text-slate-800 font-sans antialiased overflow-x-hidden bg-[#f9f8f4]">
    
    <!-- Sidebar Navigation -->
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

    <!-- Main Content Panel -->
    <div class="flex-1 flex flex-col min-w-0">
      
      <!-- Top Health Indicators bar -->
      <header class="h-14 shrink-0 glass-panel border-b border-[#e4e2d8] flex items-center justify-between px-6 z-10 bg-white/40">
        <!-- Breadcrumb / Section indicators -->
        <div class="flex items-center gap-2">
          <span class="text-[10px] font-bold uppercase tracking-widest text-[#4a4943] font-mono">SmartLogix Hub</span>
          <span class="text-slate-300">/</span>
          <span class="text-[10px] font-extrabold text-[#1c1b17] uppercase tracking-widest font-mono">{{ $route.name || 'Dashboard' }}</span>
        </div>

        <!-- Infrastructure Health Checks -->
        <div class="flex items-center gap-4 text-[9px] font-bold uppercase tracking-wider text-slate-500 hidden sm:flex font-mono">
          <div class="flex items-center gap-1.5">
            <Server class="w-3.5 h-3.5 text-slate-400" />
            <span>.NET Gateway</span>
            <span class="h-1.5 w-1.5 rounded-full bg-emerald-500"></span>
          </div>
          <div class="flex items-center gap-1.5">
            <Cpu class="w-3.5 h-3.5 text-slate-400" />
            <span>AI FastAPI</span>
            <span class="h-1.5 w-1.5 rounded-full bg-emerald-500"></span>
          </div>
          <div class="flex items-center gap-1.5">
            <Database class="w-3.5 h-3.5 text-slate-400" />
            <span>Chroma Vector</span>
            <span class="h-1.5 w-1.5 rounded-full bg-emerald-500"></span>
          </div>
        </div>
      </header>

      <!-- Main viewport routing with beautiful transitions -->
      <main class="flex-grow p-6 overflow-y-auto relative">
        <router-view v-slot="{ Component }">
          <transition name="fade" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </main>

    </div>
  </div>
</template>

<style>
/* Smooth View Transition animations */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s cubic-bezier(0.4, 0, 0.2, 1), transform 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

.fade-enter-from {
  opacity: 0;
  transform: translateY(6px);
}

.fade-leave-to {
  opacity: 0;
  transform: translateY(-6px);
}
</style>
