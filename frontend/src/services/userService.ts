import type { UserRole } from '../types/auth'
import type { CreateUserInput, ItStaffMember, ManagedUser } from '../types/user'
import { apiClient } from './api'
import { getApiErrorMessage } from './apiError'

const roles: UserRole[] = ['Admin', 'IT', 'Employee']

const isRecord = (value: unknown): value is Record<string, unknown> =>
  typeof value === 'object' && value !== null

const isRole = (value: unknown): value is UserRole =>
  typeof value === 'string' && roles.includes(value as UserRole)

const mapUser = (value: unknown): ManagedUser => {
  if (
    !isRecord(value) ||
    typeof value.id !== 'string' ||
    (value.employeeId !== null && typeof value.employeeId !== 'string') ||
    typeof value.username !== 'string' ||
    typeof value.email !== 'string' ||
    !isRole(value.role) ||
    typeof value.roleDisplayName !== 'string' ||
    typeof value.isActive !== 'boolean'
  ) {
    throw new Error('API geçersiz bir kullanıcı kaydı döndürdü.')
  }


  const fullName = typeof value.fullName === 'string' && value.fullName.trim().length > 0
    ? value.fullName.trim()
    : null
  const department = typeof value.department === 'string' && value.department.trim().length > 0
    ? value.department.trim()
    : null

  if (value.employeeId !== null && (fullName === null || department === null)) {
    throw new Error('API çalışan adı veya birim bilgisi eksik bir kullanıcı kaydı döndürdü.')
  }

  return {
    id: value.id,
    employeeId: value.employeeId,
    fullName: fullName ?? value.username,
    department,
    employeeNo:
      typeof value.employeeNo === 'string' && value.employeeNo.trim().length > 0
        ? value.employeeNo.trim()
        : null,
    username: value.username,
    email: value.email,
    role: value.role,
    roleDisplayName: value.roleDisplayName,
    isActive: value.isActive,
    status: value.status === 'Aktif' || value.status === 'Pasif'
      ? value.status
      : value.isActive ? 'Aktif' : 'Pasif',
  }
}

export const userService = {
  async getUsers(): Promise<ManagedUser[]> {
    try {
      const response = await apiClient.get<unknown>('/api/users')
      if (!Array.isArray(response.data)) throw new Error('API kullanıcı listesini beklenen formatta döndürmedi.')
      return response.data.map(mapUser)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Kullanıcılar alınamadı.'))
    }
  },

  async createUser(input: CreateUserInput): Promise<ManagedUser> {
    try {
      const response = await apiClient.post<unknown>('/api/users', input)
      return mapUser(response.data)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Kullanıcı oluşturulamadı.'))
    }
  },

  async getUsernameSuggestion(employeeId: string): Promise<string> {
    try {
      const response = await apiClient.get<unknown>('/api/users/username-suggestion', {
        params: { employeeId },
      })
      if (!isRecord(response.data) || typeof response.data.username !== 'string') {
        throw new Error('API geçersiz bir kullanıcı adı önerisi döndürdü.')
      }
      return response.data.username
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Kullanıcı adı önerisi oluşturulamadı.'))
    }
  },

  async getItStaff(): Promise<ItStaffMember[]> {
    try {
      const response = await apiClient.get<unknown>('/api/users/it-staff')
      if (!Array.isArray(response.data)) throw new Error('IT personeli listesi geçersiz.')
      return response.data.map((value) => {
        if (!isRecord(value) || typeof value.userId !== 'string' || typeof value.employeeId !== 'string' || typeof value.fullName !== 'string' || typeof value.email !== 'string') {
          throw new Error('IT personeli kaydı geçersiz.')
        }
        return { userId: value.userId, employeeId: value.employeeId, fullName: value.fullName, email: value.email }
      })
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'IT personeli listesi alınamadı.'))
    }
  },
}
