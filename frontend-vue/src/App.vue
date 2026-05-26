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
  <div class="min-h-screen flex text-slate-100 font-sans antialiased overflow-x-hidden">
    
    <!-- Sidebar Navigation -->
    <aside class="w-64 shrink-0 glass-panel border-r border-white/5 flex flex-col justify-between hidden md:flex z-20">
      <div class="p-6 space-y-7">
        <!-- Logo -->
        <div class="flex items-center gap-3">
          <div class="h-8 w-8 rounded-lg bg-gradient-to-tr from-indigo-500 to-purple-600 flex items-center justify-center shadow-lg shadow-indigo-500/20">
            <Cpu class="w-5 h-5 text-white" />
          </div>
          <span class="font-extrabold tracking-widest text-lg bg-gradient-to-r from-white via-indigo-200 to-indigo-400 bg-clip-text text-transparent uppercase">
            SmartLogix
          </span>
        </div>

        <!-- Navigation Links -->
        <nav class="space-y-1.5">
          <router-link 
            to="/" 
            v-slot="{ isActive }" 
            class="block"
          >
            <span 
              class="flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-bold uppercase tracking-wider transition-all duration-200"
              :class="isActive 
                ? 'bg-indigo-500/10 text-indigo-400 border border-indigo-500/25 shadow-lg shadow-indigo-500/5' 
                : 'text-slate-400 hover:text-white border border-transparent hover:bg-white/[0.02]'"
            >
              <LayoutDashboard class="w-4.5 h-4.5" />
              Dashboard
            </span>
          </router-link>

          <router-link 
            to="/predict" 
            v-slot="{ isActive }" 
            class="block"
          >
            <span 
              class="flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-bold uppercase tracking-wider transition-all duration-200"
              :class="isActive 
                ? 'bg-indigo-500/10 text-indigo-400 border border-indigo-500/25 shadow-lg shadow-indigo-500/5' 
                : 'text-slate-400 hover:text-white border border-transparent hover:bg-white/[0.02]'"
            >
              <Compass class="w-4.5 h-4.5" />
              Risk Predictor
            </span>
          </router-link>

          <router-link 
            to="/chat" 
            v-slot="{ isActive }" 
            class="block"
          >
            <span 
              class="flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-bold uppercase tracking-wider transition-all duration-200"
              :class="isActive 
                ? 'bg-indigo-500/10 text-indigo-400 border border-indigo-500/25 shadow-lg shadow-indigo-500/5' 
                : 'text-slate-400 hover:text-white border border-transparent hover:bg-white/[0.02]'"
            >
              <MessageSquare class="w-4.5 h-4.5" />
              AI Copilot
            </span>
          </router-link>
        </nav>
      </div>

      <!-- Dev Details Bottom -->
      <div class="p-6 border-t border-white/5 bg-slate-950/20">
        <div class="flex items-center gap-2">
          <div class="h-2 w-2 rounded-full bg-emerald-400 animate-pulse"></div>
          <span class="text-[10px] uppercase font-extrabold tracking-wider text-slate-500">Agentic Web Platform</span>
        </div>
        <p class="text-[11px] font-bold text-slate-400 mt-1 font-mono">v1.2.0 (Phase 3 Dev)</p>
      </div>
    </aside>

    <!-- Main Content Panel -->
    <div class="flex-1 flex flex-col min-w-0">
      
      <!-- Top Health Indicators bar -->
      <header class="h-16 shrink-0 glass-panel border-b border-white/5 flex items-center justify-between px-6 z-10">
        <!-- Breadcrumb / Section indicators -->
        <div class="flex items-center gap-2">
          <span class="text-xs font-bold uppercase tracking-widest text-slate-500 font-mono">SmartLogix Hub</span>
          <span class="text-slate-700">/</span>
          <span class="text-xs font-extrabold text-indigo-400 uppercase tracking-widest font-mono">{{ $route.name || 'Dashboard' }}</span>
        </div>

        <!-- Infrastructure Health Checks -->
        <div class="flex items-center gap-4 text-[10px] font-bold uppercase tracking-wider text-slate-400 hidden sm:flex">
          <div class="flex items-center gap-1.5">
            <Server class="w-3.5 h-3.5 text-indigo-400" />
            <span>.NET Gateway</span>
            <span class="h-1.5 w-1.5 rounded-full bg-emerald-400 shadow-[0_0_6px] shadow-emerald-400"></span>
          </div>
          <div class="flex items-center gap-1.5">
            <Cpu class="w-3.5 h-3.5 text-indigo-400" />
            <span>AI FastAPI</span>
            <span class="h-1.5 w-1.5 rounded-full bg-emerald-400 shadow-[0_0_6px] shadow-emerald-400"></span>
          </div>
          <div class="flex items-center gap-1.5">
            <Database class="w-3.5 h-3.5 text-indigo-400" />
            <span>Chroma Vector</span>
            <span class="h-1.5 w-1.5 rounded-full bg-emerald-400 shadow-[0_0_6px] shadow-emerald-400"></span>
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
  transform: translateY(8px);
}

.fade-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}
</style>
