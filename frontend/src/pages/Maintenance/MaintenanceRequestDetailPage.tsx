import { ArrowLeftOutlined } from '@ant-design/icons'
import { Button, Descriptions, Space } from 'antd'
import type { DescriptionsProps } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ContentCard, ErrorState, LoadingState, MaintenanceRequestActions, PageHeader, StatusTag } from '../../components'
import { maintenanceService } from '../../services/maintenanceService'
import type { MaintenanceRequest } from '../../types/maintenance'
import { formatDate } from '../../utils'

function MaintenanceRequestDetailPage() {
  const { id } = useParams<{ id: string }>(); const navigate = useNavigate(); const [request, setRequest] = useState<MaintenanceRequest | null>(null); const [loading, setLoading] = useState(true); const [error, setError] = useState<string | null>(null)
  const load = useCallback(async () => { if (!id) return; setLoading(true); setError(null); try { const found = await maintenanceService.getRequestById(id); if (!found) throw new Error('Teknik destek talebi bulunamadı.'); setRequest(found) } catch (reason: unknown) { setError(reason instanceof Error ? reason.message : 'Teknik destek talebi yüklenemedi.') } finally { setLoading(false) } }, [id])
  useEffect(() => { void load() }, [load])
  const items: DescriptionsProps['items'] = request ? [
    { key: 'number', label: 'Talep No', children: request.requestNumber }, { key: 'asset', label: 'Cihaz', children: `${request.assetCode} — ${request.assetName}` },
    { key: 'title', label: 'Konu', children: request.title }, { key: 'priority', label: 'Öncelik', children: <StatusTag status={request.priority} /> },
    { key: 'status', label: 'Durum', children: <StatusTag status={request.status} /> }, { key: 'requestedBy', label: 'Talebi Açan', children: `${request.requestedByName} · ${request.requestedByDepartment}` },
    { key: 'assigned', label: 'Atanan IT', children: request.assignedToName || '—' }, { key: 'created', label: 'Oluşturulma Tarihi', children: formatDate(request.createdAt) },
    { key: 'updated', label: 'Güncellenme Tarihi', children: formatDate(request.updatedAt) }, { key: 'completed', label: 'Tamamlanma Tarihi', children: formatDate(request.completedAt) },
    { key: 'completedBy', label: 'Çözen IT Personeli', children: request.completedByName || '—' }, { key: 'description', label: 'Açıklama', children: request.description, span: 2 },
    { key: 'result', label: 'Çözüm', children: request.result || '—', span: 2 }, { key: 'workNotes', label: 'Çalışma Notları', children: request.workNotes || '—', span: 2 },
    { key: 'cancel', label: 'İptal Nedeni', children: request.cancellationReason || '—', span: 2 },
  ] : []
  return <section><PageHeader title={request?.title ?? 'Teknik Destek Detayı'} description={request ? `${request.requestNumber} · ${request.assetCode}` : 'Teknik destek talebi bilgileri.'} actions={<Space wrap><Button icon={<ArrowLeftOutlined />} onClick={() => void navigate('/support-requests')}>Teknik Desteğe Dön</Button>{request && <MaintenanceRequestActions mode="buttons" onSuccess={() => void load()} request={request} />}</Space>} /><ContentCard>{loading ? <LoadingState /> : error ? <ErrorState message={error} onRetry={() => void load()} /> : request ? <Descriptions bordered column={{ xs: 1, md: 2 }} items={items} /> : null}</ContentCard></section>
}
export default MaintenanceRequestDetailPage
