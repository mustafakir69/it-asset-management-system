export interface AuditLog {
  id: string
  userId: string
  username: string
  entityName: string
  entityId: string
  action: string
  oldValue: string | null
  newValue: string | null
  createdAt: string
}

export interface AuditLogFilters {
  entityName?: string
  action?: string
  username?: string
  from?: string
  to?: string
}
