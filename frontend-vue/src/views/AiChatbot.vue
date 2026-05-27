<script setup lang="ts">
import {
  Globe,
  RefreshCw,
  Send
} from '@lucide/vue'
import { nextTick, onMounted, onUnmounted, ref } from 'vue'
import ChatMessage from '../components/chat/ChatMessage.vue'
import { useTypewriter } from '../composables/useTypewriter'
import { CHAT_SUGGESTIONS, simulateChatFallback } from '../constants/mockData'
import { chatService } from '../services/chatService'
import type { Citation, Message, StepInfo } from '../types'

// Reactive Chat States
const messages = ref<Message[]>([])
const promptInput = ref('')
const selectedProvider = ref('MiniMax')
const selectedFocus = ref('All') // Focus: All, Customs, Routes
const isModelDropdownOpen = ref(false)
const isFocusDropdownOpen = ref(false)
const loading = ref(false)
const chatContainer = ref<HTMLElement | null>(null)

// Instantiate typewriter composable
const { type } = useTypewriter()

// Auto-scroll logic inside viewport
const scrollToBottom = async () => {
  await nextTick()
  if (chatContainer.value) {
    chatContainer.value.scrollTop = chatContainer.value.scrollHeight
  }
}

// Reset chat handler
const resetChat = () => {
  messages.value = []
  promptInput.value = ''
  selectedFocus.value = 'All'
  loading.value = false
}

// Listen to custom sidebar event
onMounted(() => {
  window.addEventListener('reset-chat', resetChat)
})

onUnmounted(() => {
  window.removeEventListener('reset-chat', resetChat)
})

// Clicking recommendation fills prompt and sends immediately
const handleSuggestionClick = (sug: typeof CHAT_SUGGESTIONS[0]) => {
  promptInput.value = sug.query
  selectedFocus.value = sug.focus
  handleSend()
}

// Handle sending prompt queries
const handleSend = async () => {
  const prompt = promptInput.value.trim()
  if (!prompt || prompt.length < 2 || prompt.length > 1000) return

  const userMsgId = Date.now()
  messages.value.push({
    id: userMsgId,
    role: 'user',
    text: prompt,
    displayText: prompt,
    isTyping: false
  })

  promptInput.value = ''
  loading.value = true
  isModelDropdownOpen.value = false
  isFocusDropdownOpen.value = false
  scrollToBottom()

  const aiMsgId = Date.now() + 1
  messages.value.push({
    id: aiMsgId,
    role: 'assistant',
    text: '',
    displayText: '',
    isTyping: true,
    provider: selectedProvider.value,
    citations: [],
    steps: []
  })

  const reactiveMessage = messages.value[messages.value.length - 1]

  try {
    await chatService.askStream(
      prompt,
      selectedProvider.value,
      (token: string) => {
        reactiveMessage.text += token
        reactiveMessage.displayText += token
        scrollToBottom()
      },
      (citations: Citation[]) => {
        reactiveMessage.citations = citations
      },
      (step: StepInfo) => {
        if (!reactiveMessage.steps) {
          reactiveMessage.steps = []
        }
        const existingIdx = reactiveMessage.steps.findIndex(s => s.id === step.id)
        if (existingIdx !== -1) {
          reactiveMessage.steps[existingIdx] = step
        } else {
          reactiveMessage.steps.push(step)
        }
        scrollToBottom()
      },
      () => {
        reactiveMessage.isTyping = false
        loading.value = false
        scrollToBottom()
      },
      (err: string) => {
        console.error('SSE stream error, running offline simulation fallback.', err)
        loading.value = false
        simulateFallback(prompt, reactiveMessage)
      }
    )
  } catch (err: any) {
    console.error('FastAPI RAG error, running offline simulation fallback.', err)
    loading.value = false
    simulateFallback(prompt, reactiveMessage)
  }
}

// offline simulator for safety/offline demonstration using mockData helper
const simulateFallback = (prompt: string, aiMessage: Message) => {
  const { answer, citations } = simulateChatFallback(prompt, selectedFocus.value)
  aiMessage.text = answer
  aiMessage.citations = citations
  
  // Trigger typewriter effect
  type(
    answer,
    (_, accumulated) => {
      aiMessage.displayText = accumulated
      scrollToBottom()
    },
    () => {
      aiMessage.isTyping = false
      scrollToBottom()
    }
  )
}
</script>

<template>
  <div class="h-[calc(100dvh-3.5rem-2.5rem)] md:h-[calc(100vh-5.2rem)] flex flex-col justify-between max-w-5xl mx-auto text-text-primary">
    
    <!-- EMPTY STATE: Centered Search Home -->
    <div 
      v-if="messages.length === 0" 
      class="flex-1 flex flex-col items-center justify-center max-w-2xl mx-auto w-full px-4 animate-in fade-in duration-500"
    >
      <!-- Title Heading -->
      <div class="text-center space-y-4 mb-8 select-none">
        <h1 class="text-2xl md:text-3xl lg:text-4xl font-brand font-black tracking-tight leading-none text-text-primary">
          SmartLogix
        </h1>
      </div>

      <!-- Centered Prompt Box Container -->
      <div class="w-full glass-card rounded-2xl p-3 shadow-lg border border-brand-border bg-card-bg">
        <form @submit.prevent="handleSend" class="space-y-3">
          <!-- TextArea Prompt -->
          <textarea 
            v-model="promptInput" 
            placeholder="Ask anything about customs clearance manuals or flight routing delays..." 
            rows="3"
            required
            @keydown.enter.prevent="handleSend"
            class="w-full bg-transparent text-sm text-text-primary placeholder-text-secondary/60 focus:outline-none resize-none px-2 pt-1 font-medium leading-relaxed"
          ></textarea>

          <!-- Inline Actions toolbar -->
          <div class="flex items-center justify-end pt-3 px-1.5">
            <button 
              type="submit" 
              :disabled="loading || promptInput.trim().length < 2"
              class="h-7 w-7 rounded-full bg-text-primary text-brand-bg flex items-center justify-center hover:bg-brand-accent hover:text-white transition disabled:opacity-30 cursor-pointer shadow-sm"
              aria-label="Submit search thread"
            >
              <Send class="w-3.5 h-3.5" />
            </button>
          </div>
        </form>
      </div>

      <!-- Quick Recommendations Grid -->
      <div class="flex flex-col gap-3 w-full mt-6">
        <div 
          v-for="sug in CHAT_SUGGESTIONS" 
          :key="sug.title"
          @click="handleSuggestionClick(sug)"
          class="p-3.5 rounded-xl border border-brand-border bg-card-bg hover:border-brand-accent cursor-pointer transition-all duration-300 hover:scale-[1.01] hover:shadow-md flex flex-col justify-between text-left group"
        >
          <div class="space-y-1">
            <h3 class="text-sm font-extrabold text-text-primary tracking-wide line-clamp-1 group-hover:text-brand-accent transition-colors">
              {{ sug.title }}
            </h3>
            <p class="text-sm text-text-secondary leading-relaxed line-clamp-2">
              {{ sug.desc }}
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- ACTIVE STATE: Scrolling Chat Thread & Sticky bottom search bar -->
    <div v-else class="max-w-3xl mx-auto flex-1 flex flex-col min-h-0 bg-transparent py-4 relative">
      <!-- Scrollable Message list -->
      <div 
        ref="chatContainer"
        class="flex-grow overflow-y-auto space-y-6 mb-16 scroll-smooth"
      >
        <ChatMessage 
          v-for="msg in messages" 
          :key="msg.id"
          :message="msg"
        />

        <!-- Loading streaming indicator -->
        <div v-if="loading" class="flex items-center gap-3 text-text-secondary text-sm pl-2 select-none animate-pulse">
          <RefreshCw class="w-3.5 h-3.5 animate-spin text-brand-accent" />
          <span>Generating context answers from Dimerco manual vector blocks...</span>
        </div>
      </div>

      <!-- Floating Sticky bottom search box panel -->
      <div class="absolute bottom-0 left-0 right-0 p-3 pb-safe bg-brand-bg/85 backdrop-blur-md">
        <div class="glass-card rounded-2xl p-2.5 shadow-lg border border-brand-border bg-card-bg max-w-3xl mx-auto">
          <form @submit.prevent="handleSend" class="flex items-center gap-2">
            <!-- Text Input -->
            <input 
              v-model="promptInput" 
              type="text" 
              placeholder="Ask a follow-up query regarding Taoyuan air speeds, customs inspection, or routes..." 
              required
              :disabled="loading"
              class="flex-grow bg-transparent text-sm text-text-primary placeholder-text-secondary/60 focus:outline-none px-1 py-1 font-medium"
            />

            <!-- Send button -->
            <button 
              type="submit" 
              :disabled="loading || promptInput.trim().length < 2"
              class="h-7 w-7 rounded-full bg-text-primary text-brand-bg flex items-center justify-center hover:bg-brand-accent hover:text-white transition disabled:opacity-30 cursor-pointer shadow-sm shrink-0"
              aria-label="Send follow up search"
            >
              <Send class="w-3 h-3" />
            </button>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>
