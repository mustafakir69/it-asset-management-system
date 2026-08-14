import axios from 'axios'
import {
  maintenanceRequestPriorities,
  maintenanceRequestStatuses,
  maintenanceTaskStatuses,
  type MaintenanceCompleteInput,
  type MaintenancePlan,
  type MaintenancePlanInput,
  type MaintenanceRequest,
  type MaintenanceRequestInput,
  type MaintenanceStoredStatus,
  type MaintenanceTask,
  type MaintenanceTaskCompleteInput,
  type MaintenanceTaskRescheduleInput,
} from '../types/maintenance'
import { apiClient } from './api'

const isRecord = (value: unknown): value is Record<string, unknown> => typeof value === 'object' && value !== null
const isNullableString = (value: unknown): value is string | null => typeof value === 'string' || value === null
const inValues = (value: string, values: readonly string[]) => values.includes(value)
const isStoredStatus = (value: string): value is MaintenanceStoredStatus => inValues(value, ['Planlandı', 'Tamamlandı', 'İptal Edildi'])

const mapPlan = (value: unknown): MaintenancePlan => {
  if (!isRecord(value)) throw new Error('API geçersiz bir bakım planı döndürdü.')
  const { id, assetId, assetCode, assetName, name, description, frequencyDays, startDate, responsibleTechnician, isActive, createdAt } = value
  if (typeof id !== 'string' || typeof assetId !== 'string' || typeof assetCode !== 'string' || typeof assetName !== 'string' || typeof name !== 'string' || !isNullableString(description) || typeof frequencyDays !== 'number' || typeof startDate !== 'string' || typeof responsibleTechnician !== 'string' || typeof isActive !== 'boolean' || typeof createdAt !== 'string') throw new Error('API bakım planı verileri beklenen formatta değil.')
  return { id, assetId, assetCode, assetName, name, description, frequencyDays, startDate, responsibleTechnician, isActive, createdAt }
}

const mapTask = (value: unknown): MaintenanceTask => {
  if (!isRecord(value)) throw new Error('API geçersiz bir bakım görevi döndürdü.')
  const { id, maintenancePlanId, assetId, assetCode, assetName, title, description, plannedDate, completedDate, status, displayStatus, assignedTechnician, notes, completedBy, result, workNotes, cancellationReason, createdAt } = value
  if (typeof id !== 'string' || typeof maintenancePlanId !== 'string' || typeof assetId !== 'string' || typeof assetCode !== 'string' || typeof assetName !== 'string' || typeof title !== 'string' || !isNullableString(description) || typeof plannedDate !== 'string' || !isNullableString(completedDate) || typeof status !== 'string' || !isStoredStatus(status) || typeof displayStatus !== 'string' || !inValues(displayStatus, maintenanceTaskStatuses) || !isNullableString(assignedTechnician) || !isNullableString(notes) || !isNullableString(completedBy) || !isNullableString(result) || !isNullableString(workNotes) || !isNullableString(cancellationReason) || typeof createdAt !== 'string') throw new Error('API bakım görevi verileri beklenen formatta değil.')
  return { id, maintenancePlanId, assetId, assetCode, assetName, title, description, plannedDate, completedDate, status, displayStatus: displayStatus as MaintenanceTask['displayStatus'], assignedTechnician, notes, completedBy, result, workNotes, cancellationReason, createdAt }
}

const mapRequest = (value: unknown): MaintenanceRequest => {
  if (!isRecord(value)) throw new Error('API geçersiz bir bakım talebi döndürdü.')
  const { id, requestNumber, assetId, assetCode, assetName, title, description, priority, status, requestedBy, assignedTechnician, createdAt, updatedAt, completedAt, completedBy, result, workNotes, cancellationReason } = value
  if (typeof id !== 'string' || typeof requestNumber !== 'string' || typeof assetId !== 'string' || typeof assetCode !== 'string' || typeof assetName !== 'string' || typeof title !== 'string' || typeof description !== 'string' || typeof priority !== 'string' || !inValues(priority, maintenanceRequestPriorities) || typeof status !== 'string' || !inValues(status, maintenanceRequestStatuses) || typeof requestedBy !== 'string' || !isNullableString(assignedTechnician) || typeof createdAt !== 'string' || typeof updatedAt !== 'string' || !isNullableString(completedAt) || !isNullableString(completedBy) || !isNullableString(result) || !isNullableString(workNotes) || !isNullableString(cancellationReason)) throw new Error('API bakım talebi verileri beklenen formatta değil.')
  return { id, requestNumber, assetId, assetCode, assetName, title, description, priority: priority as MaintenanceRequest['priority'], status: status as MaintenanceRequest['status'], requestedBy, assignedTechnician, createdAt, updatedAt, completedAt, completedBy, result, workNotes, cancellationReason }
}

const validationMessage = (value: unknown): string | undefined => isRecord(value) ? Object.values(value).flatMap((entry) => Array.isArray(entry) ? entry.filter((item): item is string => typeof item === 'string') : []).join(' ') || undefined : undefined
const apiError = (error: unknown, fallback: string): Error => {
  if (!axios.isAxiosError(error)) return new Error(error instanceof Error ? error.message : fallback)
  if (!error.response) return new Error('Backend servisine ulaşılamadı. Servisin çalıştığını kontrol edip tekrar deneyin.')
  const data: unknown = error.response.data
  if (isRecord(data)) return new Error(validationMessage(data.errors) ?? (typeof data.detail === 'string' ? data.detail : fallback))
  return new Error(fallback)
}

const getList = async <T>(url: string, mapper: (value: unknown) => T, fallback: string): Promise<T[]> => {
  try { const response = await apiClient.get<unknown>(url); if (!Array.isArray(response.data)) throw new Error(fallback); return response.data.map(mapper) }
  catch (error: unknown) { throw apiError(error, fallback) }
}

const getOne = async <T>(url: string, mapper: (value: unknown) => T, fallback: string): Promise<T | undefined> => {
  try { return mapper((await apiClient.get<unknown>(url)).data) }
  catch (error: unknown) { if (axios.isAxiosError(error) && error.response?.status === 404) return undefined; throw apiError(error, fallback) }
}

export const maintenanceService = {
  getPlans: () => getList('/api/maintenance/plans', mapPlan, 'Bakım planları yüklenemedi.'),
  getPlanById: (id: string) => getOne(`/api/maintenance/plans/${encodeURIComponent(id)}`, mapPlan, 'Bakım planı yüklenemedi.'),
  async createPlan(input: MaintenancePlanInput) { try { return mapPlan((await apiClient.post('/api/maintenance/plans', input)).data) } catch (error) { throw apiError(error, 'Bakım planı kaydedilemedi.') } },
  async updatePlan(id: string, input: MaintenancePlanInput) { try { return mapPlan((await apiClient.put(`/api/maintenance/plans/${encodeURIComponent(id)}`, input)).data) } catch (error) { throw apiError(error, 'Bakım planı güncellenemedi.') } },
  async updatePlanStatus(id: string, isActive: boolean) { try { return mapPlan((await apiClient.put(`/api/maintenance/plans/${encodeURIComponent(id)}/status`, { isActive })).data) } catch (error) { throw apiError(error, 'Bakım planı durumu güncellenemedi.') } },
  getTasks: () => getList('/api/maintenance/tasks', mapTask, 'Bakım görevleri yüklenemedi.'),
  getTaskById: (id: string) => getOne(`/api/maintenance/tasks/${encodeURIComponent(id)}`, mapTask, 'Bakım görevi yüklenemedi.'),
  async completeTask(id: string, input: MaintenanceTaskCompleteInput) { try { return mapTask((await apiClient.put(`/api/maintenance/tasks/${encodeURIComponent(id)}/complete`, input)).data) } catch (error) { throw apiError(error, 'Bakım görevi tamamlanamadı.') } },
  async cancelTask(id: string, cancellationReason: string) { try { return mapTask((await apiClient.put(`/api/maintenance/tasks/${encodeURIComponent(id)}/cancel`, { cancellationReason })).data) } catch (error) { throw apiError(error, 'Bakım görevi iptal edilemedi.') } },
  async rescheduleTask(id: string, input: MaintenanceTaskRescheduleInput) { try { return mapTask((await apiClient.put(`/api/maintenance/tasks/${encodeURIComponent(id)}/reschedule`, input)).data) } catch (error) { throw apiError(error, 'Bakım görevi yeniden planlanamadı.') } },
  getRequests: () => getList('/api/maintenance/requests', mapRequest, 'Bakım talepleri yüklenemedi.'),
  getRequestById: (id: string) => getOne(`/api/maintenance/requests/${encodeURIComponent(id)}`, mapRequest, 'Bakım talebi yüklenemedi.'),
  async createRequest(input: MaintenanceRequestInput) { try { return mapRequest((await apiClient.post('/api/maintenance/requests', input)).data) } catch (error) { throw apiError(error, 'Bakım talebi kaydedilemedi.') } },
  async updateRequest(id: string, input: MaintenanceRequestInput) { try { return mapRequest((await apiClient.put(`/api/maintenance/requests/${encodeURIComponent(id)}`, input)).data) } catch (error) { throw apiError(error, 'Bakım talebi güncellenemedi.') } },
  async assignRequest(id: string, assignedTechnician: string) { try { return mapRequest((await apiClient.put(`/api/maintenance/requests/${encodeURIComponent(id)}/assign`, { assignedTechnician })).data) } catch (error) { throw apiError(error, 'Bakım talebi atanamadı.') } },
  async startRequest(id: string) { try { return mapRequest((await apiClient.put(`/api/maintenance/requests/${encodeURIComponent(id)}/start`)).data) } catch (error) { throw apiError(error, 'Bakım talebi işleme alınamadı.') } },
  async completeRequest(id: string, input: MaintenanceCompleteInput) { try { return mapRequest((await apiClient.put(`/api/maintenance/requests/${encodeURIComponent(id)}/complete`, input)).data) } catch (error) { throw apiError(error, 'Bakım talebi tamamlanamadı.') } },
  async cancelRequest(id: string, cancellationReason: string) { try { return mapRequest((await apiClient.put(`/api/maintenance/requests/${encodeURIComponent(id)}/cancel`, { cancellationReason })).data) } catch (error) { throw apiError(error, 'Bakım talebi iptal edilemedi.') } },
}
