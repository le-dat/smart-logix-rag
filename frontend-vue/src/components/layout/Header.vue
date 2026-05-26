<script setup lang="ts">
import { useRoute } from 'vue-router'
import { Server, Database, Cpu } from '@lucide/vue'

const route = useRoute()
</script>

<template>
  <header class="h-14 shrink-0 glass-panel border-b border-[#e4e2d8] flex items-center justify-between px-6 z-10 bg-white/40">
    <!-- Breadcrumb / Section indicators -->
    <div class="flex items-center gap-2">
      <span class="text-[10px] font-bold uppercase tracking-widest text-[#4a4943] font-mono">SmartLogix Hub</span>
      <span class="text-slate-300">/</span>
      <span class="text-[10px] font-extrabold text-[#1c1b17] uppercase tracking-widest font-mono">{{ route.name || 'Dashboard' }}</span>
    </div>

    <!-- Infrastructure Health Checks -->
    <div class="flex items-center gap-4 text-[9px] font-bold uppercase tracking-wider text-slate-500 hidden sm:flex font-mono">
      <div class="flex items-center gap-1.5">
        <Server class="w-3.5 h-3.5 text-slate-400" />
        <span>.NET Gateway</span>
        <span class="h-1.5 w-1.5 rounded-full bg-emerald-500 animate-pulse"></span>
      </div>
      <div class="flex items-center gap-1.5">
        <Cpu class="w-3.5 h-3.5 text-slate-400" />
        <span>AI FastAPI</span>
        <span class="h-1.5 w-1.5 rounded-full bg-emerald-500 animate-pulse"></span>
      </div>
      <div class="flex items-center gap-1.5">
        <Database class="w-3.5 h-3.5 text-slate-400" />
        <span>Chroma Vector</span>
        <span class="h-1.5 w-1.5 rounded-full bg-emerald-500 animate-pulse"></span>
      </div>
    </div>
  </header>
</template>
