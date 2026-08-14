export const warrantyStatuses = [
  'Aktif',
  'Yaklaşıyor',
  'Süresi Doldu',
  'Garanti Bilgisi Yok',
] as const

export type WarrantyStatus = (typeof warrantyStatuses)[number]

export interface WarrantyAsset {
  assetId: string
  assetCode: string
  category: string
  brand: string
  model: string
  serialNumber: string
  location: string
  purchaseDate: string
  warrantyEndDate: string | null
  remainingDays: number | null
  warrantyStatus: WarrantyStatus
}
