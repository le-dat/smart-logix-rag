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

// Quick Recommendations List
const suggestions = [
  {
    title: 'Taoyuan Air Clearance average speed',
    desc: 'Retrieve standard customs manuals for Taipei Taoyuan (TPE) air cargo.',
    focus: 'Customs',
    query: 'What is the average customs clearance time at Taoyuan (TPE) and the delay risks?'
  },
  {
    title: 'Diagnose heavy ocean freight delays',
    desc: 'Verify cross-Pacific shipping rules for machinery above 3,000 kg.',
    focus: 'Routes',
    query: 'Analyze risks and delay factors for ocean shipping routes from PVG to LAX.'
  },
  {
    title: 'Review Noi Bai dispatch procedures',
    desc: 'Check fast-track customs codes at Hanoi Noi Bai (HAN) air ports.',
    focus: 'Customs',
    query: 'What are the rules and standard hours for cargo clearance at Noi Bai SGN/HAN?'
  }
]

// Clicking recommendation fills prompt and sends immediately
const handleSuggestionClick = (sug: typeof suggestions[0]) => {
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

// offline simulator for safety/offline demonstration
const simulateFallback = (prompt: string, aiMessage: Message) => {
  const query = prompt.toLowerCase()
  let answer = ""
  let citations: Citation[] = []

  if (selectedFocus.value === 'Customs' || query.includes('customs') || query.includes('clearance') || query.includes('hải quan') || query.includes('taoyuan')) {
    answer = `Based on Dimerco standard operating procedures for **Air Freight Customs Clearance** [1] and cargo manuals:\n\n1. **Standard Lead Time:** Taoyuan TPE averages **4 to 6 operating hours**; Hanoi Noi Bai HAN averages **8 operating hours** [2] for standard declarations.\n2. **High-Risk Thresholds:** Any cargo weighing more than **3,000 kg** [1] or containing electronics/batteries triggers physical inspection audits, increasing processing volatility by 35%.\n3. **Recommended Preventive Action:** Pre-alert the broker 24 hours prior to landing to prevent manifest registration delays.\n\nWould you like me to cross-examine specific transit schedules?`
    citations = [
      {
        source: 'dimerco_tpe_air_clearance.txt',
        content_snippet: 'Taoyuan (TPE) standard customs manifest registration takes 4-6 hours. Hanoi (HAN) averages 8 hours for general electronic cargo under 2,000 kg.'
      },
      {
        source: 'dimerco_customs_audits_2026.txt',
        content_snippet: 'Automatic physical inspection procedures trigger for specialized shipments above 3,000 kg or cargo containing dangerous materials, causing 6-12 hr volatility.'
      }
    ]
  } else if (selectedFocus.value === 'Routes' || query.includes('route') || query.includes('tuyến đường') || query.includes('delay') || query.includes('pvg') || query.includes('lax')) {
    answer = `Evaluating historical transit logs for the PVG (Shanghai) to LAX (Los Angeles) shipping corridor [1]:\n\n* **Average Flight Duration:** 120 hours (5 days) [2] including airport ground operations.\n* **Delay Multiplier:** Severe weather and terminal ground backlogs add **24-48 hours** [1] to the dispatch timeframe.\n* **Carrier Volatility:** DHL Express holds the highest reliability score on this route at **91%** [2] compared to competitors.\n\nYou can use our XGBoost Risk Predictor tool to evaluate structural delays for your custom route weight.`
    citations = [
      {
        source: 'dimerco_shanghai_la_routing.txt',
        content_snippet: 'Shanghai Pudong (PVG) to Los Angeles (LAX) experiences high congestion scores during Peak Logistics Season (Q4) adding 24-48 hours average delay.'
      },
      {
        source: 'carrier_on_time_performance_2026.txt',
        content_snippet: 'Comparative reliability on transpacific runs: DHL Logistics averages 91% on-time threshold, followed by Cathay Pacific Cargo at 82%.'
      }
    ]
  } else {
    answer = `I searched Dimerco's knowledge repositories [1] in ChromaDB but found no exact matching FAQ. Here is a general RAG overview:\n\n* **Transit Rules:** Air shipping is expedited within 1-3 days globally [1]; sea shipping ranges from 15-30 days.\n* **Recommendation:** Ensure all dispatch packing slips are digitised to avoid manual customs logs and bottlenecks [2].\n\nAsk a follow-up about customs manuals or specific routes to fetch deeper vectors!`
    citations = [
      {
        source: 'dimerco_logistics_general.txt',
        content_snippet: 'General cargo rules: Standard air transit is scheduled for 1-3 days depending on regional airport infrastructure clearances.'
      },
      {
        source: 'dimerco_digital_declarations_faq.txt',
        content_snippet: 'Digitizing packing lists reduces manual administrative looping errors by 90% across Taoyuan, Noi Bai, and regional sea ports.'
      }
    ]
  }

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
            class="w-full bg-transparent text-xs text-text-primary placeholder-text-secondary/60 focus:outline-none resize-none px-2 pt-1 font-medium leading-relaxed"
          ></textarea>

          <!-- Inline Actions toolbar -->
          <div class="flex items-center justify-between border-t border-brand-border/40 pt-3 px-1.5">
            <div class="flex items-center gap-2">
              
              <!-- Focus selector dropdown -->
              <div class="relative">
                <button 
                  type="button"
                  @click="isFocusDropdownOpen = !isFocusDropdownOpen"
                  class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg border border-brand-border bg-brand-panel text-[10px] font-bold text-text-secondary hover:text-text-primary hover:border-brand-accent transition cursor-pointer select-none"
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
                    class="w-full text-left px-2.5 py-1.5 text-[10px] font-bold rounded-lg hover:bg-brand-panel hover:text-brand-accent transition flex items-center gap-2 cursor-pointer"
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
                  class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg border border-brand-border bg-brand-panel text-[10px] font-bold text-text-secondary hover:text-text-primary hover:border-brand-accent transition cursor-pointer select-none"
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
                    class="w-full text-left px-2.5 py-1.5 text-[10px] font-bold rounded-lg hover:bg-brand-panel hover:text-brand-accent transition flex items-center gap-2 cursor-pointer"
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
          v-for="sug in suggestions" 
          :key="sug.title"
          @click="handleSuggestionClick(sug)"
          class="p-3.5 rounded-xl border border-brand-border bg-card-bg hover:border-brand-accent cursor-pointer transition-all duration-300 hover:scale-[1.01] hover:shadow-md flex flex-col justify-between text-left group"
        >
          <div class="space-y-1">
            <h3 class="text-[11px] font-extrabold text-text-primary tracking-wide line-clamp-1 group-hover:text-brand-accent transition-colors">
              {{ sug.title }}
            </h3>
            <p class="text-[10px] text-text-secondary leading-relaxed line-clamp-2">
              {{ sug.desc }}
            </p>
          </div>
          <div class="flex items-center gap-1 mt-3.5 text-[9px] font-bold text-brand-accent font-mono uppercase tracking-wider">
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
          <h2 class="text-xs font-black uppercase tracking-wider font-mono"> RAG Investigation Thread </h2>
        </div>
        <button 
          @click="resetChat"
          class="text-[9px] font-extrabold text-text-secondary hover:text-brand-accent uppercase tracking-wider bg-brand-panel border border-brand-border px-2 py-1 rounded-lg cursor-pointer transition"
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
        <div v-if="loading" class="flex items-center gap-3 text-text-secondary text-xs pl-2 select-none animate-pulse">
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
              class="flex items-center gap-1 px-2.5 py-1.5 rounded-lg border border-brand-border bg-brand-panel text-[9px] font-bold text-text-secondary select-none shrink-0"
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
              class="flex-grow bg-transparent text-xs text-text-primary placeholder-text-secondary/60 focus:outline-none px-1 py-1 font-medium"
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
        <div class="flex justify-between max-w-3xl mx-auto text-[8px] text-text-secondary/60 font-bold px-3 mt-1.5 font-mono select-none">
          <span>Focus filters active: {{ selectedFocus === 'All' ? 'All ChromaDB Vectors' : selectedFocus }}</span>
          <span>Fast RAG Search Active</span>
        </div>
      </div>

    </div>

  </div>
</template>
