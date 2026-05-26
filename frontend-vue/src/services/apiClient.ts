export interface FetchOptions extends RequestInit {
  params?: Record<string, string | number | boolean>
}

class ApiClient {
  private async request<T>(url: string, options: FetchOptions = {}): Promise<T> {
    const { params, headers, ...customOptions } = options
    
    let targetUrl = url
    if (params) {
      const queryParams = new URLSearchParams()
      Object.entries(params).forEach(([key, val]) => {
        queryParams.append(key, String(val))
      })
      targetUrl = `${url}?${queryParams.toString()}`
    }

    const defaultHeaders = {
      'Content-Type': 'application/json',
      ...headers
    }

    const response = await fetch(targetUrl, {
      ...customOptions,
      headers: defaultHeaders
    })

    if (!response.ok) {
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
