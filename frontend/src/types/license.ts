export const licenseStatuses = ['Aktif', 'Yaklaşıyor', 'Süresi Doldu', 'Pasif'] as const

export type LicenseStatus = (typeof licenseStatuses)[number]

export interface License {
  id: string
  licenseCode: string
  productName: string
  vendor: string
  licenseType: string
  totalSeats: number
  usedSeats: number
  availableSeats: number
  startDate: string
  expirationDate: string | null
  isActive: boolean
  notes?: string
  licenseStatus: LicenseStatus
}

export interface LicenseInput {
  licenseCode: string
  productName: string
  vendor: string
  licenseType: string
  totalSeats: number
  startDate: string
  expirationDate: string | null
  isActive: boolean
  notes?: string
}

export interface LicenseAssignment {
  id: string
  licenseId: string
  licenseCode: string
  productName: string
  licenseType: string
  employeeId: string | null
  employeeName: string | null
  employeeDepartment: string | null
  assetId: string | null
  assetCode: string | null
  assetName: string | null
  assignedAt: string
  assignedByUserId: string
  assignedByName: string
  revokedAt: string | null
  revokedByUserId: string | null
  revokedByName: string | null
  status: 'Aktif' | 'Kaldırıldı'
}

export interface LicenseAssignmentInput {
  employeeId?: string
  assetId?: string
  assignedAt: string
}
