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
  type SupportRequestActivity,
} from '../types/maintenance'
import { apiClient } from './api'

const isRecord = (value: unknown): value is Record<string, unknown> => typeof value === 'object' && value !== null
const nullableString = (value: unknown): value is string | null => value === null || typeof value === 'string'
const string = (value: unknown, label: string): string => { if (typeof value !== 'string') throw new Error(`${label} alanı geçersiz.`); return value }
const number = (value: unknown, label: string): number => { if (typeof value !== 'number') throw new Error(`${label} alanı geçersiz.`); return value }
const nullable = (value: unknown, label: string): string | null => { if (!nullableString(value)) throw new Error(`${label} alanı geçersiz.`); return value }
const inValues = (value: string, values: readonly string[]) => values.includes(value)
const isStoredStatus = (value: string): value is MaintenanceStoredStatus => inValues(value, ['Planlandı', 'Tamamlandı', 'İptal Edildi'])

const mapPlan = (value: unknown): MaintenancePlan => {
  if (!isRecord(value)) throw new Error('API geçersiz bir bakım planı döndürdü.')
  return {
    id: string(value.id, 'id'), assetId: string(value.assetId, 'assetId'), assetCode: string(value.assetCode, 'assetCode'),
    assetName: string(value.assetName, 'assetName'), name: string(value.name, 'name'), description: nullable(value.description, 'description'),
    frequencyDays: number(value.frequencyDays, 'frequencyDays'), startDate: string(value.startDate, 'startDate'),
    responsibleUserId: string(value.responsibleUserId, 'responsibleUserId'), responsibleUserName: string(value.responsibleUserName, 'responsibleUserName'),
    estimatedDurationMinutes: number(value.estimatedDurationMinutes, 'estimatedDurationMinutes'), reminderLeadDays: number(value.reminderLeadDays, 'reminderLeadDays'),
    nextDueAt: string(value.nextDueAt, 'nextDueAt'), isActive: value.isActive === true, createdAt: string(value.createdAt, 'createdAt'),
  }
}

const mapTask = (value: unknown): MaintenanceTask => {
  if (!isRecord(value)) throw new Error('API geçersiz bir bakım görevi döndürdü.')
  const status = string(value.status, 'status'); const displayStatus = string(value.displayStatus, 'displayStatus')
  if (!isStoredStatus(status) || !inValues(displayStatus, maintenanceTaskStatuses)) throw new Error('Bakım görev durumu geçersiz.')
  return {
    id: string(value.id, 'id'), maintenancePlanId: string(value.maintenancePlanId, 'maintenancePlanId'), assetId: string(value.assetId, 'assetId'),
    assetCode: string(value.assetCode, 'assetCode'), assetName: string(value.assetName, 'assetName'), title: string(value.title, 'title'),
    description: nullable(value.description, 'description'), plannedDate: string(value.plannedDate, 'plannedDate'), completedDate: nullable(value.completedDate, 'completedDate'),
    status, displayStatus: displayStatus as MaintenanceTask['displayStatus'], responsibleUserId: string(value.responsibleUserId, 'responsibleUserId'),
    responsibleUserName: string(value.responsibleUserName, 'responsibleUserName'), notes: nullable(value.notes, 'notes'),
    completedByUserId: nullable(value.completedByUserId, 'completedByUserId'), completedByName: nullable(value.completedByName, 'completedByName'),
    result: nullable(value.result, 'result'), workNotes: nullable(value.workNotes, 'workNotes'), cancellationReason: nullable(value.cancellationReason, 'cancellationReason'),
    createdAt: string(value.createdAt, 'createdAt'),
  }
}

const mapRequest = (value: unknown): MaintenanceRequest => {
  if (!isRecord(value)) throw new Error('API geçersiz bir teknik destek talebi döndürdü.')
  const priority = string(value.priority, 'priority'); const status = string(value.status, 'status')
  if (!inValues(priority, maintenanceRequestPriorities) || !inValues(status, maintenanceRequestStatuses)) throw new Error('Teknik destek durumu geçersiz.')
  return {
    id: string(value.id, 'id'), requestNumber: string(value.requestNumber, 'requestNumber'), assetId: string(value.assetId, 'assetId'),
    assetCode: string(value.assetCode, 'assetCode'), assetName: string(value.assetName, 'assetName'), requestedByEmployeeId: string(value.requestedByEmployeeId, 'requestedByEmployeeId'),
    requestedByName: string(value.requestedByName, 'requestedByName'), requestedByDepartment: string(value.requestedByDepartment, 'requestedByDepartment'),
    title: string(value.title, 'title'), description: string(value.description, 'description'), priority: priority as MaintenanceRequest['priority'], status: status as MaintenanceRequest['status'],
    assignedToUserId: nullable(value.assignedToUserId, 'assignedToUserId'), assignedToName: nullable(value.assignedToName, 'assignedToName'),
    createdAt: string(value.createdAt, 'createdAt'), updatedAt: string(value.updatedAt, 'updatedAt'), completedAt: nullable(value.completedAt, 'completedAt'),
    completedByUserId: nullable(value.completedByUserId, 'completedByUserId'), completedByName: nullable(value.completedByName, 'completedByName'),
    result: nullable(value.result, 'result'), workNotes: nullable(value.workNotes, 'workNotes'), cancellationReason: nullable(value.cancellationReason, 'cancellationReason'),
  }
}

const mapActivity = (value: unknown): SupportRequestActivity => {
  if (!isRecord(value)) throw new Error('API geçersiz bir talep hareketi döndürdü.')
  return {
    id: string(value.id, 'id'),
    supportRequestId: string(value.supportRequestId, 'supportRequestId'),
    activityType: string(value.activityType, 'activityType'),
    occurredAt: string(value.occurredAt, 'occurredAt'),
    performedByUserId: string(value.performedByUserId, 'performedByUserId'),
    performedByName: string(value.performedByName, 'performedByName'),
    oldValue: nullable(value.oldValue, 'oldValue'),
    newValue: nullable(value.newValue, 'newValue'),
    description: nullable(value.description, 'description'),
  }
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
  try { const data: unknown = (await apiClient.get<unknown>(url)).data; if (!Array.isArray(data)) throw new Error(fallback); return data.map(mapper) }
  catch (error: unknown) { throw apiError(error, fallback) }
}
const getOne = async <T>(url: string, mapper: (value: unknown) => T, fallback: string): Promise<T | undefined> => {
  try { return mapper((await apiClient.get<unknown>(url)).data) }
  catch (error: unknown) { if (axios.isAxiosError(error) && error.response?.status === 404) return undefined; throw apiError(error, fallback) }
}

export const maintenanceService = {
  getPlans: () => getList('/api/maintenance/plans', mapPlan, 'Bakım planları yüklenemedi.'),
  async createPlan(input: MaintenancePlanInput) { try { return mapPlan((await apiClient.post('/api/maintenance/plans', input)).data) } catch (error) { throw apiError(error, 'Bakım planı kaydedilemedi.') } },
  getTasks: () => getList('/api/maintenance/tasks', mapTask, 'Bakım görevleri yüklenemedi.'),
  getTasksByAsset: (assetId: string) => getList(`/api/maintenance/tasks?assetId=${encodeURIComponent(assetId)}`, mapTask, 'Cihaz bakım geçmişi yüklenemedi.'),
  getTaskById: (id: string) => getOne(`/api/maintenance/tasks/${encodeURIComponent(id)}`, mapTask, 'Bakım görevi yüklenemedi.'),
  async completeTask(id: string, input: MaintenanceTaskCompleteInput) { try { return mapTask((await apiClient.put(`/api/maintenance/tasks/${encodeURIComponent(id)}/complete`, input)).data) } catch (error) { throw apiError(error, 'Bakım görevi tamamlanamadı.') } },
  async cancelTask(id: string, cancellationReason: string) { try { return mapTask((await apiClient.put(`/api/maintenance/tasks/${encodeURIComponent(id)}/cancel`, { cancellationReason })).data) } catch (error) { throw apiError(error, 'Bakım görevi iptal edilemedi.') } },
  async rescheduleTask(id: string, input: MaintenanceTaskRescheduleInput) { try { return mapTask((await apiClient.put(`/api/maintenance/tasks/${encodeURIComponent(id)}/reschedule`, input)).data) } catch (error) { throw apiError(error, 'Bakım görevi yeniden planlanamadı.') } },
  getRequests: () => getList('/api/support-requests', mapRequest, 'Teknik destek talepleri yüklenemedi.'),
  getMyRequests: () => getList('/api/support-requests/my', mapRequest, 'Teknik destek talepleriniz yüklenemedi.'),
  getRequestById: (id: string) => getOne(`/api/support-requests/${encodeURIComponent(id)}`, mapRequest, 'Teknik destek talebi yüklenemedi.'),
  getRequestActivities: (id: string) => getList(`/api/support-requests/${encodeURIComponent(id)}/activities`, mapActivity, 'Talep hareketleri yüklenemedi.'),
  async createRequest(input: MaintenanceRequestInput) { try { return mapRequest((await apiClient.post('/api/support-requests', input)).data) } catch (error) { throw apiError(error, 'Teknik destek talebi kaydedilemedi.') } },
  async assignRequest(id: string, assignedToUserId: string) { try { return mapRequest((await apiClient.put(`/api/support-requests/${encodeURIComponent(id)}/assign`, { assignedToUserId })).data) } catch (error) { throw apiError(error, 'Teknik destek talebi atanamadı.') } },
  async startRequest(id: string) { try { return mapRequest((await apiClient.put(`/api/support-requests/${encodeURIComponent(id)}/start`)).data) } catch (error) { throw apiError(error, 'Teknik destek talebi işleme alınamadı.') } },
  async completeRequest(id: string, input: MaintenanceCompleteInput) { try { return mapRequest((await apiClient.put(`/api/support-requests/${encodeURIComponent(id)}/complete`, input)).data) } catch (error) { throw apiError(error, 'Teknik destek talebi tamamlanamadı.') } },
  async cancelRequest(id: string, cancellationReason: string) { try { return mapRequest((await apiClient.put(`/api/support-requests/${encodeURIComponent(id)}/cancel`, { cancellationReason })).data) } catch (error) { throw apiError(error, 'Teknik destek talebi iptal edilemedi.') } },
}
