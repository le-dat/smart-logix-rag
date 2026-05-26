import { apiClient } from './apiClient'
import type { ChatResponse } from '../types'

const API_PYTHON = import.meta.env.VITE_API_PYTHON || 'http://localhost:8000'

export const chatService = {
  async ask(prompt: string, provider: string): Promise<ChatResponse> {
    return apiClient.post<ChatResponse>(`${API_PYTHON}/api/v1/chat/`, {
      prompt,
      provider
    })
  }
}
