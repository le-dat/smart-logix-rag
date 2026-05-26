import { useAuthStore } from '../stores/auth'

export const API_NET = import.meta.env.VITE_API_NET || 'http://localhost:5000'
export const API_PYTHON = import.meta.env.VITE_API_PYTHON || 'http://localhost:8000'

export interface FetchOptions extends RequestInit {
  params?: Record<string, string | number | boolean>
  /** If true, bypass auth token injection (e.g., for public login endpoint) */
  skipAuth?: boolean
}

class ApiClient {
  private async request<T>(url: string, options: FetchOptions = {}): Promise<T> {
    const { params, headers, skipAuth, ...customOptions } = options

    let targetUrl = url
    if (params) {
      const queryParams = new URLSearchParams()
      Object.entries(params).forEach(([key, val]) => {
        queryParams.append(key, String(val))
      })
      targetUrl = `${url}?${queryParams.toString()}`
    }

    // Inject JWT Bearer token automatically from the auth store
    const authHeaders: Record<string, string> = {}
    if (!skipAuth) {
      try {
        const authStore = useAuthStore()
        if (authStore.token) {
          authHeaders['Authorization'] = `Bearer ${authStore.token}`
        }
      } catch {
        // Store may not be initialized yet (e.g., during login)
      }
    }

    const defaultHeaders = {
      'Content-Type': 'application/json',
      ...authHeaders,
      ...(headers as Record<string, string> | undefined)
    }

    const response = await fetch(targetUrl, {
      ...customOptions,
      headers: defaultHeaders
    })

    if (!response.ok) {
      // If 401, the token has expired — redirect to login
      if (response.status === 401) {
        try {
          const authStore = useAuthStore()
          authStore.logout()
          window.location.href = '/login'
        } catch { /* ignore */ }
      }
      let errMsg = `API error: ${response.statusText}`
      try {
        const errBody = await response.json()
        errMsg = errBody.message || errMsg
      } catch {
        // Fallback to generic status text
      }
      throw new Error(errMsg)
    }

    // Handle 204 No Content
    if (response.status === 204) {
      return {} as T
    }

    return response.json() as Promise<T>
  }

  get<T>(url: string, options: FetchOptions = {}): Promise<T> {
    return this.request<T>(url, { ...options, method: 'GET' })
  }

  post<T>(url: string, data?: any, options: FetchOptions = {}): Promise<T> {
    return this.request<T>(url, {
      ...options,
      method: 'POST',
      body: data ? JSON.stringify(data) : undefined
    })
  }

  delete<T>(url: string, options: FetchOptions = {}): Promise<T> {
    return this.request<T>(url, { ...options, method: 'DELETE' })
  }
}

export const apiClient = new ApiClient()
