import axios from 'axios'
import {
  licenseStatuses,
  type License,
  type LicenseInput,
  type LicenseAssignment,
  type LicenseAssignmentInput,
  type LicenseStatus,
} from '../types/license'
import { apiClient } from './api'

export interface LicenseService {
  getLicenses: () => Promise<License[]>
  getLicenseById: (id: string) => Promise<License | undefined>
  createLicense: (license: LicenseInput) => Promise<License>
  updateLicense: (id: string, license: LicenseInput) => Promise<License>
  getAssignments: (id: string) => Promise<LicenseAssignment[]>
  getAssetAssignments: (assetId: string) => Promise<LicenseAssignment[]>
  createAssignment: (id: string, input: LicenseAssignmentInput) => Promise<LicenseAssignment>
  revokeAssignment: (licenseId: string, assignmentId: string) => Promise<LicenseAssignment>
}

const isRecord = (value: unknown): value is Record<string, unknown> =>
  typeof value === 'object' && value !== null

const isLicenseStatus = (value: string): value is LicenseStatus =>
  (licenseStatuses as readonly string[]).includes(value)

const mapLicenseResponse = (value: unknown): License => {
  if (!isRecord(value)) {
    throw new Error('API geçersiz bir lisans kaydı döndürdü.')
  }

  const {
    id,
    licenseCode,
    productName,
    vendor,
    licenseType,
    totalSeats,
    usedSeats,
    availableSeats,
    startDate,
    expirationDate,
    isActive,
    notes,
    licenseStatus,
  } = value

  if (
    typeof id !== 'string' ||
    typeof licenseCode !== 'string' ||
    typeof productName !== 'string' ||
    typeof vendor !== 'string' ||
    typeof licenseType !== 'string' ||
    typeof totalSeats !== 'number' ||
    typeof usedSeats !== 'number' ||
    typeof availableSeats !== 'number' ||
    totalSeats < 0 ||
    usedSeats < 0 ||
    availableSeats < 0 ||
    usedSeats > totalSeats ||
    typeof startDate !== 'string' ||
    (expirationDate !== null && typeof expirationDate !== 'string') ||
    typeof isActive !== 'boolean' ||
    (notes !== null && notes !== undefined && typeof notes !== 'string') ||
    typeof licenseStatus !== 'string' ||
    !isLicenseStatus(licenseStatus)
  ) {
    throw new Error('API lisans verileri beklenen formatta değil.')
  }

  return {
    id,
    licenseCode,
    productName,
    vendor,
    licenseType,
    totalSeats,
    usedSeats,
    availableSeats,
    startDate,
    expirationDate,
    isActive,
    notes: typeof notes === 'string' ? notes : undefined,
    licenseStatus,
  }
}

const nullableString = (value: unknown): value is string | null =>
  value === null || typeof value === 'string'

const mapAssignmentResponse = (value: unknown): LicenseAssignment => {
  if (
    !isRecord(value) ||
    typeof value.id !== 'string' ||
    typeof value.licenseId !== 'string' ||
    typeof value.licenseCode !== 'string' ||
    typeof value.productName !== 'string' ||
    typeof value.licenseType !== 'string' ||
    !nullableString(value.employeeId) ||
    !nullableString(value.employeeName) ||
    !nullableString(value.employeeDepartment) ||
    !nullableString(value.assetId) ||
    !nullableString(value.assetCode) ||
    !nullableString(value.assetName) ||
    typeof value.assignedAt !== 'string' ||
    typeof value.assignedByUserId !== 'string' ||
    typeof value.assignedByName !== 'string' ||
    !nullableString(value.revokedAt) ||
    !nullableString(value.revokedByUserId) ||
    !nullableString(value.revokedByName) ||
    (value.status !== 'Aktif' && value.status !== 'Kaldırıldı')
  ) {
    throw new Error('API geçersiz bir lisans ataması döndürdü.')
  }
  return value as unknown as LicenseAssignment
}

const mapAssignmentList = (value: unknown): LicenseAssignment[] => {
  if (!Array.isArray(value)) throw new Error('API lisans atama listesini beklenen formatta döndürmedi.')
  return value.map(mapAssignmentResponse)
}

const getValidationMessage = (value: unknown): string | undefined => {
  if (!isRecord(value)) return undefined

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
    if (validationMessage) return validationMessage
    if (typeof responseData.detail === 'string') return responseData.detail
    if (typeof responseData.message === 'string') return responseData.message
  }

  return fallbackMessage
}

export const licenseService: LicenseService = {
  getLicenses: async () => {
    try {
      const response = await apiClient.get<unknown>('/api/licenses')
      if (!Array.isArray(response.data)) {
        throw new Error('API lisans listesi beklenen formatta değil.')
      }
      return response.data.map(mapLicenseResponse)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Lisans verileri API üzerinden alınamadı.'))
    }
  },

  getLicenseById: async (id) => {
    try {
      const response = await apiClient.get<unknown>(`/api/licenses/${encodeURIComponent(id)}`)
      return mapLicenseResponse(response.data)
    } catch (error: unknown) {
      if (axios.isAxiosError(error) && error.response?.status === 404) return undefined
      throw new Error(getApiErrorMessage(error, 'Lisans bilgileri API üzerinden alınamadı.'))
    }
  },

  createLicense: async (license) => {
    try {
      const response = await apiClient.post<unknown>('/api/licenses', license)
      return mapLicenseResponse(response.data)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Lisans kaydedilemedi.'))
    }
  },

  updateLicense: async (id, license) => {
    try {
      const response = await apiClient.put<unknown>(
        `/api/licenses/${encodeURIComponent(id)}`,
        license,
      )
      return mapLicenseResponse(response.data)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Lisans bilgileri güncellenemedi.'))
    }
  },

  getAssignments: async (id) => {
    try {
      return mapAssignmentList((await apiClient.get<unknown>(`/api/licenses/${encodeURIComponent(id)}/assignments`)).data)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Lisans atamaları alınamadı.'))
    }
  },

  getAssetAssignments: async (assetId) => {
    try {
      return mapAssignmentList((await apiClient.get<unknown>(`/api/licenses/assignments/asset/${encodeURIComponent(assetId)}`)).data)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Cihaza atanmış lisanslar alınamadı.'))
    }
  },

  createAssignment: async (id, input) => {
    try {
      return mapAssignmentResponse((await apiClient.post<unknown>(`/api/licenses/${encodeURIComponent(id)}/assignments`, input)).data)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Lisans atanamadı.'))
    }
  },

  revokeAssignment: async (licenseId, assignmentId) => {
    try {
      return mapAssignmentResponse((await apiClient.put<unknown>(`/api/licenses/${encodeURIComponent(licenseId)}/assignments/${encodeURIComponent(assignmentId)}/revoke`)).data)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Lisans ataması kaldırılamadı.'))
    }
  },
}
