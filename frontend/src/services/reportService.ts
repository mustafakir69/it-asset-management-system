import type {
  AssignmentReportFilters,
  AssignmentReportItem,
  InventoryReportFilters,
  InventoryReportItem,
  MaintenanceReportFilters,
  MaintenanceReportResponse,
  LicenseReportFilters,
  LicenseReportItem,
  SupportReportFilters,
  SupportReportItem,
  StockReportFilters,
  StockReportItem,
  WarrantyReportFilters,
  WarrantyReportItem,
} from '../types/report'
import { apiClient } from './api'
import { getApiErrorMessage } from './apiError'

type ReportFilters = InventoryReportFilters | AssignmentReportFilters | StockReportFilters |
  MaintenanceReportFilters | WarrantyReportFilters | LicenseReportFilters | SupportReportFilters

const request = async <T>(path: string, params?: ReportFilters): Promise<T> => {
  try {
    return (await apiClient.get<T>(path, { params })).data
  } catch (error: unknown) {
    throw new Error(getApiErrorMessage(error, 'Rapor verileri alınamadı.'))
  }
}

const downloadCsv = async (path: string, fileName: string, params?: ReportFilters): Promise<void> => {
  try {
    const response = await apiClient.get<BlobPart>(path, { params, responseType: 'blob' })
    const url = URL.createObjectURL(new Blob([response.data], { type: 'text/csv;charset=utf-8' }))
    const link = document.createElement('a')
    link.href = url
    link.download = fileName
    document.body.appendChild(link)
    link.click()
    link.remove()
    URL.revokeObjectURL(url)
  } catch (error: unknown) {
    throw new Error(getApiErrorMessage(error, 'CSV dosyası indirilemedi.'))
  }
}

export const reportService = {
  getInventory: (filters: InventoryReportFilters) => request<InventoryReportItem[]>('/api/reports/inventory', filters),
  getAssignments: (filters: AssignmentReportFilters) => request<AssignmentReportItem[]>('/api/reports/assignments', filters),
  getStock: (filters: StockReportFilters) => request<StockReportItem[]>('/api/reports/stock', filters),
  getMaintenance: (filters: MaintenanceReportFilters) => request<MaintenanceReportResponse>('/api/reports/maintenance', filters),
  getWarranties: (filters: WarrantyReportFilters) => request<WarrantyReportItem[]>('/api/reports/warranties', filters),
  getLicenses: (filters: LicenseReportFilters) => request<LicenseReportItem[]>('/api/reports/licenses', filters),
  getSupportRequests: (filters: SupportReportFilters) => request<SupportReportItem[]>('/api/reports/support-requests', filters),
  downloadInventoryCsv: (filters: InventoryReportFilters) => downloadCsv('/api/reports/inventory/csv', 'envanter-raporu.csv', filters),
  downloadAssignmentsCsv: (filters: AssignmentReportFilters) => downloadCsv('/api/reports/assignments/csv', 'zimmet-raporu.csv', filters),
  downloadStockCsv: (filters: StockReportFilters) => downloadCsv('/api/reports/stock/csv', 'stok-raporu.csv', filters),
  downloadMaintenanceCsv: (filters: MaintenanceReportFilters) => downloadCsv('/api/reports/maintenance/csv', 'bakim-raporu.csv', filters),
  downloadWarrantiesCsv: (filters: WarrantyReportFilters) => downloadCsv('/api/reports/warranties/csv', 'garanti-raporu.csv', filters),
  downloadLicensesCsv: (filters: LicenseReportFilters) => downloadCsv('/api/reports/licenses/csv', 'lisans-raporu.csv', filters),
  downloadSupportRequestsCsv: (filters: SupportReportFilters) => downloadCsv('/api/reports/support-requests/csv', 'teknik-destek-raporu.csv', filters),
}
