import type { UserRole } from './auth'

export interface ManagedUser {
  id: string
  employeeId: string | null
  employeeName: string | null
  username: string
  email: string
  role: UserRole
  roleDisplayName: string
  isActive: boolean
}

export interface CreateUserInput {
  employeeId: string | null
  username: string
  email: string
  password: string
  role: UserRole
}
