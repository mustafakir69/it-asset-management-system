import { ArrowLeftOutlined, EditOutlined } from '@ant-design/icons'
import { Button, Descriptions, List, Space, Typography } from 'antd'
import type { DescriptionsProps } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ContentCard, EmptyState, ErrorState, LoadingState, PageHeader, StatusTag } from '../../components'
import { maintenanceService } from '../../services/maintenanceService'
import type { MaintenancePlan, MaintenanceTask } from '../../types/maintenance'
import { formatDate } from '../../utils'

function MaintenancePlanDetailPage() {
  const { id } = useParams<{ id: string }>(); const navigate = useNavigate()
  const [plan, setPlan] = useState<MaintenancePlan | null>(null); const [tasks, setTasks] = useState<MaintenanceTask[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState<string | null>(null)
  const load = useCallback(async () => { if (!id) return; setLoading(true); try { const [found, allTasks] = await Promise.all([maintenanceService.getPlanById(id), maintenanceService.getTasks()]); if (!found) throw new Error('Bakım planı bulunamadı.'); setPlan(found); setTasks(allTasks.filter((task) => task.maintenancePlanId === id).sort((a, b) => b.plannedDate.localeCompare(a.plannedDate)).slice(0, 6)) } catch (reason: unknown) { setError(reason instanceof Error ? reason.message : 'Bakım planı yüklenemedi.') } finally { setLoading(false) } }, [id])
  useEffect(() => { void load() }, [load])
  const items: DescriptionsProps['items'] = plan ? [{ key: 'asset', label: 'Cihaz', children: `${plan.assetCode} — ${plan.assetName}` }, { key: 'name', label: 'Plan Adı', children: plan.name }, { key: 'description', label: 'Açıklama', children: plan.description || '—', span: 2 }, { key: 'responsible', label: 'Sorumlu IT Personeli', children: plan.responsibleUserName }, { key: 'frequency', label: 'Bakım Sıklığı', children: `${plan.frequencyDays} gün` }, { key: 'start', label: 'Başlangıç Tarihi', children: formatDate(plan.startDate) }, { key: 'active', label: 'Durum', children: <StatusTag status={plan.isActive ? 'Aktif' : 'Pasif'} /> }] : []
  return <section><PageHeader title={plan?.name ?? 'Bakım Planı Detayı'} description={plan ? `${plan.assetCode} — ${plan.assetName}` : 'Bakım planı bilgileri.'} actions={<Space wrap><Button icon={<ArrowLeftOutlined />} onClick={() => void navigate('/maintenance/plans')}>Planlara Dön</Button>{plan && <Button icon={<EditOutlined />} onClick={() => void navigate(`/maintenance/plans/${plan.id}/edit`)} type="primary">Düzenle</Button>}</Space>} />{loading ? <ContentCard><LoadingState /></ContentCard> : error ? <ContentCard><ErrorState message={error} onRetry={() => void load()} /></ContentCard> : plan ? <Space direction="vertical" size="large" style={{ width: '100%' }}><ContentCard title="Plan Bilgileri"><Descriptions bordered column={{ xs: 1, md: 2 }} items={items} /></ContentCard><ContentCard title="Bu Plana Bağlı Son Görevler">{tasks.length === 0 ? <EmptyState /> : <List dataSource={tasks} renderItem={(task) => <List.Item actions={[<Button key="view" onClick={() => void navigate(`/maintenance/tasks/${task.id}`)} type="link">Görüntüle</Button>]}><List.Item.Meta title={<Space wrap><Typography.Text>{formatDate(task.plannedDate)}</Typography.Text><StatusTag status={task.displayStatus} /></Space>} description={task.title} /></List.Item>} />}</ContentCard></Space> : null}</section>
}
export default MaintenancePlanDetailPage
