<script setup lang="ts">
import { useRoute, useRouter } from 'vue-router'
import { Server, Database, Cpu, LogOut, User } from '@lucide/vue'
import { useAuthStore } from '../../stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}
</script>

<template>
  <header class="h-14 shrink-0 glass-panel border-b border-[#e4e2d8] flex items-center justify-between px-6 z-10 bg-white/40">
    <!-- Breadcrumb / Section indicators -->
    <div class="flex items-center gap-2">
      <span class="text-[10px] font-bold uppercase tracking-widest text-[#4a4943] font-mono">SmartLogix Hub</span>
      <span class="text-slate-300">/</span>
      <span class="text-[10px] font-extrabold text-[#1c1b17] uppercase tracking-widest font-mono">{{ route.name || 'Dashboard' }}</span>
    </div>

    <!-- Right side: Health checks + User info -->
    <div class="flex items-center gap-4">
      <!-- Infrastructure Health Checks -->
      <div class="hidden sm:flex items-center gap-4 text-[9px] font-bold uppercase tracking-wider text-slate-500 font-mono">
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

      <!-- Divider -->
      <div class="h-5 w-px bg-[#e4e2d8] hidden sm:block"></div>

      <!-- User + logout -->
      <div class="flex items-center gap-2 text-[10px] font-mono text-slate-500">
        <User class="w-3.5 h-3.5" />
        <span class="font-semibold text-[#1c1b17]">{{ authStore.user?.username || 'admin' }}</span>
        <span class="text-[9px] text-slate-400 bg-[#f3f2eb] px-1.5 py-0.5 rounded-full border border-[#e4e2d8]">
          {{ authStore.user?.role || 'Admin' }}
        </span>
        <button
          id="logout-btn"
          @click="handleLogout"
          class="ml-1 p-1.5 rounded-lg text-slate-400 hover:text-[#1c1b17] hover:bg-[#f3f2eb] transition-colors cursor-pointer"
          aria-label="Sign out"
          title="Sign out"
        >
          <LogOut class="w-3.5 h-3.5" />
        </button>
      </div>
    </div>
  </header>
</template>
