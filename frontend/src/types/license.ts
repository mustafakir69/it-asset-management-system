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
  usedSeats: number
  startDate: string
  expirationDate: string | null
  isActive: boolean
  notes?: string
}
