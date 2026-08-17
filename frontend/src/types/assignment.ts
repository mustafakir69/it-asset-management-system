import type { AssetStatus } from './asset'

export interface Assignment {
  id: string
  assetId: string
  assetCode: string
  assetName: string
  assetCategory: string
  assetBrand: string
  assetModel: string
  assetStatus: AssetStatus
  employeeId: string
  employeeNo: string
  employeeName: string
  department: string
  assignedAt: string
  returnedAt: string | null
  assignedByUserId: string
  assignedByName: string
  returnedByUserId: string | null
  returnedByName: string | null
  notes: string | null
  returnNotes: string | null
  isActive: boolean
}

export interface Employee {
  id: string
  employeeNo: string
  fullName: string
  department: string
  email: string
}

export type AssignmentStatus = 'Aktif' | 'İade Edildi'

export interface CreateAssignmentInput {
  assetId: string
  employeeId: string
  assignedAt: string
  notes: string | null
}

export interface ReturnAssignmentInput {
  returnedAt: string
  returnNotes: string | null
}
