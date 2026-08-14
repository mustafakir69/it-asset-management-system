import { mockAssets } from '../mocks/assets'
import type { Asset, AssetInput, AssetStatus } from '../types/asset'

export interface AssetService {
  getAssets: () => Promise<Asset[]>
  getAssetById: (id: string) => Promise<Asset | undefined>
  createAsset: (asset: AssetInput) => Promise<Asset>
  updateAsset: (id: string, asset: AssetInput) => Promise<Asset>
  updateAssetStatus: (id: string, status: AssetStatus) => Promise<Asset>
}

let assetStore: Asset[] = mockAssets.map((asset) => ({ ...asset }))
let nextAssetSequence = assetStore.length + 1

const cloneAsset = (asset: Asset): Asset => ({ ...asset })

const findDuplicate = (asset: AssetInput, excludedId?: string) =>
  assetStore.find(
    (current) =>
      current.id !== excludedId &&
      (current.assetCode.toLocaleLowerCase('tr-TR') ===
        asset.assetCode.toLocaleLowerCase('tr-TR') ||
        current.serialNumber.toLocaleLowerCase('tr-TR') ===
          asset.serialNumber.toLocaleLowerCase('tr-TR')),
  )

export const assetService: AssetService = {
  getAssets: () => Promise.resolve(assetStore.map(cloneAsset)),

  getAssetById: (id) => {
    const asset = assetStore.find((current) => current.id === id)
    return Promise.resolve(asset ? cloneAsset(asset) : undefined)
  },

  createAsset: (assetInput) => {
    if (findDuplicate(assetInput)) {
      return Promise.reject(new Error('Varlık kodu veya seri numarası başka bir cihazda kullanılıyor.'))
    }

    const createdAsset: Asset = {
      id: `asset-${String(nextAssetSequence).padStart(3, '0')}`,
      ...assetInput,
    }

    nextAssetSequence += 1
    assetStore = [createdAsset, ...assetStore]

    return Promise.resolve(cloneAsset(createdAsset))
  },

  updateAsset: (id, assetInput) => {
    const assetIndex = assetStore.findIndex((current) => current.id === id)

    if (assetIndex === -1) {
      return Promise.reject(new Error('Düzenlenecek cihaz bulunamadı.'))
    }

    if (findDuplicate(assetInput, id)) {
      return Promise.reject(new Error('Varlık kodu veya seri numarası başka bir cihazda kullanılıyor.'))
    }

    const updatedAsset: Asset = { id, ...assetInput }
    assetStore = assetStore.map((current) => (current.id === id ? updatedAsset : current))

    return Promise.resolve(cloneAsset(updatedAsset))
  },

  updateAssetStatus: (id, status) => {
    const asset = assetStore.find((current) => current.id === id)

    if (!asset) {
      return Promise.reject(new Error('Durumu güncellenecek cihaz bulunamadı.'))
    }

    const updatedAsset: Asset = { ...asset, status }
    assetStore = assetStore.map((current) => (current.id === id ? updatedAsset : current))

    return Promise.resolve(cloneAsset(updatedAsset))
  },
}
