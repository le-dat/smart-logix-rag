<script setup lang="ts">
import { useRoute, useRouter } from "vue-router";
import { LogOut, Menu } from "@lucide/vue";
import { useAuthStore } from "../../stores/auth";

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();

const handleLogout = () => {
  authStore.logout();
  router.push("/login");
};

const toggleMobileMenu = () => {
  window.dispatchEvent(new CustomEvent("toggle-mobile-sidebar"));
};
</script>

<template>
  <header
    class="h-14 shrink-0 glass-panel border-b border-brand-border flex items-center justify-between px-6 z-10 bg-card-bg/40 backdrop-blur-md"
  >
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
      <span class="text-sm font-black text-text-primary uppercase tracking-widest font-mono">{{
        route.name || "Dashboard"
      }}</span>
    </div>

    <!-- Right side: Health checks + User info -->
    <div class="flex items-center gap-4">
      <!-- Divider -->
      <div class="h-5 w-px bg-brand-border hidden lg:block"></div>

      <!-- logout -->
      <div class="flex items-center gap-2 text-sm font-mono text-text-secondary">
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
