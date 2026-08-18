export const assetCategories = [
  'Dizüstü Bilgisayar',
  'Masaüstü Bilgisayar',
  'Monitör',
  'Telefon',
  'Tablet',
  'Yazıcı',
] as const

export const assetStatuses = [
  'Boşta',
  'Zimmetli',
  'Bakımda',
  'Kayıp',
  'Hurda',
  'Elden Çıkarıldı',
] as const

export const assetLocations = [
  'İstanbul Merkez',
  'İstanbul Depo',
  'Ankara Ofis',
  'İzmir Ofis',
  'Bursa Şube',
] as const

export type AssetCategory = (typeof assetCategories)[number]
export type AssetStatus = (typeof assetStatuses)[number]
export type AssetLocation = (typeof assetLocations)[number]

export const assetScrapReasons = [
  'Ekonomik onarım mümkün değil',
  'Fiziksel hasar',
  'Donanım ömrünü tamamladı',
  'Diğer',
] as const

export const assetDisposalMethods = ['Satış', 'Bağış', 'İmha', 'Diğer'] as const

export type AssetScrapReason = (typeof assetScrapReasons)[number]
export type AssetDisposalMethod = (typeof assetDisposalMethods)[number]

export interface Asset {
  id: string
  assetCode: string
  category: AssetCategory
  brand: string
  model: string
  serialNumber: string
  status: AssetStatus
  location: AssetLocation
  purchaseDate: string
  warrantyEndDate: string | null
  currentAssigneeEmployeeId: string | null
  currentAssigneeName: string | null
  currentAssigneeDepartment: string | null
  currentAssignmentDate: string | null
}

export type AssetInput = Pick<Asset,
  'assetCode' | 'category' | 'brand' | 'model' | 'serialNumber' | 'status' |
  'location' | 'purchaseDate' | 'warrantyEndDate'>

export interface AssetMovement {
  id: string
  assetId: string
  movementType: string
  occurredAt: string
  previousStatus: AssetStatus | null
  newStatus: AssetStatus
  performedByUserId: string
  performedByName: string
  description: string | null
  reason: string | null
  method: string | null
  relatedEntityType: string | null
  relatedEntityId: string | null
}

export interface AssetLostInput {
  lostDate: string
  description: string
}

export interface AssetScrapInput {
  scrappedDate: string
  reason: AssetScrapReason
  description?: string
}

export interface AssetDisposeInput {
  disposedDate: string
  method: AssetDisposalMethod
  description?: string
}
