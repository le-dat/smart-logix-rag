<script setup lang="ts">
import { useRoute, useRouter } from 'vue-router'
import { Server, Database, Cpu, LogOut, User, Menu } from '@lucide/vue'
import { useAuthStore } from '../../stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}

const toggleMobileMenu = () => {
  window.dispatchEvent(new CustomEvent('toggle-mobile-sidebar'))
}
</script>

<template>
  <header class="h-14 shrink-0 glass-panel border-b border-brand-border flex items-center justify-between px-6 z-10 bg-card-bg/40 backdrop-blur-md">
    <!-- Breadcrumb / Section indicators -->
    <div class="flex items-center gap-2">
      <!-- Mobile Hamburger Button -->
      <button 
        @click="toggleMobileMenu"
        class="md:hidden p-1.5 rounded-lg text-text-secondary hover:text-brand-accent hover:bg-brand-panel transition-all cursor-pointer mr-1 shrink-0"
        aria-label="Open navigation menu"
        title="Menu"
      >
        <Menu class="w-5 h-5" />
      </button>
      <span class="text-sm font-bold uppercase tracking-widest text-text-secondary font-mono font-sans">SmartLogix Hub</span>
      <span class="text-brand-border">/</span>
      <span class="text-sm font-black text-text-primary uppercase tracking-widest font-mono">{{ route.name || 'Dashboard' }}</span>
    </div>

    <!-- Right side: Health checks + User info -->
    <div class="flex items-center gap-4">
      <!-- Infrastructure Health Checks -->
      <div class="hidden lg:flex items-center gap-4 text-sm font-bold uppercase tracking-wider text-text-secondary font-mono">
        <div class="flex items-center gap-1.5 px-2 py-0.5 rounded-md border border-brand-border/40 bg-brand-panel/20">
          <Server class="w-3.5 h-3.5 text-text-secondary" />
          <span>.NET Gateway</span>
          <span class="h-1.5 w-1.5 rounded-full bg-emerald-500 shadow-md shadow-emerald-500/50 animate-pulse"></span>
        </div>
        <div class="flex items-center gap-1.5 px-2 py-0.5 rounded-md border border-brand-border/40 bg-brand-panel/20">
          <Cpu class="w-3.5 h-3.5 text-text-secondary" />
          <span>AI FastAPI</span>
          <span class="h-1.5 w-1.5 rounded-full bg-emerald-500 shadow-md shadow-emerald-500/50 animate-pulse"></span>
        </div>
        <div class="flex items-center gap-1.5 px-2 py-0.5 rounded-md border border-brand-border/40 bg-brand-panel/20">
          <Database class="w-3.5 h-3.5 text-text-secondary" />
          <span>Chroma Vector</span>
          <span class="h-1.5 w-1.5 rounded-full bg-emerald-500 shadow-md shadow-emerald-500/50 animate-pulse"></span>
        </div>
      </div>

      <!-- Divider -->
      <div class="h-5 w-px bg-brand-border hidden lg:block"></div>

      <!-- User + logout -->
      <div class="flex items-center gap-2 text-sm font-mono text-text-secondary">
        <User class="w-3.5 h-3.5" />
        <span class="font-bold text-text-primary">{{ authStore.user?.username || 'admin' }}</span>
        <span class="text-sm text-text-secondary bg-brand-panel px-1.5 py-0.5 rounded-full border border-brand-border">
          {{ authStore.user?.role || 'Admin' }}
        </span>
        <button
          id="logout-btn"
          @click="handleLogout"
          class="ml-1 p-1.5 rounded-lg text-text-secondary hover:text-brand-accent hover:bg-brand-panel transition-all cursor-pointer"
          aria-label="Sign out"
          title="Sign out"
        >
          <LogOut class="w-3.5 h-3.5" />
        </button>
      </div>
    </div>
  </header>
</template>
