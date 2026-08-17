import { ClearOutlined, PlusOutlined, SearchOutlined } from '@ant-design/icons'
import { Button, Col, Input, Row, Select, Space, Table, Typography } from 'antd'
import type { TableColumnsType } from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ContentCard, EmptyState, ErrorState, LoadingState, MaintenanceRequestActions, PageHeader, StatusTag } from '../../components'
import { useAuth } from '../../contexts/useAuth'
import { maintenanceService } from '../../services/maintenanceService'
import { maintenanceRequestPriorities, maintenanceRequestStatuses, type MaintenanceRequest, type MaintenanceRequestPriority, type MaintenanceRequestStatus } from '../../types/maintenance'
import { formatDate } from '../../utils'
import './MaintenancePage.css'

function MaintenanceRequestsPage() {
  const navigate = useNavigate(); const { user } = useAuth()
  const [requests, setRequests] = useState<MaintenanceRequest[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState<string | null>(null)
  const [search, setSearch] = useState(''); const [status, setStatus] = useState<MaintenanceRequestStatus>(); const [priority, setPriority] = useState<MaintenanceRequestPriority>()
  const load = useCallback(async () => { setLoading(true); setError(null); try { setRequests(await (user?.role === 'Employee' ? maintenanceService.getMyRequests() : maintenanceService.getRequests())) } catch (reason: unknown) { setError(reason instanceof Error ? reason.message : 'Teknik destek talepleri yüklenemedi.') } finally { setLoading(false) } }, [user?.role])
  useEffect(() => { void load() }, [load])
  const filtered = useMemo(() => { const term = search.trim().toLocaleLowerCase('tr-TR'); return requests.filter((item) => (!term || [item.assetCode, item.assetName, item.title, item.requestedByName, item.assignedToName ?? ''].some((value) => value.toLocaleLowerCase('tr-TR').includes(term))) && (!status || item.status === status) && (!priority || item.priority === priority)) }, [priority, requests, search, status])
  const columns: TableColumnsType<MaintenanceRequest> = [
    { title: 'Talep No', dataIndex: 'requestNumber', width: 120, render: (value: string) => <Typography.Text strong>{value}</Typography.Text> },
    { title: 'Cihaz', width: 180, render: (_value, item) => <Space direction="vertical" size={0}><Typography.Text>{item.assetCode}</Typography.Text><Typography.Text type="secondary">{item.assetName}</Typography.Text></Space> },
    { title: 'Konu', dataIndex: 'title', width: 190, ellipsis: true },
    { title: 'Öncelik', dataIndex: 'priority', width: 95, render: (value: MaintenanceRequestPriority) => <StatusTag status={value} /> },
    { title: 'Talebi Açan', dataIndex: 'requestedByName', width: 145, ellipsis: true, responsive: ['lg'] },
    { title: 'Atanan IT', dataIndex: 'assignedToName', width: 145, ellipsis: true, responsive: ['xl'], render: (value: string | null) => value ?? '—' },
    { title: 'Oluşturulma', dataIndex: 'createdAt', width: 120, render: (value: string) => formatDate(value) },
    { title: 'Durum', dataIndex: 'status', width: 115, render: (value: MaintenanceRequestStatus) => <StatusTag status={value} /> },
    { title: 'İşlemler', width: 75, align: 'center', render: (_value, item) => <MaintenanceRequestActions onSuccess={() => void load()} request={item} /> },
  ]
  return <section className="maintenance-page">
    <PageHeader title="Teknik Destek" description={user?.role === 'Employee' ? 'Kendi teknik destek taleplerinizi izleyin.' : 'Şirket teknik destek taleplerini yönetin.'} actions={user?.role === 'Employee' ? <Button icon={<PlusOutlined />} onClick={() => void navigate('/support-requests/new')} type="primary">Yeni Destek Talebi</Button> : undefined} />
    {loading ? <ContentCard><LoadingState /></ContentCard> : error ? <ContentCard><ErrorState message={error} onRetry={() => void load()} /></ContentCard> : <ContentCard><Space className="maintenance-table-content" direction="vertical" size="large">
      <Row gutter={[12, 12]}><Col xs={24} lg={10}><Input allowClear prefix={<SearchOutlined />} placeholder="Cihaz, konu, çalışan veya IT personeli ara" value={search} onChange={(event) => setSearch(event.target.value)} /></Col><Col xs={24} sm={8} lg={5}><Select allowClear options={maintenanceRequestStatuses.map((value) => ({ label: value, value }))} placeholder="Durum" value={status} onChange={setStatus} /></Col><Col xs={24} sm={8} lg={4}><Select allowClear options={maintenanceRequestPriorities.map((value) => ({ label: value, value }))} placeholder="Öncelik" value={priority} onChange={setPriority} /></Col><Col xs={24} sm={8} lg={5}><Button block icon={<ClearOutlined />} onClick={() => { setSearch(''); setStatus(undefined); setPriority(undefined) }}>Filtreleri Temizle</Button></Col></Row>
      <Typography.Text type="secondary">{filtered.length} kayıt bulundu</Typography.Text>
      <Table className="app-data-table" columns={columns} dataSource={filtered} locale={{ emptyText: <EmptyState /> }} pagination={{ pageSize: 10, pageSizeOptions: ['10', '20', '50'], showSizeChanger: true }} rowKey="id" scroll={{ x: 1120 }} size="small" tableLayout="fixed" />
    </Space></ContentCard>}
  </section>
}
export default MaintenanceRequestsPage
