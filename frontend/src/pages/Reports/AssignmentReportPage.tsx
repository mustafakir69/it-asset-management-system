import { ClearOutlined, DownloadOutlined } from '@ant-design/icons'
import { App, Button, Col, DatePicker, Row, Select, Space, Table, Tag, Typography } from 'antd'
import type { TableColumnsType } from 'antd'
import type { Dayjs } from 'dayjs'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { ContentCard, EmptyState, ErrorState, LoadingState, PageHeader } from '../../components'
import { reportService } from '../../services/reportService'
import type { AssignmentReportFilters, AssignmentReportItem } from '../../types/report'
import { formatDate } from '../../utils'
import './ReportsPage.css'

function AssignmentReportPage() {
  const { message } = App.useApp(); const [records, setRecords] = useState<AssignmentReportItem[]>([]); const [filters, setFilters] = useState<AssignmentReportFilters>({}); const [loading, setLoading] = useState(true); const [error, setError] = useState<string | null>(null)
  const load = useCallback(async () => { setLoading(true); setError(null); try { setRecords(await reportService.getAssignments(filters)) } catch (reason: unknown) { setError(reason instanceof Error ? reason.message : 'Zimmet raporu alınamadı.') } finally { setLoading(false) } }, [filters])
  useEffect(() => { void load() }, [load])
  const departments = useMemo(() => [...new Set(records.map((item) => item.department))].sort().map((value) => ({ value, label: value })), [records])
  const columns: TableColumnsType<AssignmentReportItem> = [
    { title: 'Varlık Kodu', dataIndex: 'assetCode', width: 125, render: (value: string) => <Typography.Text strong>{value}</Typography.Text> }, { title: 'Cihaz', dataIndex: 'assetName', width: 180 }, { title: 'Çalışan', dataIndex: 'employeeName', width: 160 }, { title: 'Departman', dataIndex: 'department', width: 140 }, { title: 'Zimmet Tarihi', dataIndex: 'assignedAt', width: 125, render: (value: string) => formatDate(value) }, { title: 'İade Tarihi', dataIndex: 'returnedAt', width: 125, render: (value: string | null) => formatDate(value) }, { title: 'Durum', dataIndex: 'status', width: 110, render: (value: string) => <Tag color={value === 'Aktif' ? 'green' : 'cyan'}>{value}</Tag> }, { title: 'Zimmetleyen', dataIndex: 'assignedBy', width: 140 }, { title: 'İade Alan', dataIndex: 'returnedBy', width: 140, render: (value: string | null) => value ?? '—' },
  ]
  const setDate = (key: 'from' | 'to', value: Dayjs | null) => setFilters((current) => ({ ...current, [key]: value?.startOf(key === 'from' ? 'day' : 'day').toISOString() }))
  const exportCsv = async () => { try { await reportService.downloadAssignmentsCsv(filters); void message.success('Zimmet raporu indirildi.') } catch (reason: unknown) { void message.error(reason instanceof Error ? reason.message : 'CSV indirilemedi.') } }
  return <section><PageHeader title="Zimmet Raporu" description="Aktif ve iade edilmiş zimmet kayıtlarını raporlayın." actions={<Button icon={<DownloadOutlined />} onClick={() => void exportCsv()} type="primary">CSV İndir</Button>} /><ContentCard>{error ? <ErrorState message={error} onRetry={() => void load()} /> : loading ? <LoadingState message="Zimmet raporu yükleniyor..." /> : <Space className="report-page-content" direction="vertical" size="large"><Row gutter={[12, 12]}><Col xs={24} md={5}><Select allowClear options={[{ value: 'Aktif', label: 'Aktif' }, { value: 'İade Edildi', label: 'İade Edildi' }]} placeholder="Durum" value={filters.status} onChange={(status) => setFilters((current) => ({ ...current, status }))} /></Col><Col xs={24} md={5}><Select allowClear options={departments} placeholder="Departman" value={filters.department} onChange={(department) => setFilters((current) => ({ ...current, department }))} /></Col><Col xs={24} md={5}><DatePicker aria-label="Başlangıç tarihi" onChange={(value) => setDate('from', value)} placeholder="Başlangıç" /></Col><Col xs={24} md={5}><DatePicker aria-label="Bitiş tarihi" onChange={(value) => setDate('to', value)} placeholder="Bitiş" /></Col><Col xs={24} md={4}><Button block icon={<ClearOutlined />} onClick={() => setFilters({})}>Temizle</Button></Col></Row><Typography.Text type="secondary">{records.length} kayıt bulundu</Typography.Text><Table columns={columns} dataSource={records} locale={{ emptyText: <EmptyState description="Filtrelere uygun zimmet kaydı bulunamadı." /> }} pagination={{ defaultPageSize: 10, pageSizeOptions: ['10', '20', '50'], showSizeChanger: true }} rowKey={(item) => `${item.assetCode}-${item.assignedAt}`} scroll={{ x: 1245 }} size="small" tableLayout="fixed" /></Space>}</ContentCard></section>
}
export default AssignmentReportPage
