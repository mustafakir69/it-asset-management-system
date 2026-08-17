export const assetCategories = [
  'Dizüstü Bilgisayar',
  'Masaüstü Bilgisayar',
  'Monitör',
  'Telefon',
  'Tablet',
  'Yazıcı',
] as const

export const assetStatuses = [
  'Stokta',
  'Zimmetli',
  'Bakımda',
  'Kayıp',
  'Hurda',
  'Elden çıkarıldı',
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
