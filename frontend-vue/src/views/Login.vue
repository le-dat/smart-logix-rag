<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { Eye, EyeOff, LogIn, Shield, Cpu } from '@lucide/vue'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const username = ref('')
const password = ref('')
const showPassword = ref(false)
const submitted = ref(false)

// Local state to check if theme is dark
const isDark = ref(false)

const isFormValid = computed(() =>
  username.value.trim().length >= 2 && password.value.length >= 3
)

const handleLogin = async () => {
  if (!isFormValid.value) return
  submitted.value = true

  const success = await authStore.login(username.value.trim(), password.value)
  if (success) {
    const redirect = route.query.redirect as string | undefined
    router.push(redirect || '/')
  }
  submitted.value = false
}

// Initialise theme states on mount
onMounted(() => {
  const savedTheme = localStorage.getItem('theme')
  const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches
  
  if (savedTheme === 'dark' || (!savedTheme && prefersDark)) {
    isDark.value = true
    document.documentElement.classList.add('dark')
  } else {
    isDark.value = false
    document.documentElement.classList.remove('dark')
  }
})
</script>

<template>
  <div class="min-h-screen bg-brand-bg flex items-center justify-center p-4 relative overflow-hidden transition-colors duration-300">

    <!-- Ambient glow effects (adapting to Light/Dark Mode) -->
    <div class="pointer-events-none absolute inset-0 select-none">
      <div 
        class="absolute top-[-10%] left-[-5%] w-[500px] h-[500px] rounded-full blur-[120px] transition-colors duration-300"
        :class="isDark ? 'bg-brand-accent/10' : 'bg-brand-accent/5'"
      ></div>
      <div 
        class="absolute bottom-[-10%] right-[-5%] w-[500px] h-[500px] rounded-full blur-[120px] transition-colors duration-300"
        :class="isDark ? 'bg-emerald-500/5' : 'bg-emerald-500/3'"
      ></div>
      <div class="absolute top-[40%] left-[50%] -translate-x-1/2 w-[300px] h-[300px] rounded-full bg-brand-accent/5 blur-[100px]"></div>
    </div>

    <!-- Dot grid pattern -->
    <div class="pointer-events-none absolute inset-0 select-none opacity-60"
      :style="isDark 
        ? 'background-image: radial-gradient(rgba(255,255,255,0.02) 1px, transparent 1px); background-size: 28px 28px;'
        : 'background-image: radial-gradient(rgba(25,26,25,0.025) 1px, transparent 1px); background-size: 28px 28px;'">
    </div>

    <!-- Login card container -->
    <div class="relative z-10 w-full max-w-md select-none">

      <!-- Logo / Brand -->
      <div class="text-center mb-8">
        <div class="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-text-primary text-brand-bg shadow-md mb-4 border border-brand-border/20">
          <Cpu class="w-7 h-7 text-brand-accent" />
        </div>
        <h1 class="text-3xl font-brand font-black tracking-tight text-text-primary uppercase">SmartLogix</h1>
        <p class="text-text-secondary text-xs mt-1 font-semibold uppercase tracking-wider font-mono">AI Logistics Operations Hub</p>
      </div>

      <!-- Main Login Card -->
      <div class="glass-card rounded-3xl p-8 shadow-xl bg-card-bg border border-brand-border/60">
        <div class="flex items-center gap-2 mb-6 border-b border-brand-border pb-3.5">
          <Shield class="w-4.5 h-4.5 text-brand-accent" />
          <h2 class="text-text-primary font-bold text-xs uppercase tracking-widest font-mono">Gateway Access</h2>
        </div>

        <form @submit.prevent="handleLogin" class="space-y-4" id="login-form">
          <!-- Username Input -->
          <div>
            <label for="login-username" class="block text-[10px] font-black text-text-secondary mb-1.5 uppercase tracking-wider font-mono">
              Username
            </label>
            <input
              id="login-username"
              v-model="username"
              type="text"
              autocomplete="username"
              placeholder="Enter username"
              required
              :disabled="authStore.loading"
              class="glass-input px-4 py-3 text-xs w-full font-semibold focus:outline-none"
            />
          </div>

          <!-- Password Input -->
          <div>
            <label for="login-password" class="block text-[10px] font-black text-text-secondary mb-1.5 uppercase tracking-wider font-mono">
              Password
            </label>
            <div class="relative">
              <input
                id="login-password"
                v-model="password"
                :type="showPassword ? 'text' : 'password'"
                autocomplete="current-password"
                placeholder="Enter password"
                required
                :disabled="authStore.loading"
                class="glass-input px-4 py-3 pr-11 text-xs w-full font-semibold focus:outline-none"
              />
              <button
                type="button"
                @click="showPassword = !showPassword"
                class="absolute right-3 top-1/2 -translate-y-1/2 text-text-secondary/70 hover:text-text-primary transition-colors cursor-pointer p-1 rounded-lg"
                :aria-label="showPassword ? 'Hide password' : 'Show password'"
              >
                <Eye v-if="!showPassword" class="w-4 h-4" />
                <EyeOff v-else class="w-4 h-4" />
              </button>
            </div>
          </div>

          <!-- Error Message Banner -->
          <transition name="slide-down">
            <div v-if="authStore.error" 
                 class="flex items-start gap-2.5 text-xs text-rose-500 bg-rose-500/10 border border-rose-500/20 rounded-xl px-3 py-2.5 font-medium leading-normal animate-in slide-in-from-top-2 duration-150">
              <span class="text-rose-500 shrink-0 mt-0.5 font-bold">⚠</span>
              <span>{{ authStore.error }}</span>
            </div>
          </transition>

          <!-- Login Button Capsule -->
          <button
            id="login-submit-btn"
            type="submit"
            :disabled="!isFormValid || authStore.loading"
            class="w-full flex items-center justify-center gap-2 py-3 rounded-full font-bold text-xs uppercase tracking-wider
                   btn-capsule-primary cursor-pointer shadow-md select-none mt-2 disabled:opacity-30 disabled:cursor-not-allowed"
          >
            <span v-if="authStore.loading" class="w-4 h-4 border-2 border-brand-bg/30 border-t-brand-bg rounded-full animate-spin"></span>
            <LogIn v-else class="w-4 h-4 shrink-0" />
            <span>{{ authStore.loading ? 'Authenticating...' : 'Sign In' }}</span>
          </button>
        </form>

        <!-- Default Credentials Hint -->
        <div class="mt-6 pt-5 border-t border-brand-border/60">
          <p class="text-center text-[10px] text-text-secondary/70 font-mono">
            Credentials: <span class="text-text-primary font-bold">admin</span> / <span class="text-text-primary font-bold">admin123</span>
          </p>
        </div>
      </div>

      <!-- Footer Info -->
      <p class="text-center text-[9px] font-black uppercase tracking-wider text-text-secondary/60 mt-6 font-mono">
        SmartLogix &copy; 2026 — AI Logistics Hub for Dimerco
      </p>
    </div>
  </div>
</template>

<style scoped>
.slide-down-enter-active,
.slide-down-leave-active {
  transition: all 0.2s ease;
}
.slide-down-enter-from {
  opacity: 0;
  transform: translateY(-6px);
}
.slide-down-leave-to {
  opacity: 0;
  transform: translateY(-4px);
}
</style>
