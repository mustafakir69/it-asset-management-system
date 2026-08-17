import axios from 'axios'
import {
  warrantyStatuses,
  type WarrantyAsset,
  type WarrantyStatus,
} from '../types/warranty'
import { apiClient } from './api'

export interface WarrantyService {
  getWarranties: () => Promise<WarrantyAsset[]>
}

const isRecord = (value: unknown): value is Record<string, unknown> =>
  typeof value === 'object' && value !== null

const isWarrantyStatus = (value: string): value is WarrantyStatus =>
  (warrantyStatuses as readonly string[]).includes(value)

const mapWarrantyResponse = (value: unknown): WarrantyAsset => {
  if (!isRecord(value)) {
    throw new Error('API geçersiz bir garanti kaydı döndürdü.')
  }

  const {
    assetId,
    assetCode,
    assetName,
    category,
    brand,
    model,
    serialNumber,
    location,
    purchaseDate,
    warrantyEndDate,
    remainingDays,
    warrantyStatus,
    assetStatus,
    currentAssigneeEmployeeId,
    currentAssigneeName,
    currentAssigneeDepartment,
  } = value

  if (
    typeof assetId !== 'string' ||
    typeof assetCode !== 'string' ||
    typeof assetName !== 'string' ||
    typeof category !== 'string' ||
    typeof brand !== 'string' ||
    typeof model !== 'string' ||
    typeof serialNumber !== 'string' ||
    typeof location !== 'string' ||
    typeof purchaseDate !== 'string' ||
    (warrantyEndDate !== null && typeof warrantyEndDate !== 'string') ||
    (remainingDays !== null && typeof remainingDays !== 'number') ||
    typeof warrantyStatus !== 'string' ||
    !isWarrantyStatus(warrantyStatus)
    || typeof assetStatus !== 'string'
    || (currentAssigneeEmployeeId !== null && typeof currentAssigneeEmployeeId !== 'string')
    || (currentAssigneeName !== null && typeof currentAssigneeName !== 'string')
    || (currentAssigneeDepartment !== null && typeof currentAssigneeDepartment !== 'string')
  ) {
    throw new Error('API garanti verileri beklenen formatta değil.')
  }

  return {
    assetId,
    assetCode,
    assetName,
    category,
    brand,
    model,
    serialNumber,
    location,
    purchaseDate,
    warrantyEndDate,
    remainingDays,
    warrantyStatus,
    assetStatus,
    currentAssigneeEmployeeId,
    currentAssigneeName,
    currentAssigneeDepartment,
  }
}

export const warrantyService: WarrantyService = {
  getWarranties: async () => {
    try {
      const response = await apiClient.get<unknown>('/api/warranties')

      if (!Array.isArray(response.data)) {
        throw new Error('API garanti listesi beklenen formatta değil.')
      }

      return response.data.map(mapWarrantyResponse)
    } catch (error: unknown) {
      if (axios.isAxiosError(error) && !error.response) {
        throw new Error(
          'Backend servisine ulaşılamadı. Servisin çalıştığını kontrol edip tekrar deneyin.',
        )
      }

      if (error instanceof Error) {
        throw error
      }

      throw new Error('Garanti verileri API üzerinden alınamadı.')
    }
  },
}
