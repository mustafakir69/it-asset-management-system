import axios from 'axios'
import type {
  StockItem,
  StockItemInput,
  StockTransaction,
  StockTransactionInput,
  StockTransactionListItem,
  StockTransactionType,
} from '../types/stockItem'
import { apiClient } from './api'

export interface StockService {
  getStockItems: () => Promise<StockItem[]>
  getStockItemById: (id: string) => Promise<StockItem | undefined>
  createStockItem: (stockItem: StockItemInput) => Promise<StockItem>
  createStockTransaction: (
    stockItemId: string,
    transaction: StockTransactionInput,
  ) => Promise<StockTransaction>
  getStockTransactions: (stockItemId: string) => Promise<StockTransaction[]>
  getAllStockTransactions: () => Promise<StockTransactionListItem[]>
}

const isRecord = (value: unknown): value is Record<string, unknown> =>
  typeof value === 'object' && value !== null

const isTransactionType = (value: string): value is StockTransactionType =>
  value === 'Giriş' || value === 'Çıkış'

const mapStockItemResponse = (value: unknown): StockItem => {
  if (!isRecord(value)) {
    throw new Error('API geçersiz bir stok kaydı döndürdü.')
  }

  const {
    id,
    itemCode,
    name,
    category,
    brandModel,
    unit,
    currentQuantity,
    minimumQuantity,
    location,
    isActive,
    isCritical,
  } = value

  if (
    typeof id !== 'string' ||
    typeof itemCode !== 'string' ||
    typeof name !== 'string' ||
    typeof category !== 'string' ||
    typeof brandModel !== 'string' ||
    typeof unit !== 'string' ||
    typeof currentQuantity !== 'number' ||
    typeof minimumQuantity !== 'number' ||
    typeof location !== 'string' ||
    typeof isActive !== 'boolean' ||
    typeof isCritical !== 'boolean' ||
    currentQuantity < 0 ||
    minimumQuantity < 0
  ) {
    throw new Error('API stok verileri beklenen formatta değil.')
  }

  return {
    id,
    itemCode,
    name,
    category,
    brandModel,
    unit,
    currentQuantity,
    minimumQuantity,
    location,
    isActive,
    isCritical,
  }
}

const mapStockTransactionResponse = (value: unknown): StockTransaction => {
  if (!isRecord(value)) {
    throw new Error('API geçersiz bir stok hareketi döndürdü.')
  }

  const { id, stockItemId, transactionType, quantity, transactionDate, personName, note } = value

  if (
    typeof id !== 'string' ||
    typeof stockItemId !== 'string' ||
    typeof transactionType !== 'string' ||
    !isTransactionType(transactionType) ||
    typeof quantity !== 'number' ||
    quantity <= 0 ||
    typeof transactionDate !== 'string' ||
    typeof personName !== 'string' ||
    (note !== null && note !== undefined && typeof note !== 'string')
  ) {
    throw new Error('API stok hareketi verileri beklenen formatta değil.')
  }

  return {
    id,
    stockItemId,
    transactionType,
    quantity,
    transactionDate,
    personName,
    note: typeof note === 'string' ? note : undefined,
  }
}

const mapStockTransactionListResponse = (value: unknown): StockTransactionListItem => {
  const transaction = mapStockTransactionResponse(value)

  if (!isRecord(value) || typeof value.itemCode !== 'string' || typeof value.itemName !== 'string') {
    throw new Error('API stok hareket listesi verilerini beklenen formatta döndürmedi.')
  }

  return {
    ...transaction,
    itemCode: value.itemCode,
    itemName: value.itemName,
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

export const stockService: StockService = {
  getStockItems: async () => {
    try {
      const response = await apiClient.get<unknown>('/api/stock-items')

      if (!Array.isArray(response.data)) {
        throw new Error('API stok listesi beklenen formatta değil.')
      }

      return response.data.map(mapStockItemResponse)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Stok verileri API üzerinden alınamadı.'))
    }
  },

  getStockItemById: async (id) => {
    try {
      const response = await apiClient.get<unknown>(`/api/stock-items/${encodeURIComponent(id)}`)
      return mapStockItemResponse(response.data)
    } catch (error: unknown) {
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        return undefined
      }

      throw new Error(getApiErrorMessage(error, 'Stok ürünü bilgileri alınamadı.'))
    }
  },

  createStockItem: async (stockItem) => {
    try {
      const response = await apiClient.post<unknown>('/api/stock-items', stockItem)
      return mapStockItemResponse(response.data)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Stok ürünü kaydedilemedi.'))
    }
  },

  createStockTransaction: async (stockItemId, transaction) => {
    try {
      const response = await apiClient.post<unknown>(
        `/api/stock-items/${encodeURIComponent(stockItemId)}/transactions`,
        transaction,
      )
      return mapStockTransactionResponse(response.data)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Stok hareketi kaydedilemedi.'))
    }
  },

  getStockTransactions: async (stockItemId) => {
    try {
      const response = await apiClient.get<unknown>(
        `/api/stock-items/${encodeURIComponent(stockItemId)}/transactions`,
      )

      if (!Array.isArray(response.data)) {
        throw new Error('API stok hareket listesi beklenen formatta değil.')
      }

      return response.data.map(mapStockTransactionResponse)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Stok hareketleri alınamadı.'))
    }
  },

  getAllStockTransactions: async () => {
    try {
      const response = await apiClient.get<unknown>('/api/stock-transactions')

      if (!Array.isArray(response.data)) {
        throw new Error('API stok hareket listesi beklenen formatta değil.')
      }

      return response.data.map(mapStockTransactionListResponse)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Stok hareketleri alınamadı.'))
    }
  },
}
