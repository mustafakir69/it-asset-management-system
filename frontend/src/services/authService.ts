import axios from 'axios'
import type { AuthUser, LoginCredentials, LoginResponse, UserRole } from '../types/auth'
import { apiClient } from './api'
import { clearStoredToken, getStoredToken, storeToken } from './authStorage'

const userRoles: UserRole[] = ['Admin', 'IT', 'Employee']

const isRecord = (value: unknown): value is Record<string, unknown> =>
  typeof value === 'object' && value !== null

const isUserRole = (value: unknown): value is UserRole =>
  typeof value === 'string' && userRoles.includes(value as UserRole)

const parseAuthUser = (value: unknown): AuthUser => {
  if (
    !isRecord(value) ||
    typeof value.id !== 'string' ||
    (value.employeeId !== null && typeof value.employeeId !== 'string') ||
    typeof value.username !== 'string' ||
    typeof value.email !== 'string' ||
    !isUserRole(value.role) ||
    typeof value.roleDisplayName !== 'string'
  ) {
    throw new Error('Sunucudan geçersiz kullanıcı bilgisi alındı.')
  }

  const fullName = typeof value.fullName === 'string' && value.fullName.trim().length > 0
    ? value.fullName.trim()
    : null
  const department = typeof value.department === 'string' && value.department.trim().length > 0
    ? value.department.trim()
    : null

  if (value.employeeId !== null && (fullName === null || department === null)) {
    throw new Error('Sunucudan çalışan adı veya birim bilgisi alınamadı.')
  }

  return {
    id: value.id,
    employeeId: value.employeeId,
    fullName: fullName ?? value.username,
    department,
    username: value.username,
    email: value.email,
    role: value.role,
    roleDisplayName: value.roleDisplayName,
  }
}

const getErrorMessage = (error: unknown): string => {
  if (!axios.isAxiosError(error)) {
    return error instanceof Error ? error.message : 'Beklenmeyen bir hata oluştu.'
  }

  if (!error.response) {
    return 'Sunucuya bağlanılamadı. Backend servisinin çalıştığını kontrol edin.'
  }

  const data: unknown = error.response.data
  if (isRecord(data) && typeof data.detail === 'string') return data.detail
  if (error.response.status === 401) return 'Kullanıcı adı/e-posta veya parola hatalı.'
  if (error.response.status === 403) return 'Bu kullanıcı hesabıyla giriş yapılamıyor.'

  return 'Giriş işlemi sırasında bir hata oluştu.'
}

export const authService = {
  async login(credentials: LoginCredentials): Promise<LoginResponse> {
    try {
      const response = await apiClient.post<unknown>('/api/auth/login', credentials)
      if (
        !isRecord(response.data) ||
        typeof response.data.token !== 'string' ||
        typeof response.data.expiresAt !== 'string'
      ) {
        throw new Error('Sunucudan geçersiz giriş yanıtı alındı.')
      }

      const result: LoginResponse = {
        token: response.data.token,
        expiresAt: response.data.expiresAt,
        user: parseAuthUser(response.data.user),
      }
      storeToken(result.token)
      return result
    } catch (error: unknown) {
      throw new Error(getErrorMessage(error))
    }
  },

  async getMe(): Promise<AuthUser> {
    try {
      const response = await apiClient.get<unknown>('/api/auth/me')
      return parseAuthUser(response.data)
    } catch (error: unknown) {
      throw new Error(getErrorMessage(error))
    }
  },

  logout(): void {
    clearStoredToken()
  },

  getToken(): string | null {
    return getStoredToken()
  },
}
