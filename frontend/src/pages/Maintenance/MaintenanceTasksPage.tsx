import { ClearOutlined, SearchOutlined } from '@ant-design/icons'
import { Button, Col, Input, Row, Select, Space, Table, Typography } from 'antd'
import type { TableColumnsType, TablePaginationConfig } from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { ContentCard, EmptyState, ErrorState, LoadingState, MaintenanceTaskActions, PageHeader, StatusTag } from '../../components'
import { maintenanceService } from '../../services/maintenanceService'
import { maintenanceTaskStatuses, type MaintenanceTask, type MaintenanceTaskStatus } from '../../types/maintenance'
import { formatDate } from '../../utils'
import './MaintenancePage.css'

function MaintenanceTasksPage() {
  const [tasks, setTasks] = useState<MaintenanceTask[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState<MaintenanceTaskStatus>()
  const [pagination, setPagination] = useState({ current: 1, pageSize: 10 })

  const loadTasks = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)
    try { setTasks(await maintenanceService.getTasks()) }
    catch (error: unknown) { setLoadError(error instanceof Error ? error.message : 'Bakım görevleri yüklenemedi.') }
    finally { setIsLoading(false) }
  }, [])

  useEffect(() => { void loadTasks() }, [loadTasks])

  const filteredTasks = useMemo(() => {
    const normalized = search.trim().toLocaleLowerCase('tr-TR')
    return tasks.filter((task) => (normalized.length === 0 || [task.assetCode, task.assetName, task.title].some((value) => value.toLocaleLowerCase('tr-TR').includes(normalized))) && (!status || task.displayStatus === status))
  }, [search, status, tasks])

  const columns: TableColumnsType<MaintenanceTask> = [
    { title: 'Cihaz', key: 'asset', width: 210, sorter: (a, b) => a.assetCode.localeCompare(b.assetCode, 'tr-TR'), render: (_value, task) => <Space direction="vertical" size={0}><Typography.Text strong>{task.assetCode}</Typography.Text><Typography.Text type="secondary">{task.assetName}</Typography.Text></Space> },
    { title: 'Bakım', dataIndex: 'title', key: 'title', ellipsis: true, width: 210, sorter: (a, b) => a.title.localeCompare(b.title, 'tr-TR') },
    { title: 'Planlanan Tarih', dataIndex: 'plannedDate', key: 'plannedDate', align: 'center', width: 135, sorter: (a, b) => a.plannedDate.localeCompare(b.plannedDate), render: (value: string) => formatDate(value) },
    { title: 'Sorumlu IT', dataIndex: 'responsibleUserName', key: 'responsibleUserName', ellipsis: true, width: 155, responsive: ['md'] },
    { title: 'Durum', dataIndex: 'displayStatus', key: 'displayStatus', align: 'center', width: 120, render: (value: MaintenanceTaskStatus) => <StatusTag status={value} /> },
    { title: 'İşlemler', key: 'actions', align: 'center', width: 72, render: (_value, task) => <MaintenanceTaskActions onSuccess={() => void loadTasks()} task={task} /> },
  ]

  const resetPage = () => setPagination((current) => ({ ...current, current: 1 }))
  const handlePagination = (next: TablePaginationConfig) => setPagination({ current: next.current ?? 1, pageSize: next.pageSize ?? 10 })

  return <section className="maintenance-page"><PageHeader title="Bakım Görevleri" description="Planlanan, yaklaşan ve geciken bakım görevlerini takip edin." />{isLoading ? <ContentCard><LoadingState message="Bakım görevleri yükleniyor..." /></ContentCard> : loadError ? <ContentCard><ErrorState message={loadError} onRetry={() => void loadTasks()} /></ContentCard> : <ContentCard><Space className="maintenance-table-content" direction="vertical" size="large"><Row gutter={[12, 12]}><Col xs={24} md={12}><Input allowClear aria-label="Bakım görevlerinde ara" placeholder="Cihaz kodu, cihaz veya bakım ara" prefix={<SearchOutlined />} value={search} onChange={(event) => { setSearch(event.target.value); resetPage() }} /></Col><Col xs={24} sm={12} md={6}><Select<MaintenanceTaskStatus> allowClear aria-label="Bakım durumu filtresi" options={maintenanceTaskStatuses.map((item) => ({ label: item, value: item }))} placeholder="Durum" value={status} onChange={(value) => { setStatus(value); resetPage() }} /></Col><Col xs={24} sm={12} md={6}><Button block icon={<ClearOutlined />} onClick={() => { setSearch(''); setStatus(undefined); resetPage() }}>Filtreleri Temizle</Button></Col></Row><Typography.Text type="secondary">{filteredTasks.length} kayıt bulundu</Typography.Text><Table<MaintenanceTask> className="app-data-table" columns={columns} dataSource={filteredTasks} locale={{ emptyText: <EmptyState description="Filtrelere uygun bakım görevi bulunamadı." /> }} onChange={handlePagination} pagination={{ ...pagination, pageSizeOptions: ['10', '20', '50'], showSizeChanger: true, showTotal: (total) => `Toplam ${total} kayıt` }} rowKey="id" scroll={{ x: 897 }} size="small" tableLayout="fixed" /></Space></ContentCard>}</section>
}

export default MaintenanceTasksPage
