<script setup lang="ts">
import Sidebar from './components/layout/Sidebar.vue'
import Header from './components/layout/Header.vue'
</script>

<template>
  <div class="min-h-screen flex text-slate-800 font-sans antialiased overflow-x-hidden bg-[#f9f8f4]">
    
    <!-- Sidebar Navigation -->
    <Sidebar />

    <!-- Main Content Panel -->
    <div class="flex-1 flex flex-col min-w-0">
      
      <!-- Top Health Indicators bar -->
      <Header />

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
