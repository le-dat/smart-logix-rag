import { API_NET } from './apiClient'
import { useAuthStore } from '../stores/auth'
import type { ChatResponse, Citation, StepInfo } from '../types'

export const chatService = {
  /**
   * Non-streaming chat via .NET Gateway proxy → Python FastAPI.
   */
  async ask(prompt: string, provider: string): Promise<ChatResponse> {
    const authStore = useAuthStore()
    const response = await fetch(`${API_NET}/api/ai/chat`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...(authStore.token ? { 'Authorization': `Bearer ${authStore.token}` } : {})
      },
      body: JSON.stringify({ prompt, provider })
    })

    if (!response.ok) {
      const errBody = await response.json().catch(() => ({}))
      throw new Error(errBody.message || `Chat error: ${response.statusText}`)
    }

    return response.json() as Promise<ChatResponse>
  },

  /**
   * Streaming SSE chat via .NET Gateway proxy → Python FastAPI.
   * Calls onToken for each incremental text chunk, onCitations once, and onDone at the end.
   */
  async askStream(
    prompt: string,
    provider: string,
    onToken: (token: string) => void,
    onCitations: (citations: Citation[]) => void,
    onStep: (step: StepInfo) => void,
    onDone: () => void,
    onError: (err: string) => void
  ): Promise<void> {
    const authStore = useAuthStore()

    let response: Response
    try {
      response = await fetch(`${API_NET}/api/ai/chat/stream`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...(authStore.token ? { 'Authorization': `Bearer ${authStore.token}` } : {})
        },
        body: JSON.stringify({ prompt, provider })
      })
    } catch (err: any) {
      onError(err.message || 'Network error connecting to AI Gateway.')
      return
    }

    if (!response.ok || !response.body) {
      const errText = await response.text().catch(() => 'Unknown error')
      onError(`Stream error ${response.status}: ${errText}`)
      return
    }

    const reader = response.body.getReader()
    const decoder = new TextDecoder()
    let buffer = ''

    try {
      while (true) {
        const { done, value } = await reader.read()
        if (done) break

        buffer += decoder.decode(value, { stream: true })

        // Process complete SSE lines from buffer
        const lines = buffer.split('\n')
        buffer = lines.pop() ?? '' // Keep incomplete last line in buffer

        for (const line of lines) {
          const trimmed = line.trim()
          if (!trimmed.startsWith('data: ')) continue

          try {
            const jsonStr = trimmed.slice(6)
            const chunk = JSON.parse(jsonStr)

            if (chunk.type === 'token') {
              onToken(chunk.token ?? '')
            } else if (chunk.type === 'citations') {
              onCitations(chunk.citations ?? [])
            } else if (chunk.type === 'step') {
              onStep(chunk.step)
            } else if (chunk.type === 'done') {
              onDone()
              return
            } else if (chunk.type === 'error') {
              onError(chunk.message ?? 'Unknown AI stream error')
              return
            }
          } catch {
            // Malformed SSE line, skip
          }
        }
      }
    } finally {
      reader.releaseLock()
    }

    onDone()
  }
}
