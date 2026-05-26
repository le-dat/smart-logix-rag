import { ref, computed } from 'vue'
import { defineStore } from 'pinia'

const API_NET = import.meta.env.VITE_API_NET || 'http://localhost:5000'
const TOKEN_KEY = 'smartlogix_token'
const USER_KEY = 'smartlogix_user'

export interface AuthUser {
  username: string
  role: string
  expiresAt: string
}

export const useAuthStore = defineStore('auth', () => {
  // ─── State ────────────────────────────────────────────────────────────────
  const token = ref<string | null>(localStorage.getItem(TOKEN_KEY))
  const user = ref<AuthUser | null>(JSON.parse(localStorage.getItem(USER_KEY) || 'null'))
  const loading = ref(false)
  const error = ref<string | null>(null)

  // ─── Getters ─────────────────────────────────────────────────────────────
  const isAuthenticated = computed(() => {
    if (!token.value || !user.value) return false
    // Check token expiry
    const expiresAt = new Date(user.value.expiresAt)
    return expiresAt > new Date()
  })

  // ─── Actions ─────────────────────────────────────────────────────────────
  const login = async (username: string, password: string): Promise<boolean> => {
    loading.value = true
    error.value = null
    try {
      const response = await fetch(`${API_NET}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password })
      })

      if (!response.ok) {
        const errBody = await response.json().catch(() => ({}))
        error.value = errBody.message || 'Invalid username or password.'
        return false
      }

      const data = await response.json()
      token.value = data.token
      user.value = {
        username: data.username,
        role: data.role,
        expiresAt: data.expiresAt
      }

      localStorage.setItem(TOKEN_KEY, data.token)
      localStorage.setItem(USER_KEY, JSON.stringify(user.value))
      return true
    } catch (err: any) {
      error.value = err.message || 'Network error. Could not connect to gateway.'
      return false
    } finally {
      loading.value = false
    }
  }

  const logout = () => {
    token.value = null
    user.value = null
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
  }

  return {
    token,
    user,
    loading,
    error,
    isAuthenticated,
    login,
    logout
  }
})
