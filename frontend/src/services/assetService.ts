import axios from 'axios'
import { mockAssets } from '../mocks/assets'
import {
  assetCategories,
  assetLocations,
  assetStatuses,
  type Asset,
  type AssetCategory,
  type AssetInput,
  type AssetLocation,
  type AssetStatus,
} from '../types/asset'
import { apiClient } from './api'

export interface AssetService {
  getAssets: () => Promise<Asset[]>
  getAssetById: (id: string) => Promise<Asset | undefined>
  createAsset: (asset: AssetInput) => Promise<Asset>
  updateAsset: (id: string, asset: AssetInput) => Promise<Asset>
  updateAssetStatus: (id: string, status: AssetStatus) => Promise<Asset>
}

let assignmentAssetStore: Asset[] = mockAssets.map((asset) => ({ ...asset }))

const cloneAsset = (asset: Asset): Asset => ({ ...asset })

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
    typeof warrantyEndDate !== 'string' ||
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

  updateAssetStatus: (id, status) => {
    const asset = assignmentAssetStore.find((current) => current.id === id)

    if (!asset) {
      return Promise.reject(new Error('Durumu güncellenecek cihaz bulunamadı.'))
    }

    const updatedAsset: Asset = { ...asset, status }
    assignmentAssetStore = assignmentAssetStore.map((current) =>
      current.id === id ? updatedAsset : current,
    )

    return Promise.resolve(cloneAsset(updatedAsset))
  },
}
