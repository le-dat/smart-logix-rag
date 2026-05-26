<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { 
  LayoutDashboard, 
  Compass, 
  MessageSquare, 
  Cpu,
  ChevronLeft,
  ChevronRight,
  Sun,
  Moon,
  Plus,
  X
} from '@lucide/vue'

const route = useRoute()
const router = useRouter()
const isCollapsed = ref(false)
const isDark = ref(false)
const isMobileOpen = ref(false)

// Handle Theme Switcher
const initTheme = () => {
  const savedTheme = localStorage.getItem('theme')
  const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches
  
  if (savedTheme === 'dark' || (!savedTheme && prefersDark)) {
    isDark.value = true
    document.documentElement.classList.add('dark')
  } else {
    isDark.value = false
    document.documentElement.classList.remove('dark')
  }
}

const toggleTheme = () => {
  isDark.value = !isDark.value
  if (isDark.value) {
    document.documentElement.classList.add('dark')
    localStorage.setItem('theme', 'dark')
  } else {
    document.documentElement.classList.remove('dark')
    localStorage.setItem('theme', 'light')
  }
}

const handleNewThread = () => {
  router.push('/chat').then(() => {
    // If already on /chat, trigger a soft page reload or local chat reset
    window.dispatchEvent(new CustomEvent('reset-chat'))
  })
}

// Mobile sidebar custom events
const handleToggleMobile = () => {
  isMobileOpen.value = !isMobileOpen.value
}

const handleCloseMobile = () => {
  isMobileOpen.value = false
}

// Auto-close sidebar on mobile navigation
watch(() => route.path, () => {
  isMobileOpen.value = false
})

onMounted(() => {
  initTheme()
  // Check if sidebar state was saved
  const savedSidebar = localStorage.getItem('sidebar-collapsed')
  if (savedSidebar === 'true') {
    isCollapsed.value = true
  }
  window.addEventListener('toggle-mobile-sidebar', handleToggleMobile)
  window.addEventListener('close-mobile-sidebar', handleCloseMobile)
})

onUnmounted(() => {
  window.removeEventListener('toggle-mobile-sidebar', handleToggleMobile)
  window.removeEventListener('close-mobile-sidebar', handleCloseMobile)
})

const toggleSidebar = () => {
  isCollapsed.value = !isCollapsed.value
  localStorage.setItem('sidebar-collapsed', String(isCollapsed.value))
}
</script>

<template>
  <!-- Mobile Backdrop overlay -->
  <div 
    v-if="isMobileOpen" 
    @click="isMobileOpen = false" 
    class="fixed inset-0 bg-black/45 backdrop-blur-[1px] z-40 md:hidden animate-in fade-in duration-200"
  ></div>

  <!-- Sidebar Container -->
  <aside 
    class="shrink-0 glass-panel border-r border-brand-border flex flex-col justify-between z-50 fixed md:relative md:z-20 inset-y-0 left-0 transition-all duration-300 ease-in-out bg-brand-panel md:flex"
    :class="[
      isCollapsed ? 'md:w-16' : 'md:w-60',
      isMobileOpen ? 'w-60 translate-x-0' : 'w-60 -translate-x-full md:translate-x-0'
    ]"
  >
    <!-- Collapse toggle button (desktop only) -->
    <button 
      @click="toggleSidebar"
      class="absolute top-7 -right-3 h-6 w-6 rounded-full border border-brand-border bg-card-bg text-text-primary hidden md:flex items-center justify-center cursor-pointer shadow-sm hover:border-brand-accent transition-colors z-30"
      :aria-label="isCollapsed ? 'Expand sidebar' : 'Collapse sidebar'"
      :title="isCollapsed ? 'Expand' : 'Collapse'"
    >
      <ChevronLeft v-if="!isCollapsed" class="w-3.5 h-3.5" />
      <ChevronRight v-else class="w-3.5 h-3.5" />
    </button>

    <div class="py-6 px-4 space-y-6 flex flex-col items-stretch">
      <!-- Logo Brand Row -->
      <div class="flex items-center justify-between px-2">
        <div 
          class="flex items-center gap-3 transition-all duration-300"
          :class="isCollapsed ? 'md:justify-center' : ''"
        >
          <div class="h-8 w-8 rounded-lg bg-text-primary text-brand-bg flex items-center justify-center shadow-sm shrink-0">
            <Cpu class="w-5 h-5" />
          </div>
          <span 
            v-if="!isCollapsed" 
            class="font-brand font-black tracking-widest text-base text-text-primary uppercase animate-in fade-in duration-200"
          >
            SmartLogix
          </span>
        </div>
        <!-- Mobile Close Button -->
        <button 
          @click="isMobileOpen = false"
          class="md:hidden p-1.5 rounded-lg text-text-secondary hover:text-brand-accent hover:bg-brand-panel transition-colors cursor-pointer"
          aria-label="Close menu"
          title="Close menu"
        >
          <X class="w-5 h-5" />
        </button>
      </div>

      <!-- New Thread Button (Perplexity Style) -->
      <button 
        @click="handleNewThread"
        class="flex items-center justify-center cursor-pointer transition-all duration-200"
        :class="isCollapsed 
          ? 'md:h-9 md:w-9 md:rounded-full bg-brand-accent text-white hover:bg-emerald-600 shadow-sm shadow-emerald-500/20 w-full rounded-full py-2.5' 
          : 'btn-capsule-primary py-2.5 px-4 text-sm tracking-wider uppercase flex gap-2 font-bold w-full shadow-md shadow-brand-accent/5'"
        :title="isCollapsed ? 'New Thread' : ''"
      >
        <Plus class="w-4.5 h-4.5 shrink-0" />
        <span v-if="!isCollapsed" class="animate-in fade-in duration-200">New Thread</span>
      </button>

      <!-- Navigation Links -->
      <nav class="space-y-1.5 pt-2">
        <!-- Dashboard Link -->
        <router-link 
          to="/" 
          v-slot="{ isActive }" 
          class="block"
        >
          <span 
            class="flex items-center rounded-lg text-sm font-bold uppercase tracking-wider transition-all duration-200"
            :class="[
              isActive 
                ? 'bg-card-bg text-text-primary border border-brand-border shadow-sm' 
                : 'text-text-secondary hover:text-text-primary border border-transparent hover:bg-text-primary/[0.02]',
              isCollapsed ? 'md:justify-center md:p-2.5' : 'gap-3.5 px-4 py-2.5'
            ]"
            :title="isCollapsed ? 'Dashboard' : ''"
          >
            <LayoutDashboard class="w-4 h-4 shrink-0" />
            <span v-if="!isCollapsed" class="animate-in fade-in duration-200">Dashboard</span>
          </span>
        </router-link>

        <!-- Risk Predictor Link -->
        <router-link 
          to="/predict" 
          v-slot="{ isActive }" 
          class="block"
        >
          <span 
            class="flex items-center rounded-lg text-sm font-bold uppercase tracking-wider transition-all duration-200"
            :class="[
              isActive 
                ? 'bg-card-bg text-text-primary border border-brand-border shadow-sm' 
                : 'text-text-secondary hover:text-text-primary border border-transparent hover:bg-text-primary/[0.02]',
              isCollapsed ? 'md:justify-center md:p-2.5' : 'gap-3.5 px-4 py-2.5'
            ]"
            :title="isCollapsed ? 'Risk Predictor' : ''"
          >
            <Compass class="w-4 h-4 shrink-0" />
            <span v-if="!isCollapsed" class="animate-in fade-in duration-200">Risk Predictor</span>
          </span>
        </router-link>

        <!-- AI Copilot Link -->
        <router-link 
          to="/chat" 
          v-slot="{ isActive }" 
          class="block"
        >
          <span 
            class="flex items-center rounded-lg text-sm font-bold uppercase tracking-wider transition-all duration-200"
            :class="[
              isActive 
                ? 'bg-card-bg text-text-primary border border-brand-border shadow-sm' 
                : 'text-text-secondary hover:text-text-primary border border-transparent hover:bg-text-primary/[0.02]',
              isCollapsed ? 'md:justify-center md:p-2.5' : 'gap-3.5 px-4 py-2.5'
            ]"
            :title="isCollapsed ? 'AI Copilot' : ''"
          >
            <MessageSquare class="w-4 h-4 shrink-0" />
            <span v-if="!isCollapsed" class="animate-in fade-in duration-200">AI Copilot</span>
          </span>
        </router-link>
      </nav>
    </div>

    <!-- Theme Switcher & Utility Panel -->
    <div class="p-4 border-t border-brand-border space-y-4">
      <div 
        class="flex items-center"
        :class="isCollapsed ? 'md:justify-center' : 'justify-between px-2'"
      >
        <!-- Dynamic Switcher Pill (Perplexity-style) -->
        <button 
          @click="toggleTheme"
          class="h-8 rounded-lg flex items-center justify-center border border-brand-border bg-card-bg cursor-pointer hover:border-brand-accent transition-colors"
          :class="isCollapsed ? 'md:w-8' : 'w-full gap-2 text-sm font-semibold text-text-secondary hover:text-text-primary'"
          :title="isDark ? 'Switch to Light Mode' : 'Switch to Dark Mode'"
        >
          <Sun v-if="isDark" class="w-4 h-4 text-amber-500 animate-in spin-in-12 duration-300" />
          <Moon v-else class="w-4 h-4 text-indigo-500 animate-in spin-in-12 duration-300" />
          <span v-if="!isCollapsed" class="animate-in fade-in duration-200">
            {{ isDark ? 'Light Theme' : 'Dark Theme' }}
          </span>
        </button>
      </div>
    </div>
  </aside>
</template>
