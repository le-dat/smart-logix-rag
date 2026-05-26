<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import Sidebar from './components/layout/Sidebar.vue'
import Header from './components/layout/Header.vue'

const route = useRoute()
const isLoginPage = computed(() => route.name === 'Login')
</script>

<template>
  <!-- Full-page login layout (no chrome) -->
  <router-view v-if="isLoginPage" />

  <!-- Authenticated app shell -->
  <div v-else class="min-h-screen flex text-slate-800 font-sans antialiased overflow-x-hidden bg-[#f9f8f4]">

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
