import { ClearOutlined, DownloadOutlined } from '@ant-design/icons'
import { App, Button, Col, Row, Select, Space, Table, Tag, Typography } from 'antd'
import type { TableColumnsType } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { ContentCard, EmptyState, ErrorState, LoadingState, PageHeader } from '../../components'
import { reportService } from '../../services/reportService'
import type { SupportReportFilters, SupportReportItem } from '../../types/report'
import { formatDate } from '../../utils'
import './ReportsPage.css'

const statuses = ['Açık', 'Atandı', 'İşlemde', 'Tamamlandı', 'İptal Edildi']
const priorities = ['Düşük', 'Normal', 'Yüksek', 'Kritik']

function SupportReportPage() {
  const { message } = App.useApp()
  const [records, setRecords] = useState<SupportReportItem[]>([])
  const [filters, setFilters] = useState<SupportReportFilters>({})
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const load = useCallback(async () => {
    setLoading(true); setError(null)
    try { setRecords(await reportService.getSupportRequests(filters)) }
    catch (reason: unknown) { setError(reason instanceof Error ? reason.message : 'Teknik destek raporu alınamadı.') }
    finally { setLoading(false) }
  }, [filters])
  useEffect(() => { void load() }, [load])
  const columns: TableColumnsType<SupportReportItem> = [
    { title: 'Talep No', dataIndex: 'requestNumber', width: 125, render: (value: string) => <Typography.Text strong>{value}</Typography.Text> },
    { title: 'Cihaz', width: 175, render: (_value, item) => <>{item.assetCode}<br /><Typography.Text type="secondary">{item.assetName}</Typography.Text></> },
    { title: 'Talebi Açan', dataIndex: 'requestedByName', width: 150 }, { title: 'Birim', dataIndex: 'department', width: 140 },
    { title: 'Öncelik', dataIndex: 'priority', width: 90, render: (value: string) => <Tag>{value}</Tag> }, { title: 'Durum', dataIndex: 'status', width: 110, render: (value: string) => <Tag>{value}</Tag> },
    { title: 'Atanan IT', dataIndex: 'assignedToName', width: 140, render: (value: string | null) => value ?? '—' },
    { title: 'Oluşturulma', dataIndex: 'createdAt', width: 120, render: (value: string) => formatDate(value) },
    { title: 'Tamamlayan', dataIndex: 'completedByName', width: 140, render: (value: string | null) => value ?? '—' },
    { title: 'Çözüm', dataIndex: 'result', width: 180, ellipsis: true, render: (value: string | null) => value ?? '—' },
  ]
  const exportCsv = async () => {
    try { await reportService.downloadSupportRequestsCsv(filters); message.success('Teknik destek raporu indirildi.') }
    catch (reason: unknown) { message.error(reason instanceof Error ? reason.message : 'CSV indirilemedi.') }
  }
  return <section><PageHeader title="Teknik Destek Raporu" description="Destek taleplerini gerçek Employee, Asset ve IT ilişkileriyle raporlayın." actions={<Button icon={<DownloadOutlined />} onClick={() => void exportCsv()} type="primary">CSV İndir</Button>} /><ContentCard>{loading ? <LoadingState /> : error ? <ErrorState message={error} onRetry={() => void load()} /> : <Space className="report-page-content" direction="vertical" size="large"><Row gutter={[12, 12]}><Col xs={24} md={9}><Select allowClear options={statuses.map((value) => ({ label: value, value }))} placeholder="Durum" value={filters.status} onChange={(status) => setFilters((current) => ({ ...current, status }))} /></Col><Col xs={24} md={9}><Select allowClear options={priorities.map((value) => ({ label: value, value }))} placeholder="Öncelik" value={filters.priority} onChange={(priority) => setFilters((current) => ({ ...current, priority }))} /></Col><Col xs={24} md={6}><Button block icon={<ClearOutlined />} onClick={() => setFilters({})}>Temizle</Button></Col></Row><Typography.Text type="secondary">{records.length} kayıt bulundu</Typography.Text><Table columns={columns} dataSource={records} locale={{ emptyText: <EmptyState /> }} pagination={{ pageSize: 10, showSizeChanger: true }} rowKey="requestNumber" scroll={{ x: 1370 }} size="small" tableLayout="fixed" /></Space>}</ContentCard></section>
}
export default SupportReportPage
