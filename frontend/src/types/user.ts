import type { UserRole } from './auth'

export interface ManagedUser {
  id: string
  employeeId: string | null
  fullName: string
  department: string | null
  employeeNo: string | null
  username: string
  email: string
  role: UserRole
  roleDisplayName: string
  isActive: boolean
  status: 'Aktif' | 'Pasif'
}

export interface CreateUserInput {
  employeeId: string | null
  username: string
  email: string
  password: string
  role: UserRole
}

export interface ItStaffMember {
  userId: string
  employeeId: string
  fullName: string
  email: string
}
