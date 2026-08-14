export interface Assignment {
  id: string
  assetId: string
  assetCode: string
  assetBrand: string
  assetModel: string
  employeeId: string
  employeeName: string
  department: string
  assignedAt: string
  assignedBy: string
  returnedAt: string | null
  returnedBy?: string | null
  returnNotes?: string | null
  notes: string | null
}

export type AssignmentStatus = 'Aktif' | 'İade Edildi'

export interface CreateAssignmentInput {
  assetId: string
  employeeName: string
  department: string
  assignedAt: string
  assignedBy: string
  notes: string | null
}

export interface ReturnAssignmentInput {
  returnedAt: string
  returnedBy: string
  returnNotes: string | null
}
