export type UserRole = 'Admin' | 'IT' | 'Employee' | 'Auditor'

export interface AuthUser {
  id: string
  employeeId: string | null
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
