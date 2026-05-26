import type { Citation, PredictionInput, PredictionResult } from '../types'

// ==========================================
// 1. Chatbot Mock Data & Simulator Fallbacks
// ==========================================

export interface ChatSuggestion {
  title: string
  desc: string
  focus: string
  query: string
}

export const CHAT_SUGGESTIONS: ChatSuggestion[] = [
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

export interface ChatSimulationResult {
  answer: string
  citations: Citation[]
}

export function simulateChatFallback(prompt: string, selectedFocus: string): ChatSimulationResult {
  const query = prompt.toLowerCase()
  let answer = ""
  let citations: Citation[] = []

  if (selectedFocus === 'Customs' || query.includes('customs') || query.includes('clearance') || query.includes('hải quan') || query.includes('taoyuan')) {
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
  } else if (selectedFocus === 'Routes' || query.includes('route') || query.includes('tuyến đường') || query.includes('delay') || query.includes('pvg') || query.includes('lax')) {
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

  return { answer, citations }
}

// ==========================================
// 2. Risk Predictor Fallback Simulator
// ==========================================

export function simulatePredictFallback(form: PredictionInput): PredictionResult {
  let score = 0.15
  if (form.route_id === 4) score += 0.35
  if (form.weather_index > 3.5) score += 0.30
  if (form.weight > 3000) score += 0.15
  if (form.carrier === 'DHL Logistics') score -= 0.05
  
  score = Math.min(Math.max(score, 0.05), 0.95)
  
  let level = 'Low'
  if (score > 0.65) level = 'High'
  else if (score > 0.30) level = 'Medium'
  
  const wIdx = form.weather_index > 3.5 ? 0.45 : 0.20
  const rIdx = form.route_id === 4 ? 0.35 : 0.15
  const cIdx = 0.20
  const wtIdx = 0.15
  const total = wIdx + rIdx + cIdx + wtIdx
  
  const factors: string[] = []
  if (form.weather_index > 3.5) {
    factors.push('Severe weather indices encountered along flyway routing.')
  }
  if (form.route_id === 4) {
    factors.push('Cross-Pacific long-haul routes exhibit higher average congestion indices.')
  }
  if (form.weight > 2000) {
    factors.push('Heavy-weight freight requires complex container balancing checks.')
  }
  if (factors.length === 0) {
    factors.push('Standard freight parameters meet optimal seasonal safety profiles.')
  }

  return {
    risk_score: score,
    risk_level: level,
    contributing_factors: {
      'weather_index': wIdx / total,
      'route_id': rIdx / total,
      'carrier': cIdx / total,
      'weight': wtIdx / total
    },
    is_fallback: true,
    factors
  }
}

// ==========================================
// 3. Chart Mock & Standard Configuration Data
// ==========================================

export const DELAY_TREND_MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun']
export const DELAY_TREND_MONTH_NUMS = [1, 2, 3, 4, 5, 6]
export const DELAY_TREND_DEFAULT_TOTALS = [4, 6, 5, 8, 7, 10]
export const DELAY_TREND_DEFAULT_RATIOS = [0.25, 0.30, 0.28, 0.35, 0.32, 0.38]

export interface FeatureImportanceConfig {
  label: string
  key: string
  color: string
  bgColor: string
  description: string
}

export const RISK_FEATURE_CONFIG: FeatureImportanceConfig[] = [
  {
    label: 'Weather Index',
    key: 'weather_index',
    color: '#ef4444',
    bgColor: 'rgba(239, 68, 68, 0.12)',
    description: 'Meteorological disruption along flyways'
  },
  {
    label: 'Route Complexity',
    key: 'route_id',
    color: '#f97316',
    bgColor: 'rgba(249, 115, 22, 0.12)',
    description: 'Cross-Pacific vs. intra-Asia transit corridors'
  },
  {
    label: 'Cargo Weight',
    key: 'weight',
    color: '#eab308',
    bgColor: 'rgba(234, 179, 8, 0.12)',
    description: 'Heavy cargo triggers physical inspection gates'
  },
  {
    label: 'Carrier Performance',
    key: 'carrier',
    color: '#10b981',
    bgColor: 'rgba(16, 185, 129, 0.12)',
    description: 'Historical on-time delivery score by carrier'
  }
]

export const RISK_FEATURE_DEFAULT_WEIGHTS = [38, 29, 18, 15]
