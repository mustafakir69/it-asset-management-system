import { ClearOutlined, PlusOutlined, SearchOutlined } from '@ant-design/icons'
import { Button, Col, Input, Row, Select, Space, Table, Typography } from 'antd'
import type { TableColumnsType, TablePaginationConfig } from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ContentCard, EmptyState, ErrorState, LoadingState, MaintenanceRequestActions, PageHeader, StatusTag } from '../../components'
import { maintenanceService } from '../../services/maintenanceService'
import { maintenanceRequestPriorities, maintenanceRequestStatuses, type MaintenanceRequest, type MaintenanceRequestPriority, type MaintenanceRequestStatus } from '../../types/maintenance'
import { formatDate } from '../../utils'
import './MaintenancePage.css'

function MaintenanceRequestsPage() {
  const navigate = useNavigate(); const [requests, setRequests] = useState<MaintenanceRequest[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState<string | null>(null); const [search, setSearch] = useState(''); const [status, setStatus] = useState<MaintenanceRequestStatus>(); const [priority, setPriority] = useState<MaintenanceRequestPriority>(); const [pagination, setPagination] = useState({ current: 1, pageSize: 10 })
  const load = useCallback(async () => { setLoading(true); setError(null); try { setRequests(await maintenanceService.getRequests()) } catch (reason: unknown) { setError(reason instanceof Error ? reason.message : 'Bakım talepleri yüklenemedi.') } finally { setLoading(false) } }, [])
  useEffect(() => { void load() }, [load])
  const filtered = useMemo(() => { const normalized = search.trim().toLocaleLowerCase('tr-TR'); return requests.filter((item) => (normalized.length === 0 || [item.assetCode, item.assetName, item.title, item.requestedBy, item.assignedTechnician ?? ''].some((value) => value.toLocaleLowerCase('tr-TR').includes(normalized))) && (!status || item.status === status) && (!priority || item.priority === priority)) }, [priority, requests, search, status])
  const columns: TableColumnsType<MaintenanceRequest> = [
    { title: 'Talep No', dataIndex: 'requestNumber', key: 'requestNumber', width: 120, render: (value: string) => <Typography.Text strong>{value}</Typography.Text> },
    { title: 'Cihaz', key: 'asset', width: 180, render: (_value, item) => <Space direction="vertical" size={0}><Typography.Text>{item.assetCode}</Typography.Text><Typography.Text type="secondary">{item.assetName}</Typography.Text></Space> },
    { title: 'Başlık', dataIndex: 'title', key: 'title', width: 170, ellipsis: true },
    { title: 'Öncelik', dataIndex: 'priority', key: 'priority', align: 'center', width: 90, render: (value: MaintenanceRequestPriority) => <StatusTag status={value} /> },
    { title: 'Talebi Açan', dataIndex: 'requestedBy', key: 'requestedBy', width: 130, ellipsis: true, responsive: ['lg'] },
    { title: 'Atanan Teknisyen', dataIndex: 'assignedTechnician', key: 'assignedTechnician', width: 140, ellipsis: true, responsive: ['xl'], render: (value: string | null) => value || '—' },
    { title: 'Oluşturulma', dataIndex: 'createdAt', key: 'createdAt', width: 115, sorter: (a, b) => a.createdAt.localeCompare(b.createdAt), render: (value: string) => formatDate(value) },
    { title: 'Durum', dataIndex: 'status', key: 'status', align: 'center', width: 110, render: (value: MaintenanceRequestStatus) => <StatusTag status={value} /> },
    { title: 'İşlemler', key: 'actions', align: 'center', width: 72, render: (_value, item) => <MaintenanceRequestActions onSuccess={() => void load()} request={item} /> },
  ]
  const reset = () => setPagination((current) => ({ ...current, current: 1 })); const page = (next: TablePaginationConfig) => setPagination({ current: next.current ?? 1, pageSize: next.pageSize ?? 10 })
  return <section className="maintenance-page"><PageHeader title="Bakım Talepleri" description="Manuel bakım, arıza ve destek taleplerini yönetin." actions={<Button icon={<PlusOutlined />} onClick={() => void navigate('/maintenance/requests/new')} type="primary">Yeni Bakım Talebi</Button>} />{loading ? <ContentCard><LoadingState /></ContentCard> : error ? <ContentCard><ErrorState message={error} onRetry={() => void load()} /></ContentCard> : <ContentCard><Space className="maintenance-table-content" direction="vertical" size="large"><Row gutter={[12, 12]}><Col xs={24} lg={10}><Input allowClear prefix={<SearchOutlined />} placeholder="Cihaz, başlık, talebi açan veya teknisyen ara" value={search} onChange={(event) => { setSearch(event.target.value); reset() }} /></Col><Col xs={24} sm={8} lg={5}><Select allowClear options={maintenanceRequestStatuses.map((value) => ({ label: value, value }))} placeholder="Durum" value={status} onChange={(value) => { setStatus(value); reset() }} /></Col><Col xs={24} sm={8} lg={4}><Select allowClear options={maintenanceRequestPriorities.map((value) => ({ label: value, value }))} placeholder="Öncelik" value={priority} onChange={(value) => { setPriority(value); reset() }} /></Col><Col xs={24} sm={8} lg={5}><Button block icon={<ClearOutlined />} onClick={() => { setSearch(''); setStatus(undefined); setPriority(undefined); reset() }}>Filtreleri Temizle</Button></Col></Row><Typography.Text type="secondary">{filtered.length} kayıt bulundu</Typography.Text><Table<MaintenanceRequest> className="app-data-table" columns={columns} dataSource={filtered} locale={{ emptyText: <EmptyState /> }} onChange={page} pagination={{ ...pagination, pageSizeOptions: ['10', '20', '50'], showSizeChanger: true }} rowKey="id" scroll={{ x: 1127 }} size="small" tableLayout="fixed" /></Space></ContentCard>}</section>
}
export default MaintenanceRequestsPage
