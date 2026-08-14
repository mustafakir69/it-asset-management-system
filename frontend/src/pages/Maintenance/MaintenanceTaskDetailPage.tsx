import { ArrowLeftOutlined } from '@ant-design/icons'
import { Button, Descriptions, Space } from 'antd'
import type { DescriptionsProps } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ContentCard, ErrorState, LoadingState, MaintenanceTaskActions, PageHeader, StatusTag } from '../../components'
import { maintenanceService } from '../../services/maintenanceService'
import type { MaintenanceTask } from '../../types/maintenance'
import { formatDate } from '../../utils'

function MaintenanceTaskDetailPage() {
  const { taskId } = useParams<{ taskId: string }>()
  const navigate = useNavigate()
  const [task, setTask] = useState<MaintenanceTask | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)

  const loadTask = useCallback(async () => {
    if (!taskId) {
      setLoadError('Görüntülenecek bakım görevi belirtilmedi.')
      setIsLoading(false)
      return
    }
    setIsLoading(true)
    setLoadError(null)
    try {
      const result = await maintenanceService.getTaskById(taskId)
      if (!result) {
        setLoadError('Aradığınız bakım görevi bulunamadı.')
        return
      }
      setTask(result)
    } catch (error: unknown) {
      setLoadError(error instanceof Error ? error.message : 'Bakım görevi yüklenemedi.')
    } finally {
      setIsLoading(false)
    }
  }, [taskId])

  useEffect(() => { void loadTask() }, [loadTask])

  const items: DescriptionsProps['items'] = task ? [
    { key: 'assetCode', label: 'Cihaz Kodu', children: task.assetCode },
    { key: 'assetName', label: 'Cihaz', children: task.assetName },
    { key: 'title', label: 'Bakım', children: task.title },
    { key: 'plannedDate', label: 'Planlanan Tarih', children: formatDate(task.plannedDate) },
    { key: 'completedDate', label: 'Tamamlanma Tarihi', children: formatDate(task.completedDate) },
    { key: 'technicianName', label: 'Sorumlu Teknisyen', children: task.assignedTechnician || '—' },
    { key: 'status', label: 'Durum', children: <StatusTag status={task.displayStatus} /> },
    { key: 'completedBy', label: 'Bakımı Yapan', children: task.completedBy || '—' },
    { key: 'result', label: 'Sonuç', children: task.result || '—' },
    { key: 'workNotes', label: 'İşlem Notu', children: task.workNotes || '—', span: 2 },
    { key: 'cancellationReason', label: 'İptal Nedeni', children: task.cancellationReason || '—', span: 2 },
    { key: 'description', label: 'Açıklama', children: task.description || '—', span: 2 },
    { key: 'notes', label: 'Notlar', children: task.notes || '—', span: 2 },
  ] : []

  return (
    <section>
      <PageHeader title={task?.title ?? 'Bakım Görevi Detayı'} description={task ? `${task.assetCode} — ${task.assetName}` : 'Bakım görevinin temel bilgileri.'} actions={<Space wrap><Button icon={<ArrowLeftOutlined />} onClick={() => void navigate('/maintenance/tasks')}>Bakım Görevlerine Dön</Button>{task && <MaintenanceTaskActions mode="buttons" onSuccess={() => void loadTask()} task={task} />}</Space>} />
      <ContentCard title="Görev Bilgileri">
        {isLoading ? <LoadingState message="Bakım görevi yükleniyor..." /> : loadError ? <ErrorState message={loadError} onRetry={() => void loadTask()} /> : task ? <Descriptions bordered column={{ xs: 1, sm: 1, md: 2 }} items={items} size="middle" /> : null}
      </ContentCard>
    </section>
  )
}

export default MaintenanceTaskDetailPage
