<script setup lang="ts">
import { ref, nextTick, onMounted, onUnmounted } from 'vue'
import { 
  Send, 
  Sparkles, 
  Globe, 
  BookOpen, 
  Compass, 
  Paperclip,
  ArrowRight,
  RefreshCw,
  Zap,
  MessageSquare
} from '@lucide/vue'
import { chatService } from '../services/chatService'
import { useTypewriter } from '../composables/useTypewriter'
import type { Message, Citation } from '../types'
import ChatMessage from '../components/chat/ChatMessage.vue'
import { CHAT_SUGGESTIONS, simulateChatFallback } from '../constants/mockData'

// Reactive Chat States
const messages = ref<Message[]>([])
const promptInput = ref('')
const selectedProvider = ref('Claude')
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
  const aiMessage: Message = {
    id: aiMsgId,
    role: 'assistant',
    text: '',
    displayText: '',
    isTyping: true,
    provider: selectedProvider.value,
    citations: []
  }
  messages.value.push(aiMessage)

  try {
    await chatService.askStream(
      prompt,
      selectedProvider.value,
      (token: string) => {
        aiMessage.text += token
        aiMessage.displayText += token
        scrollToBottom()
      },
      (citations: Citation[]) => {
        aiMessage.citations = citations
      },
      () => {
        aiMessage.isTyping = false
        loading.value = false
        scrollToBottom()
      },
      (err: string) => {
        console.error('SSE stream error, running offline simulation fallback.', err)
        loading.value = false
        simulateFallback(prompt, aiMessage)
      }
    )
  } catch (err: any) {
    console.error('FastAPI RAG error, running offline simulation fallback.', err)
    loading.value = false
    simulateFallback(prompt, aiMessage)
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
  <div class="h-[calc(100vh-6.5rem)] flex flex-col justify-between max-w-5xl mx-auto text-text-primary">
    
    <!-- EMPTY STATE: Centered Search Home -->
    <div 
      v-if="messages.length === 0" 
      class="flex-1 flex flex-col items-center justify-center max-w-2xl mx-auto w-full px-4 animate-in fade-in duration-500"
    >
      <!-- Title Heading -->
      <div class="text-center space-y-4 mb-8 select-none">
        <div class="inline-flex h-12 w-12 rounded-2xl bg-brand-accent/10 border border-brand-accent/20 items-center justify-center text-brand-accent mb-2 shadow-sm animate-pulse">
          <Sparkles class="w-6 h-6" />
        </div>
        <h1 class="text-3xl md:text-4xl font-brand font-black tracking-tight leading-none text-text-primary">
          Where logistics intelligence begins.
        </h1>
        <p class="text-text-secondary text-sm font-medium">
          Ask questions about customs clearance regulations, Taoyuan/Noi Bai operations, and transpacific schedules.
        </p>
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
          <div class="flex items-center justify-between border-t border-brand-border/40 pt-3 px-1.5">
            <div class="flex items-center gap-2">
              
              <!-- Focus selector dropdown -->
              <div class="relative">
                <button 
                  type="button"
                  @click="isFocusDropdownOpen = !isFocusDropdownOpen"
                  class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg border border-brand-border bg-brand-panel text-sm font-bold text-text-secondary hover:text-text-primary hover:border-brand-accent transition cursor-pointer select-none"
                >
                  <Globe class="w-3.5 h-3.5 text-brand-accent" />
                  <span>Focus: {{ selectedFocus }}</span>
                </button>
                <div 
                  v-if="isFocusDropdownOpen" 
                  class="absolute left-0 bottom-10 w-44 rounded-xl border border-brand-border bg-card-bg p-1.5 shadow-lg z-30 animate-in slide-in-from-bottom-2 duration-150"
                >
                  <button 
                    type="button"
                    v-for="focus in ['All', 'Customs', 'Routes']" 
                    :key="focus"
                    @click="selectedFocus = focus; isFocusDropdownOpen = false"
                    class="w-full text-left px-2.5 py-1.5 text-sm font-bold rounded-lg hover:bg-brand-panel hover:text-brand-accent transition flex items-center gap-2 cursor-pointer"
                    :class="selectedFocus === focus ? 'text-brand-accent bg-brand-accent-glow' : 'text-text-secondary'"
                  >
                    <Globe v-if="focus === 'All'" class="w-3 h-3" />
                    <BookOpen v-if="focus === 'Customs'" class="w-3 h-3" />
                    <Compass v-if="focus === 'Routes'" class="w-3 h-3" />
                    {{ focus === 'All' ? 'All (FAQ Vector)' : focus }}
                  </button>
                </div>
              </div>

              <!-- Model selector dropdown -->
              <div class="relative">
                <button 
                  type="button"
                  @click="isModelDropdownOpen = !isModelDropdownOpen"
                  class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg border border-brand-border bg-brand-panel text-sm font-bold text-text-secondary hover:text-text-primary hover:border-brand-accent transition cursor-pointer select-none"
                >
                  <Sparkles class="w-3.5 h-3.5 text-brand-accent" />
                  <span>Model: {{ selectedProvider }}</span>
                </button>
                <div 
                  v-if="isModelDropdownOpen" 
                  class="absolute left-0 bottom-10 w-40 rounded-xl border border-brand-border bg-card-bg p-1.5 shadow-lg z-30 animate-in slide-in-from-bottom-2 duration-150"
                >
                  <button 
                    type="button"
                    v-for="provider in ['Claude', 'GPT', 'Gemini', 'MiniMax']" 
                    :key="provider"
                    @click="selectedProvider = provider; isModelDropdownOpen = false"
                    class="w-full text-left px-2.5 py-1.5 text-sm font-bold rounded-lg hover:bg-brand-panel hover:text-brand-accent transition flex items-center gap-2 cursor-pointer"
                    :class="selectedProvider === provider ? 'text-brand-accent bg-brand-accent-glow' : 'text-text-secondary'"
                  >
                    <Zap class="w-3 h-3" />
                    {{ provider === 'Claude' ? 'Claude 3.5' : provider === 'GPT' ? 'GPT-4o Mini' : provider }}
                  </button>
                </div>
              </div>

              <!-- Attachment indicator (Visual placeholder) -->
              <button 
                type="button"
                title="Attach dispatch papers (FAQ, packing slips)"
                class="p-2 rounded-lg text-text-secondary hover:text-brand-accent hover:bg-brand-panel transition cursor-pointer"
              >
                <Paperclip class="w-3.5 h-3.5" />
              </button>
            </div>

            <!-- Submit circular button -->
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
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-3 w-full mt-6">
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
          <div class="flex items-center gap-1 mt-3.5 text-sm font-bold text-brand-accent font-mono uppercase tracking-wider">
            <span>Ask copilot</span>
            <ArrowRight class="w-3 h-3 transform group-hover:translate-x-1 transition-transform" />
          </div>
        </div>
      </div>
    </div>

    <!-- ACTIVE STATE: Scrolling Chat Thread & Sticky bottom search bar -->
    <div v-else class="flex-1 flex flex-col min-h-0 bg-transparent py-4 relative">
      <!-- Chat Header metadata -->
      <div class="flex justify-between items-center border-b border-brand-border/40 pb-4 mb-4 select-none">
        <div class="flex items-center gap-2">
          <MessageSquare class="w-4.5 h-4.5 text-brand-accent" />
          <h2 class="text-sm font-black uppercase tracking-wider font-mono"> RAG Investigation Thread </h2>
        </div>
        <button 
          @click="resetChat"
          class="text-sm font-extrabold text-text-secondary hover:text-brand-accent uppercase tracking-wider bg-brand-panel border border-brand-border px-2 py-1 rounded-lg cursor-pointer transition"
        >
          Reset Thread
        </button>
      </div>

      <!-- Scrollable Message list -->
      <div 
        ref="chatContainer"
        class="flex-grow overflow-y-auto space-y-6 pr-2 mb-20 scroll-smooth"
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
      <div class="absolute bottom-2 left-0 right-0 p-3 bg-brand-bg/85 backdrop-blur-md border-t border-brand-border/40">
        <div class="glass-card rounded-2xl p-2.5 shadow-lg border border-brand-border bg-card-bg max-w-3xl mx-auto">
          <form @submit.prevent="handleSend" class="flex items-center gap-2">
            <!-- Inline selectors in condensed bottom bar -->
            <button 
              type="button"
              @click="isFocusDropdownOpen = !isFocusDropdownOpen"
              class="flex items-center gap-1 px-2.5 py-1.5 rounded-lg border border-brand-border bg-brand-panel text-sm font-bold text-text-secondary select-none shrink-0"
              title="Change search focus"
            >
              <Globe class="w-3 h-3 text-brand-accent" />
              <span>{{ selectedFocus }}</span>
            </button>

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
        <div class="flex justify-between max-w-3xl mx-auto text-sm text-text-secondary/60 font-bold px-3 mt-1.5 font-mono select-none">
          <span>Focus filters active: {{ selectedFocus === 'All' ? 'All ChromaDB Vectors' : selectedFocus }}</span>
          <span>Fast RAG Search Active</span>
        </div>
      </div>

    </div>

  </div>
</template>
