import type { StatusType } from '../components/StatusTag/StatusTag'
import type {
  DashboardMaintenance,
  DashboardMovement,
  DashboardStock,
  DashboardSummary,
  DashboardWarranty,
  EmployeeDashboardSummary,
} from '../types/dashboard'
import { apiClient } from './api'
import { getApiErrorMessage } from './apiError'

const isRecord = (value: unknown): value is Record<string, unknown> =>
  typeof value === 'object' && value !== null
const stringValue = (value: unknown, name: string): string => {
  if (typeof value !== 'string') throw new Error(`Dashboard ${name} alanı geçersiz.`)
  return value
}
const numberValue = (value: unknown, name: string): number => {
  if (typeof value !== 'number') throw new Error(`Dashboard ${name} alanı geçersiz.`)
  return value
}
const listValue = <T>(value: unknown, mapper: (item: unknown) => T): T[] => {
  if (!Array.isArray(value)) throw new Error('Dashboard liste verisi geçersiz.')
  return value.map(mapper)
}

const movement = (value: unknown): DashboardMovement => {
  if (!isRecord(value)) throw new Error('Dashboard hareket kaydı geçersiz.')
  return {
    assignmentId: stringValue(value.assignmentId, 'assignmentId'),
    assetId: stringValue(value.assetId, 'assetId'),
    assetCode: stringValue(value.assetCode, 'assetCode'),
    assetName: stringValue(value.assetName, 'assetName'),
    description: stringValue(value.description, 'description'),
    status: stringValue(value.status, 'status') as StatusType,
    occurredAt: stringValue(value.occurredAt, 'occurredAt'),
  }
}
const warranty = (value: unknown): DashboardWarranty => {
  if (!isRecord(value)) throw new Error('Dashboard garanti kaydı geçersiz.')
  return {
    assetId: stringValue(value.assetId, 'assetId'), assetCode: stringValue(value.assetCode, 'assetCode'),
    assetName: stringValue(value.assetName, 'assetName'), warrantyEndDate: stringValue(value.warrantyEndDate, 'warrantyEndDate'),
    remainingDays: numberValue(value.remainingDays, 'remainingDays'), status: 'Yaklaşıyor',
  }
}
const stock = (value: unknown): DashboardStock => {
  if (!isRecord(value)) throw new Error('Dashboard stok kaydı geçersiz.')
  return {
    stockItemId: stringValue(value.stockItemId, 'stockItemId'), itemCode: stringValue(value.itemCode, 'itemCode'),
    itemName: stringValue(value.itemName, 'itemName'), currentQuantity: numberValue(value.currentQuantity, 'currentQuantity'),
    minimumQuantity: numberValue(value.minimumQuantity, 'minimumQuantity'), unit: stringValue(value.unit, 'unit'),
    location: stringValue(value.location, 'location'),
  }
}
const maintenance = (value: unknown): DashboardMaintenance => {
  if (!isRecord(value)) throw new Error('Dashboard bakım kaydı geçersiz.')
  const status = stringValue(value.status, 'status')
  if (status !== 'Yaklaşıyor' && status !== 'Gecikmiş') throw new Error('Dashboard bakım durumu geçersiz.')
  return {
    taskId: stringValue(value.taskId, 'taskId'), assetCode: stringValue(value.assetCode, 'assetCode'),
    assetName: stringValue(value.assetName, 'assetName'), title: stringValue(value.title, 'title'),
    plannedDate: stringValue(value.plannedDate, 'plannedDate'), status,
  }
}

const mapSummary = (value: unknown): DashboardSummary => {
  if (!isRecord(value)) throw new Error('Dashboard özeti geçersiz.')
  return {
    totalAssets: numberValue(value.totalAssets, 'totalAssets'), inStockAssets: numberValue(value.inStockAssets, 'inStockAssets'),
    assignedAssets: numberValue(value.assignedAssets, 'assignedAssets'), maintenanceAssets: numberValue(value.maintenanceAssets, 'maintenanceAssets'),
    expiringWarranties: numberValue(value.expiringWarranties, 'expiringWarranties'), expiringLicenses: numberValue(value.expiringLicenses, 'expiringLicenses'),
    criticalStockItems: numberValue(value.criticalStockItems, 'criticalStockItems'), overdueMaintenanceTasks: numberValue(value.overdueMaintenanceTasks, 'overdueMaintenanceTasks'),
    openMaintenanceRequests: numberValue(value.openMaintenanceRequests, 'openMaintenanceRequests'), generatedAt: stringValue(value.generatedAt, 'generatedAt'),
    recentMovements: listValue(value.recentMovements, movement), upcomingWarranties: listValue(value.upcomingWarranties, warranty),
    criticalStock: listValue(value.criticalStock, stock), upcomingMaintenance: listValue(value.upcomingMaintenance, maintenance),
  }
}

const mapEmployeeSummary = (value: unknown): EmployeeDashboardSummary => {
  if (!isRecord(value)) throw new Error('Kişisel dashboard özeti geçersiz.')
  return {
    activeAssignmentCount: numberValue(value.activeAssignmentCount, 'activeAssignmentCount'),
    openSupportRequestCount: numberValue(value.openSupportRequestCount, 'openSupportRequestCount'),
    inProgressSupportRequestCount: numberValue(value.inProgressSupportRequestCount, 'inProgressSupportRequestCount'),
    myAssets: listValue(value.myAssets, (item) => {
      if (!isRecord(item)) throw new Error('Kişisel cihaz kaydı geçersiz.')
      return { assetId: stringValue(item.assetId, 'assetId'), assetCode: stringValue(item.assetCode, 'assetCode'), assetName: stringValue(item.assetName, 'assetName'), category: stringValue(item.category, 'category'), assignedAt: stringValue(item.assignedAt, 'assignedAt') }
    }),
    recentSupportRequests: listValue(value.recentSupportRequests, (item) => {
      if (!isRecord(item)) throw new Error('Destek talebi kaydı geçersiz.')
      return { id: stringValue(item.id, 'id'), requestNumber: stringValue(item.requestNumber, 'requestNumber'), title: stringValue(item.title, 'title'), status: stringValue(item.status, 'status'), updatedAt: stringValue(item.updatedAt, 'updatedAt') }
    }),
    myAssetsWarrantySummary: (() => {
      if (!isRecord(value.myAssetsWarrantySummary)) throw new Error('Garanti özeti geçersiz.')
      const item = value.myAssetsWarrantySummary
      return { active: numberValue(item.active, 'active'), expiringSoon: numberValue(item.expiringSoon, 'expiringSoon'), expired: numberValue(item.expired, 'expired'), unknown: numberValue(item.unknown, 'unknown') }
    })(),
  }
}

export const dashboardService = {
  async getSummary(): Promise<DashboardSummary> {
    try {
      return mapSummary((await apiClient.get<unknown>('/api/dashboard/summary')).data)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Dashboard verileri alınamadı.'))
    }
  },
  async getMySummary(): Promise<EmployeeDashboardSummary> {
    try {
      return mapEmployeeSummary((await apiClient.get<unknown>('/api/dashboard/my-summary')).data)
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Kişisel dashboard verileri alınamadı.'))
    }
  },
}
