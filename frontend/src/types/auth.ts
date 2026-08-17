export type UserRole = 'Admin' | 'IT' | 'Employee'

export interface AuthUser {
  id: string
  employeeId: string | null
  fullName: string
  department: string | null
  username: string
  email: string
  role: UserRole
  roleDisplayName: string
}

export interface LoginCredentials {
  identifier: string
  password: string
}

export interface LoginResponse {
  token: string
  expiresAt: string
  user: AuthUser
}
