import { apiClient, API_NET } from './apiClient'
import type { PredictionInput, PredictionResult } from '../types'

export const predictionService = {
  async predict(input: PredictionInput): Promise<PredictionResult> {
    const rawResult = await apiClient.post<any>(`${API_NET}/api/ai/predict`, input)

    // Normalize response from server format (risk_score 0-100) to client format (0-1.0)
    return {
      risk_score: rawResult.risk_score / 100.0,
      risk_level: rawResult.risk_level,
      contributing_factors: rawResult.contributing_factors,
      is_fallback: rawResult.is_fallback,
      factors: rawResult.is_fallback
        ? ['Failed to read neural network splits. Rule-based estimates are active.']
        : Object.keys(rawResult.contributing_factors).map(k => `${k} contributed significantly to this prediction.`)
    }
  }
}
