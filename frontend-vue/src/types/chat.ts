export interface Citation {
  source: string
  content_snippet: string
}

export interface Message {
  id: number
  role: 'user' | 'assistant'
  text: string
  displayText: string // Used for the simulated typing effect
  isTyping: boolean
  provider?: string
  citations?: Citation[]
  showCitations?: boolean
}

export interface ChatResponse {
  response: string
  citations?: Citation[]
}
