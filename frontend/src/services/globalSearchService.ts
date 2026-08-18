import type { GlobalSearchResult } from '../types/globalSearch'
import { apiClient } from './api'
import { getApiErrorMessage } from './apiError'

const isRecord = (value: unknown): value is Record<string, unknown> =>
  typeof value === 'object' && value !== null

const mapResult = (value: unknown): GlobalSearchResult => {
  if (
    !isRecord(value) ||
    typeof value.category !== 'string' ||
    typeof value.title !== 'string' ||
    typeof value.description !== 'string' ||
    typeof value.route !== 'string'
  ) {
    throw new Error('API geçersiz bir arama sonucu döndürdü.')
  }
  return {
    category: value.category,
    title: value.title,
    description: value.description,
    route: value.route,
  }
}

export const globalSearchService = {
  async search(query: string): Promise<GlobalSearchResult[]> {
    try {
      const response = await apiClient.get<unknown>('/api/search', {
        params: { query, limit: 5 },
      })
      if (!Array.isArray(response.data)) {
        throw new Error('API arama sonuçlarını beklenen formatta döndürmedi.')
      }
      return response.data.map(mapResult)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Arama yapılamadı.'))
    }
  },
}
