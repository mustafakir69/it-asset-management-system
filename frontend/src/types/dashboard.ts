import type { StatusType } from '../components/StatusTag/StatusTag'

export type DashboardSummaryKey =
  | 'totalAssets'
  | 'inStockAssets'
  | 'assignedAssets'
  | 'maintenanceAssets'
  | 'expiringWarranties'
  | 'expiringLicenses'
  | 'criticalStockItems'
  | 'overdueMaintenanceTasks'
  | 'openMaintenanceRequests'

export interface DashboardMovement {
  assignmentId: string
  assetId: string
  assetCode: string
  assetName: string
  description: string
  status: StatusType
  occurredAt: string
}

export interface DashboardWarranty {
  assetId: string
  assetCode: string
  assetName: string
  warrantyEndDate: string
  remainingDays: number
  status: 'Yaklaşıyor'
}

export interface DashboardStock {
  stockItemId: string
  itemCode: string
  itemName: string
  currentQuantity: number
  minimumQuantity: number
  unit: string
  location: string
}

export interface DashboardMaintenance {
  taskId: string
  assetCode: string
  assetName: string
  title: string
  plannedDate: string
  status: 'Yaklaşıyor' | 'Gecikmiş'
}

export interface DashboardSummary {
  totalAssets: number
  inStockAssets: number
  assignedAssets: number
  maintenanceAssets: number
  expiringWarranties: number
  expiringLicenses: number
  criticalStockItems: number
  overdueMaintenanceTasks: number
  openMaintenanceRequests: number
  generatedAt: string
  recentMovements: DashboardMovement[]
  upcomingWarranties: DashboardWarranty[]
  criticalStock: DashboardStock[]
  upcomingMaintenance: DashboardMaintenance[]
}

export interface EmployeeDashboardAsset {
  assetId: string
  assetCode: string
  assetName: string
  category: string
  assignedAt: string
}

export interface EmployeeDashboardSupport {
  id: string
  requestNumber: string
  title: string
  status: string
  updatedAt: string
}

export interface EmployeeDashboardSummary {
  activeAssignmentCount: number
  myAssets: EmployeeDashboardAsset[]
  openSupportRequestCount: number
  inProgressSupportRequestCount: number
  recentSupportRequests: EmployeeDashboardSupport[]
  myAssetsWarrantySummary: { active: number; expiringSoon: number; expired: number; unknown: number }
}
