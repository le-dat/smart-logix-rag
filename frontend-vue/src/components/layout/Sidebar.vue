<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { 
  LayoutDashboard, 
  Compass, 
  MessageSquare, 
  Cpu,
  ChevronLeft,
  ChevronRight,
  Sun,
  Moon,
  Plus
} from '@lucide/vue'

const router = useRouter()
const isCollapsed = ref(false)
const isDark = ref(false)

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

onMounted(() => {
  initTheme()
  // Check if sidebar state was saved
  const savedSidebar = localStorage.getItem('sidebar-collapsed')
  if (savedSidebar === 'true') {
    isCollapsed.value = true
  }
})

const toggleSidebar = () => {
  isCollapsed.value = !isCollapsed.value
  localStorage.setItem('sidebar-collapsed', String(isCollapsed.value))
}
</script>

<template>
  <aside 
    class="shrink-0 glass-panel border-r border-brand-border flex flex-col justify-between hidden md:flex z-20 relative transition-all duration-300 ease-in-out bg-brand-panel"
    :class="isCollapsed ? 'w-16' : 'w-60'"
  >
    <!-- Collapse toggle button -->
    <button 
      @click="toggleSidebar"
      class="absolute top-7 -right-3 h-6 w-6 rounded-full border border-brand-border bg-card-bg text-text-primary flex items-center justify-center cursor-pointer shadow-sm hover:border-brand-accent transition-colors z-30"
      :aria-label="isCollapsed ? 'Expand sidebar' : 'Collapse sidebar'"
      :title="isCollapsed ? 'Expand' : 'Collapse'"
    >
      <ChevronLeft v-if="!isCollapsed" class="w-3.5 h-3.5" />
      <ChevronRight v-else class="w-3.5 h-3.5" />
    </button>

    <div class="py-6 px-4 space-y-6 flex flex-col items-stretch">
      <!-- Logo Brand -->
      <div 
        class="flex items-center gap-3 transition-all duration-300"
        :class="isCollapsed ? 'justify-center' : 'px-2'"
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

      <!-- New Thread Button (Perplexity Style) -->
      <button 
        @click="handleNewThread"
        class="flex items-center justify-center cursor-pointer transition-all duration-200"
        :class="isCollapsed 
          ? 'h-9 w-9 rounded-full bg-brand-accent text-white hover:bg-emerald-600 shadow-sm shadow-emerald-500/20' 
          : 'btn-capsule-primary py-2.5 px-4 text-xs tracking-wider uppercase flex gap-2 font-bold w-full shadow-md shadow-brand-accent/5'"
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
            class="flex items-center rounded-lg text-xs font-bold uppercase tracking-wider transition-all duration-200"
            :class="[
              isActive 
                ? 'bg-card-bg text-text-primary border border-brand-border shadow-sm' 
                : 'text-text-secondary hover:text-text-primary border border-transparent hover:bg-text-primary/[0.02]',
              isCollapsed ? 'justify-center p-2.5' : 'gap-3.5 px-4 py-2.5'
            ]"
            :title="isCollapsed ? 'Dashboard' : ''"
          >
            <LayoutDashboard class="w-4 h-4 shrink-0" />
            <span v-if="!isCollapsed" class="animate-in fade-in duration-200">Dashboard</span>
          </span>
        </router-link>

        <!-- Predictor Link -->
        <router-link 
          to="/predict" 
          v-slot="{ isActive }" 
          class="block"
        >
          <span 
            class="flex items-center rounded-lg text-xs font-bold uppercase tracking-wider transition-all duration-200"
            :class="[
              isActive 
                ? 'bg-card-bg text-text-primary border border-brand-border shadow-sm' 
                : 'text-text-secondary hover:text-text-primary border border-transparent hover:bg-text-primary/[0.02]',
              isCollapsed ? 'justify-center p-2.5' : 'gap-3.5 px-4 py-2.5'
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
            class="flex items-center rounded-lg text-xs font-bold uppercase tracking-wider transition-all duration-200"
            :class="[
              isActive 
                ? 'bg-card-bg text-text-primary border border-brand-border shadow-sm' 
                : 'text-text-secondary hover:text-text-primary border border-transparent hover:bg-text-primary/[0.02]',
              isCollapsed ? 'justify-center p-2.5' : 'gap-3.5 px-4 py-2.5'
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
        :class="isCollapsed ? 'justify-center' : 'justify-between px-2'"
      >
        <!-- Dynamic Switcher Pill (Perplexity-style) -->
        <button 
          @click="toggleTheme"
          class="h-8 rounded-lg flex items-center justify-center border border-brand-border bg-card-bg cursor-pointer hover:border-brand-accent transition-colors"
          :class="isCollapsed ? 'w-8' : 'w-full gap-2 text-xs font-semibold text-text-secondary hover:text-text-primary'"
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
