export type DashboardSummaryKey =
  | 'totalDevices'
  | 'inStockDevices'
  | 'assignedDevices'
  | 'maintenanceDevices'
  | 'expiringWarranties'
  | 'upcomingLicenseRenewals'
  | 'criticalStockItems'
  | 'overdueMaintenanceTasks'

export type DashboardItemStatus =
  | 'Stokta'
  | 'Zimmetli'
  | 'Bakımda'
  | 'Yaklaşıyor'
  | 'Gecikmiş'
  | 'Tamamlandı'

export interface DashboardSummary {
  key: DashboardSummaryKey
  title: string
  value: number
}

export interface DeviceMovement {
  id: string
  assetCode: string
  deviceName: string
  description: string
  occurredAt: string
  status: DashboardItemStatus
}

export interface ExpiringWarranty {
  id: string
  assetCode: string
  deviceName: string
  expiresAt: string
  remainingDays: number
  status: 'Yaklaşıyor'
}

export interface CriticalStockItem {
  id: string
  productName: string
  currentQuantity: number
  minimumQuantity: number
  unit: string
}

export interface MaintenanceTaskSummary {
  id: string
  assetCode: string
  deviceName: string
  taskName: string
  dueDate: string
  status: 'Yaklaşıyor' | 'Gecikmiş'
}
