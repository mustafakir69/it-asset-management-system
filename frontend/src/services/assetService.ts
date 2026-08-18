import axios from 'axios'
import {
  assetCategories,
  assetLocations,
  assetStatuses,
  type Asset,
  type AssetCategory,
  type AssetDisposeInput,
  type AssetInput,
  type AssetLostInput,
  type AssetLocation,
  type AssetMovement,
  type AssetScrapInput,
  type AssetStatus,
} from '../types/asset'
import { apiClient } from './api'

export interface AssetService {
  getAssets: () => Promise<Asset[]>
  getAssetById: (id: string) => Promise<Asset | undefined>
  createAsset: (asset: AssetInput) => Promise<Asset>
  updateAsset: (id: string, asset: AssetInput) => Promise<Asset>
  getAssetMovements: (id: string) => Promise<AssetMovement[]>
  markAssetLost: (id: string, input: AssetLostInput) => Promise<AssetMovement>
  scrapAsset: (id: string, input: AssetScrapInput) => Promise<AssetMovement>
  disposeAsset: (id: string, input: AssetDisposeInput) => Promise<AssetMovement>
}

const isRecord = (value: unknown): value is Record<string, unknown> =>
  typeof value === 'object' && value !== null

const isAssetCategory = (value: string): value is AssetCategory =>
  (assetCategories as readonly string[]).includes(value)

const isAssetStatus = (value: string): value is AssetStatus =>
  (assetStatuses as readonly string[]).includes(value)

const isAssetLocation = (value: string): value is AssetLocation =>
  (assetLocations as readonly string[]).includes(value)

const mapAssetResponse = (value: unknown): Asset => {
  if (!isRecord(value)) {
    throw new Error('API geçersiz bir envanter kaydı döndürdü.')
  }

  const {
    id,
    assetCode,
    category,
    brand,
    model,
    serialNumber,
    status,
    location,
    purchaseDate,
    warrantyEndDate,
    currentAssigneeEmployeeId,
    currentAssigneeName,
    currentAssigneeDepartment,
    currentAssignmentDate,
  } = value

  if (
    typeof id !== 'string' ||
    typeof assetCode !== 'string' ||
    typeof category !== 'string' ||
    typeof brand !== 'string' ||
    typeof model !== 'string' ||
    typeof serialNumber !== 'string' ||
    typeof status !== 'string' ||
    typeof location !== 'string' ||
    typeof purchaseDate !== 'string' ||
    (warrantyEndDate !== null && typeof warrantyEndDate !== 'string') ||
    (currentAssigneeEmployeeId !== null && typeof currentAssigneeEmployeeId !== 'string') ||
    (currentAssigneeName !== null && typeof currentAssigneeName !== 'string') ||
    (currentAssigneeDepartment !== null && typeof currentAssigneeDepartment !== 'string') ||
    (currentAssignmentDate !== null && typeof currentAssignmentDate !== 'string') ||
    !isAssetCategory(category) ||
    !isAssetStatus(status) ||
    !isAssetLocation(location)
  ) {
    throw new Error('API envanter verileri beklenen formatta değil.')
  }

  return {
    id,
    assetCode,
    category,
    brand,
    model,
    serialNumber,
    status,
    location,
    purchaseDate,
    warrantyEndDate,
    currentAssigneeEmployeeId,
    currentAssigneeName,
    currentAssigneeDepartment,
    currentAssignmentDate,
  }
}

const mapAssetMovementResponse = (value: unknown): AssetMovement => {
  if (!isRecord(value)) {
    throw new Error('API geçersiz bir cihaz hareketi döndürdü.')
  }

  const {
    id,
    assetId,
    movementType,
    occurredAt,
    previousStatus,
    newStatus,
    performedByUserId,
    performedByName,
    description,
    reason,
    method,
    relatedEntityType,
    relatedEntityId,
  } = value

  if (
    typeof id !== 'string' ||
    typeof assetId !== 'string' ||
    typeof movementType !== 'string' ||
    typeof occurredAt !== 'string' ||
    (previousStatus !== null && (typeof previousStatus !== 'string' || !isAssetStatus(previousStatus))) ||
    typeof newStatus !== 'string' ||
    !isAssetStatus(newStatus) ||
    typeof performedByUserId !== 'string' ||
    typeof performedByName !== 'string' ||
    (description !== null && typeof description !== 'string') ||
    (reason !== null && typeof reason !== 'string') ||
    (method !== null && typeof method !== 'string') ||
    (relatedEntityType !== null && typeof relatedEntityType !== 'string') ||
    (relatedEntityId !== null && typeof relatedEntityId !== 'string')
  ) {
    throw new Error('API cihaz hareketi beklenen formatta değil.')
  }

  return {
    id,
    assetId,
    movementType,
    occurredAt,
    previousStatus,
    newStatus,
    performedByUserId,
    performedByName,
    description,
    reason,
    method,
    relatedEntityType,
    relatedEntityId,
  }
}

const getValidationMessage = (value: unknown): string | undefined => {
  if (!isRecord(value)) {
    return undefined
  }

  const messages = Object.values(value).flatMap((entry) =>
    Array.isArray(entry)
      ? entry.filter((message): message is string => typeof message === 'string')
      : [],
  )

  return messages.length > 0 ? messages.join(' ') : undefined
}

const getApiErrorMessage = (error: unknown, fallbackMessage: string): string => {
  if (!axios.isAxiosError(error)) {
    return error instanceof Error ? error.message : fallbackMessage
  }

  if (!error.response) {
    return 'Backend servisine ulaşılamadı. Servisin çalıştığını kontrol edip tekrar deneyin.'
  }

  const responseData: unknown = error.response.data

  if (isRecord(responseData)) {
    const validationMessage = getValidationMessage(responseData.errors)

    if (validationMessage) {
      return validationMessage
    }

    if (typeof responseData.detail === 'string') {
      return responseData.detail
    }

    if (typeof responseData.message === 'string') {
      return responseData.message
    }
  }

  return fallbackMessage
}

const getAssetsFromApi = async (): Promise<Asset[]> => {
  try {
    const response = await apiClient.get<unknown>('/api/assets')

    if (!Array.isArray(response.data)) {
      throw new Error('API envanter listesi beklenen formatta değil.')
    }

    return response.data.map(mapAssetResponse)
  } catch (error: unknown) {
    throw new Error(getApiErrorMessage(error, 'Envanter verileri API üzerinden alınamadı.'))
  }
}

export const assetService: AssetService = {
  getAssets: getAssetsFromApi,

  getAssetById: async (id) => {
    try {
      const response = await apiClient.get<unknown>(`/api/assets/${encodeURIComponent(id)}`)
      return mapAssetResponse(response.data)
    } catch (error: unknown) {
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        return undefined
      }

      throw new Error(getApiErrorMessage(error, 'Cihaz bilgileri API üzerinden alınamadı.'))
    }
  },

  createAsset: async (assetInput) => {
    try {
      const response = await apiClient.post<unknown>('/api/assets', assetInput)
      return mapAssetResponse(response.data)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Cihaz kaydedilemedi.'))
    }
  },

  updateAsset: async (id, assetInput) => {
    try {
      const response = await apiClient.put<unknown>(
        `/api/assets/${encodeURIComponent(id)}`,
        assetInput,
      )
      return mapAssetResponse(response.data)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Cihaz bilgileri güncellenemedi.'))
    }
  },

  getAssetMovements: async (id) => {
    try {
      const response = await apiClient.get<unknown>(
        `/api/assets/${encodeURIComponent(id)}/movements`,
      )
      if (!Array.isArray(response.data)) {
        throw new Error('API cihaz hareket listesi beklenen formatta değil.')
      }
      return response.data.map(mapAssetMovementResponse)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Cihaz hareket geçmişi alınamadı.'))
    }
  },

  markAssetLost: async (id, input) => {
    try {
      const response = await apiClient.post<unknown>(
        `/api/assets/${encodeURIComponent(id)}/mark-lost`,
        input,
      )
      return mapAssetMovementResponse(response.data)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Cihaz kayıp olarak işaretlenemedi.'))
    }
  },

  scrapAsset: async (id, input) => {
    try {
      const response = await apiClient.post<unknown>(
        `/api/assets/${encodeURIComponent(id)}/scrap`,
        input,
      )
      return mapAssetMovementResponse(response.data)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Cihaz hurdaya ayrılamadı.'))
    }
  },

  disposeAsset: async (id, input) => {
    try {
      const response = await apiClient.post<unknown>(
        `/api/assets/${encodeURIComponent(id)}/dispose`,
        input,
      )
      return mapAssetMovementResponse(response.data)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Cihaz elden çıkarılamadı.'))
    }
  },
}
