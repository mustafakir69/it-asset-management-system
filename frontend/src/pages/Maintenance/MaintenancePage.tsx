import { PlusOutlined } from '@ant-design/icons'
import { Button, Space, Table, Tabs, Typography } from 'antd'
import type { TableColumnsType } from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ContentCard, EmptyState, ErrorState, LoadingState, MaintenanceTaskActions, PageHeader, StatusTag } from '../../components'
import { maintenanceService } from '../../services/maintenanceService'
import type { MaintenancePlan, MaintenanceTask } from '../../types/maintenance'
import { formatDate } from '../../utils'
import './MaintenancePage.css'

function MaintenancePage() {
  const navigate = useNavigate(); const [plans, setPlans] = useState<MaintenancePlan[]>([]); const [tasks, setTasks] = useState<MaintenanceTask[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState<string | null>(null)
  const load = useCallback(async () => { setLoading(true); setError(null); try { const [planData, taskData] = await Promise.all([maintenanceService.getPlans(), maintenanceService.getTasks()]); setPlans(planData); setTasks(taskData) } catch (reason: unknown) { setError(reason instanceof Error ? reason.message : 'Periyodik bakım verileri yüklenemedi.') } finally { setLoading(false) } }, [])
  useEffect(() => { void load() }, [load])
  const taskColumns: TableColumnsType<MaintenanceTask> = [
    { title: 'Cihaz', width: 190, render: (_value, item) => <Space direction="vertical" size={0}><Typography.Text strong>{item.assetCode}</Typography.Text><Typography.Text type="secondary">{item.assetName}</Typography.Text></Space> },
    { title: 'Bakım', dataIndex: 'title', ellipsis: true }, { title: 'Planlanan Tarih', dataIndex: 'plannedDate', width: 135, render: (value: string) => formatDate(value) },
    { title: 'Sorumlu IT', dataIndex: 'responsibleUserName', width: 155, ellipsis: true }, { title: 'Durum', dataIndex: 'displayStatus', width: 115, render: (value: MaintenanceTask['displayStatus']) => <StatusTag status={value} /> },
    { title: 'İşlemler', width: 75, align: 'center', render: (_value, item) => <MaintenanceTaskActions onSuccess={() => void load()} task={item} /> },
  ]
  const planColumns: TableColumnsType<MaintenancePlan> = [
    { title: 'Cihaz', width: 190, render: (_value, item) => `${item.assetCode} · ${item.assetName}` }, { title: 'Plan Adı', dataIndex: 'name', ellipsis: true },
    { title: 'Sorumlu IT', dataIndex: 'responsibleUserName', width: 150 }, { title: 'Sıklık', dataIndex: 'frequencyDays', width: 90, render: (value: number) => `${value} gün` },
    { title: 'Sonraki Bakım', dataIndex: 'nextDueAt', width: 125, render: (value: string) => formatDate(value) }, { title: 'Durum', dataIndex: 'isActive', width: 90, render: (value: boolean) => <StatusTag status={value ? 'Aktif' : 'Pasif'} /> },
  ]
  const sets = useMemo(() => ({ upcoming: tasks.filter((item) => item.displayStatus === 'Yaklaşıyor' || item.displayStatus === 'Planlandı'), overdue: tasks.filter((item) => item.displayStatus === 'Gecikti'), completed: tasks.filter((item) => item.status === 'Tamamlandı') }), [tasks])
  const taskTable = (data: MaintenanceTask[]) => <Table columns={taskColumns} dataSource={data} locale={{ emptyText: <EmptyState /> }} pagination={{ pageSize: 10 }} rowKey="id" scroll={{ x: 930 }} size="small" tableLayout="fixed" />
  return <section className="maintenance-page"><PageHeader title="Periyodik Bakım" description="Planlı ve tekrarlayan IT bakım süreçlerini yönetin." actions={<Button icon={<PlusOutlined />} onClick={() => void navigate('/maintenance/plans/new')} type="primary">Yeni Bakım Planı</Button>} />
    {loading ? <ContentCard><LoadingState /></ContentCard> : error ? <ContentCard><ErrorState message={error} onRetry={() => void load()} /></ContentCard> : <ContentCard><Tabs items={[
      { key: 'upcoming', label: `Yaklaşan (${sets.upcoming.length})`, children: taskTable(sets.upcoming) },
      { key: 'overdue', label: `Geciken (${sets.overdue.length})`, children: taskTable(sets.overdue) },
      { key: 'completed', label: `Tamamlanan (${sets.completed.length})`, children: taskTable(sets.completed) },
      { key: 'plans', label: `Planlar (${plans.length})`, children: <Table columns={planColumns} dataSource={plans} locale={{ emptyText: <EmptyState /> }} pagination={{ pageSize: 10 }} rowKey="id" scroll={{ x: 850 }} size="small" tableLayout="fixed" /> },
    ]} /></ContentCard>}
  </section>
}
export default MaintenancePage
