import type { AuditLog, AuditLogFilters } from '../types/auditLog'
import { apiClient } from './api'
import { getApiErrorMessage } from './apiError'

export const auditLogService = {
  async getAuditLogs(filters: AuditLogFilters): Promise<AuditLog[]> {
    try {
      return (await apiClient.get<AuditLog[]>('/api/audit-logs', { params: filters })).data
    } catch (error: unknown) {
      throw new Error(getApiErrorMessage(error, 'Audit kayıtları alınamadı.'))
    }
  },
}
