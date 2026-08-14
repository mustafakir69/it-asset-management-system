import axios from 'axios'
import { assetStatuses, type Asset, type AssetStatus } from '../types/asset'
import type {
  Assignment,
  CreateAssignmentInput,
  Employee,
  ReturnAssignmentInput,
} from '../types/assignment'
import { apiClient } from './api'
import { getApiErrorMessage } from './apiError'
import { assetService } from './assetService'

export interface AssignmentFilters {
  search?: string
  department?: string
  status?: 'Aktif' | 'İade Edildi'
}

export interface AssignmentService {
  getAssignments: (filters?: AssignmentFilters) => Promise<Assignment[]>
  getActiveAssignments: (filters?: AssignmentFilters) => Promise<Assignment[]>
  getAssignmentHistory: (filters?: AssignmentFilters) => Promise<Assignment[]>
  getMyAssignments: () => Promise<Assignment[]>
  getAssignmentById: (id: string) => Promise<Assignment | undefined>
  getAssignableAssets: () => Promise<Asset[]>
  getEmployees: () => Promise<Employee[]>
  createAssignment: (input: CreateAssignmentInput) => Promise<Assignment>
  returnAssignment: (id: string, input: ReturnAssignmentInput) => Promise<Assignment>
}

const isRecord = (value: unknown): value is Record<string, unknown> =>
  typeof value === 'object' && value !== null

const isNullableString = (value: unknown): value is string | null =>
  value === null || typeof value === 'string'

const isAssetStatus = (value: unknown): value is AssetStatus =>
  typeof value === 'string' && (assetStatuses as readonly string[]).includes(value)

const mapAssignment = (value: unknown): Assignment => {
  if (
    !isRecord(value) ||
    typeof value.id !== 'string' ||
    typeof value.assetId !== 'string' ||
    typeof value.assetCode !== 'string' ||
    typeof value.assetName !== 'string' ||
    typeof value.assetCategory !== 'string' ||
    typeof value.assetBrand !== 'string' ||
    typeof value.assetModel !== 'string' ||
    !isAssetStatus(value.assetStatus) ||
    typeof value.employeeId !== 'string' ||
    typeof value.employeeNo !== 'string' ||
    typeof value.employeeName !== 'string' ||
    typeof value.department !== 'string' ||
    typeof value.assignedAt !== 'string' ||
    !isNullableString(value.returnedAt) ||
    typeof value.assignedBy !== 'string' ||
    !isNullableString(value.returnedBy) ||
    !isNullableString(value.notes) ||
    !isNullableString(value.returnNotes) ||
    typeof value.isActive !== 'boolean'
  ) {
    throw new Error('API geçersiz bir zimmet kaydı döndürdü.')
  }

  return {
    id: value.id,
    assetId: value.assetId,
    assetCode: value.assetCode,
    assetName: value.assetName,
    assetCategory: value.assetCategory,
    assetBrand: value.assetBrand,
    assetModel: value.assetModel,
    assetStatus: value.assetStatus,
    employeeId: value.employeeId,
    employeeNo: value.employeeNo,
    employeeName: value.employeeName,
    department: value.department,
    assignedAt: value.assignedAt,
    returnedAt: value.returnedAt,
    assignedBy: value.assignedBy,
    returnedBy: value.returnedBy,
    notes: value.notes,
    returnNotes: value.returnNotes,
    isActive: value.isActive,
  }
}

const mapEmployee = (value: unknown): Employee => {
  if (
    !isRecord(value) ||
    typeof value.id !== 'string' ||
    typeof value.employeeNo !== 'string' ||
    typeof value.fullName !== 'string' ||
    typeof value.department !== 'string' ||
    typeof value.email !== 'string'
  ) {
    throw new Error('API geçersiz bir çalışan kaydı döndürdü.')
  }

  return {
    id: value.id,
    employeeNo: value.employeeNo,
    fullName: value.fullName,
    department: value.department,
    email: value.email,
  }
}

const mapList = <T>(value: unknown, mapper: (item: unknown) => T, label: string): T[] => {
  if (!Array.isArray(value)) throw new Error(`API ${label} listesini beklenen formatta döndürmedi.`)
  return value.map(mapper)
}

const getAssignmentList = async (path: string, filters?: AssignmentFilters) => {
  try {
    const response = await apiClient.get<unknown>(path, { params: filters })
    return mapList(response.data, mapAssignment, 'zimmet')
  } catch (error: unknown) {
    throw new Error(getApiErrorMessage(error, 'Zimmet kayıtları alınamadı.'))
  }
}

export const assignmentService: AssignmentService = {
  getAssignments: (filters) => getAssignmentList('/api/assignments', filters),
  getActiveAssignments: (filters) => getAssignmentList('/api/assignments', filters),
  getAssignmentHistory: (filters) => getAssignmentList('/api/assignments/history', filters),
  getMyAssignments: () => getAssignmentList('/api/assignments/my'),

  getAssignmentById: async (id) => {
    try {
      const response = await apiClient.get<unknown>(`/api/assignments/${encodeURIComponent(id)}`)
      return mapAssignment(response.data)
    } catch (error: unknown) {
      if (axios.isAxiosError(error) && error.response?.status === 404) return undefined
      throw new Error(getApiErrorMessage(error, 'Zimmet bilgileri alınamadı.'))
    }
  },

  getAssignableAssets: async () => {
    const assets = await assetService.getAssets()
    return assets.filter((asset) => asset.status === 'Stokta')
  },

  getEmployees: async () => {
    try {
      const response = await apiClient.get<unknown>('/api/employees')
      return mapList(response.data, mapEmployee, 'çalışan')
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Çalışanlar alınamadı.'))
    }
  },

  createAssignment: async (input) => {
    try {
      const response = await apiClient.post<unknown>('/api/assignments', input)
      return mapAssignment(response.data)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Zimmet oluşturulamadı.'))
    }
  },

  returnAssignment: async (id, input) => {
    try {
      const response = await apiClient.put<unknown>(
        `/api/assignments/${encodeURIComponent(id)}/return`,
        input,
      )
      return mapAssignment(response.data)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Cihaz iadesi tamamlanamadı.'))
    }
  },
}
