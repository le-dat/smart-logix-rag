<script setup lang="ts">
import { ref, computed } from 'vue'
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
</script>

<template>
  <div class="min-h-screen bg-[#0d0d0f] flex items-center justify-center p-4 relative overflow-hidden">

    <!-- Ambient glow effects -->
    <div class="pointer-events-none absolute inset-0">
      <div class="absolute top-[-10%] left-[-5%] w-[500px] h-[500px] rounded-full bg-indigo-600/10 blur-[120px]"></div>
      <div class="absolute bottom-[-10%] right-[-5%] w-[500px] h-[500px] rounded-full bg-violet-600/10 blur-[120px]"></div>
      <div class="absolute top-[40%] left-[50%] -translate-x-1/2 w-[300px] h-[300px] rounded-full bg-cyan-500/5 blur-[100px]"></div>
    </div>

    <!-- Dot grid pattern -->
    <div class="pointer-events-none absolute inset-0"
      style="background-image: radial-gradient(rgba(255,255,255,0.04) 1px, transparent 1px); background-size: 28px 28px;">
    </div>

    <!-- Login card -->
    <div class="relative z-10 w-full max-w-md">

      <!-- Logo / Brand -->
      <div class="text-center mb-8">
        <div class="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-gradient-to-br from-indigo-500 to-violet-600 shadow-lg shadow-indigo-500/30 mb-4">
          <Cpu class="w-7 h-7 text-white" />
        </div>
        <h1 class="text-2xl font-extrabold text-white tracking-tight">SmartLogix</h1>
        <p class="text-slate-400 text-sm mt-1 font-medium">AI Logistics Operations Hub</p>
      </div>

      <!-- Card -->
      <div class="bg-white/[0.04] border border-white/[0.08] rounded-2xl p-8 backdrop-blur-sm shadow-2xl">
        <div class="flex items-center gap-2 mb-6">
          <Shield class="w-4 h-4 text-indigo-400" />
          <h2 class="text-white font-bold text-sm uppercase tracking-widest">Gateway Access</h2>
        </div>

        <form @submit.prevent="handleLogin" class="space-y-4" id="login-form">
          <!-- Username -->
          <div>
            <label for="login-username" class="block text-xs font-semibold text-slate-400 mb-1.5 uppercase tracking-wider">
              Username
            </label>
            <input
              id="login-username"
              v-model="username"
              type="text"
              autocomplete="username"
              placeholder="Enter your username"
              required
              :disabled="authStore.loading"
              class="w-full px-4 py-3 rounded-xl bg-white/[0.06] border border-white/[0.1] text-white placeholder-slate-500 text-sm font-medium
                     focus:outline-none focus:border-indigo-500/80 focus:ring-1 focus:ring-indigo-500/40
                     transition-all duration-200 disabled:opacity-50"
            />
          </div>

          <!-- Password -->
          <div>
            <label for="login-password" class="block text-xs font-semibold text-slate-400 mb-1.5 uppercase tracking-wider">
              Password
            </label>
            <div class="relative">
              <input
                id="login-password"
                v-model="password"
                :type="showPassword ? 'text' : 'password'"
                autocomplete="current-password"
                placeholder="Enter your password"
                required
                :disabled="authStore.loading"
                class="w-full px-4 py-3 pr-11 rounded-xl bg-white/[0.06] border border-white/[0.1] text-white placeholder-slate-500 text-sm font-medium
                       focus:outline-none focus:border-indigo-500/80 focus:ring-1 focus:ring-indigo-500/40
                       transition-all duration-200 disabled:opacity-50"
              />
              <button
                type="button"
                @click="showPassword = !showPassword"
                class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-500 hover:text-slate-300 transition-colors"
                :aria-label="showPassword ? 'Hide password' : 'Show password'"
              >
                <Eye v-if="!showPassword" class="w-4 h-4" />
                <EyeOff v-else class="w-4 h-4" />
              </button>
            </div>
          </div>

          <!-- Error message -->
          <transition name="slide-down">
            <div v-if="authStore.error" 
                 class="flex items-start gap-2 text-xs text-red-400 bg-red-500/10 border border-red-500/20 rounded-xl px-3 py-2.5">
              <span class="text-red-400 shrink-0 mt-0.5">⚠</span>
              <span>{{ authStore.error }}</span>
            </div>
          </transition>

          <!-- Login button -->
          <button
            id="login-submit-btn"
            type="submit"
            :disabled="!isFormValid || authStore.loading"
            class="w-full flex items-center justify-center gap-2.5 py-3.5 rounded-xl font-bold text-sm
                   bg-gradient-to-r from-indigo-500 to-violet-600
                   hover:from-indigo-400 hover:to-violet-500
                   text-white shadow-lg shadow-indigo-500/25
                   transition-all duration-200
                   disabled:opacity-40 disabled:cursor-not-allowed disabled:transform-none
                   hover:shadow-indigo-500/40 hover:-translate-y-0.5 active:translate-y-0"
          >
            <span v-if="authStore.loading" class="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin"></span>
            <LogIn v-else class="w-4 h-4" />
            <span>{{ authStore.loading ? 'Authenticating...' : 'Sign In' }}</span>
          </button>
        </form>

        <!-- Default credentials hint -->
        <div class="mt-6 pt-5 border-t border-white/[0.06]">
          <p class="text-center text-[11px] text-slate-600 font-mono">
            Default credentials: <span class="text-slate-400">admin</span> / <span class="text-slate-400">admin123</span>
          </p>
        </div>
      </div>

      <!-- Footer -->
      <p class="text-center text-[10px] text-slate-600 mt-6">
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
