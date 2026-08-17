export interface InventoryReportItem {
  assetCode: string
  category: string
  brand: string
  model: string
  serialNumber: string
  status: string
  location: string
  purchaseDate: string
  warrantyEndDate: string
}

export interface AssignmentReportItem {
  assetCode: string
  assetName: string
  employeeName: string
  department: string
  assignedAt: string
  returnedAt: string | null
  status: string
  assignedByName: string
  returnedByName: string | null
}

export interface StockReportItem {
  itemCode: string
  itemName: string
  category: string
  brandModel: string
  unit: string
  currentQuantity: number
  minimumQuantity: number
  isCritical: boolean
  location: string
}

export interface MaintenanceReportItem {
  id: string
  assetCode: string
  assetName: string
  title: string
  recordType: string
  plannedDate: string | null
  completedAt: string | null
  actorName: string | null
  result: string | null
  status: string
}

export interface MaintenanceReportSummary {
  planned: number
  completed: number
  overdue: number
  cancelled: number
  onTimeCompletionRate: number
}

export interface MaintenanceReportResponse {
  summary: MaintenanceReportSummary
  records: MaintenanceReportItem[]
}

export interface InventoryReportFilters { category?: string; status?: string; location?: string }
export interface AssignmentReportFilters { status?: string; department?: string; from?: string; to?: string }
export interface StockReportFilters { category?: string; location?: string; critical?: boolean }
export interface MaintenanceReportFilters { recordType?: string; status?: string }
