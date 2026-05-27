export interface Citation {
  source: string
  content_snippet: string
}

export interface StepInfo {
  id: number
  title: string
  icon: 'search' | 'db' | 'insight' | 'llm'
  status: 'pending' | 'running' | 'completed'
  citations?: Citation[] // Optional inline RAG source matches for this step
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
  steps?: StepInfo[] // List of agent research steps
}

export interface ChatResponse {
  response: string
  citations?: Citation[]
}
