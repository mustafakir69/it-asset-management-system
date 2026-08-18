export const maintenanceTaskStatuses = ['Planlandı', 'Yaklaşıyor', 'Gecikti', 'Tamamlandı', 'İptal Edildi'] as const
export const maintenanceRequestPriorities = ['Düşük', 'Normal', 'Yüksek', 'Kritik'] as const
export const maintenanceRequestStatuses = ['Açık', 'Atandı', 'İşlemde', 'Tamamlandı', 'İptal Edildi'] as const

export type MaintenanceTaskStatus = (typeof maintenanceTaskStatuses)[number]
export type MaintenanceStoredStatus = 'Planlandı' | 'Tamamlandı' | 'İptal Edildi'
export type MaintenanceRequestPriority = (typeof maintenanceRequestPriorities)[number]
export type MaintenanceRequestStatus = (typeof maintenanceRequestStatuses)[number]

export interface MaintenancePlan {
  id: string
  assetId: string
  assetCode: string
  assetName: string
  name: string
  description: string | null
  frequencyDays: number
  startDate: string
  responsibleUserId: string
  responsibleUserName: string
  estimatedDurationMinutes: number
  reminderLeadDays: number
  nextDueAt: string
  isActive: boolean
  createdAt: string
}

export interface MaintenancePlanInput {
  assetId: string
  name: string
  description?: string
  frequencyDays: number
  startDate: string
  responsibleUserId: string
  estimatedDurationMinutes: number
  reminderLeadDays: number
}

export interface MaintenanceTask {
  id: string
  maintenancePlanId: string
  assetId: string
  assetCode: string
  assetName: string
  title: string
  description: string | null
  plannedDate: string
  completedDate: string | null
  status: MaintenanceStoredStatus
  displayStatus: MaintenanceTaskStatus
  responsibleUserId: string
  responsibleUserName: string
  notes: string | null
  completedByUserId: string | null
  completedByName: string | null
  result: string | null
  workNotes: string | null
  cancellationReason: string | null
  createdAt: string
}

export interface MaintenanceTaskCompleteInput { completedDate: string; result: string; workNotes: string }
export interface MaintenanceTaskRescheduleInput { plannedDate: string; workNotes: string }

export interface MaintenanceRequest {
  id: string
  requestNumber: string
  assetId: string
  assetCode: string
  assetName: string
  requestedByEmployeeId: string
  requestedByName: string
  requestedByDepartment: string
  title: string
  description: string
  priority: MaintenanceRequestPriority
  status: MaintenanceRequestStatus
  assignedToUserId: string | null
  assignedToName: string | null
  createdAt: string
  updatedAt: string
  completedAt: string | null
  completedByUserId: string | null
  completedByName: string | null
  result: string | null
  workNotes: string | null
  cancellationReason: string | null
}

export interface MaintenanceRequestInput {
  assetId: string
  title: string
  description: string
  priority: MaintenanceRequestPriority
}

export interface MaintenanceCompleteInput { completedAt: string; result: string; workNotes: string }

export interface SupportRequestActivity {
  id: string
  supportRequestId: string
  activityType: string
  occurredAt: string
  performedByUserId: string
  performedByName: string
  oldValue: string | null
  newValue: string | null
  description: string | null
}
