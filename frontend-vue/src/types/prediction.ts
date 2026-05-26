export interface PredictionInput {
  route_id: number
  carrier: string
  weight: number
  weather_index: number
}

export interface PredictionResult {
  risk_score: number // normalized (0.0 to 1.0)
  risk_level: string
  contributing_factors: Record<string, number>
  is_fallback: boolean
  factors: string[]
}
