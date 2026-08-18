import { ClearOutlined, DownloadOutlined } from '@ant-design/icons'
import { App, Button, Col, Row, Select, Space, Table, Tag, Typography } from 'antd'
import type { TableColumnsType } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { ContentCard, EmptyState, ErrorState, LoadingState, PageHeader } from '../../components'
import { reportService } from '../../services/reportService'
import type { WarrantyReportFilters, WarrantyReportItem } from '../../types/report'
import { formatDate } from '../../utils'
import './ReportsPage.css'

const warrantyStatuses = ['Aktif', 'Yaklaşıyor', 'Süresi Doldu', 'Garanti Bilgisi Yok']
const assetStatuses = ['Boşta', 'Zimmetli', 'Bakımda', 'Kayıp', 'Hurda', 'Elden Çıkarıldı']

function WarrantyReportPage() {
  const { message } = App.useApp()
  const [records, setRecords] = useState<WarrantyReportItem[]>([])
  const [filters, setFilters] = useState<WarrantyReportFilters>({})
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const load = useCallback(async () => {
    setLoading(true); setError(null)
    try { setRecords(await reportService.getWarranties(filters)) }
    catch (reason: unknown) { setError(reason instanceof Error ? reason.message : 'Garanti raporu alınamadı.') }
    finally { setLoading(false) }
  }, [filters])
  useEffect(() => { void load() }, [load])
  const columns: TableColumnsType<WarrantyReportItem> = [
    { title: 'Cihaz', width: 190, render: (_value, item) => <><Typography.Text strong>{item.assetCode}</Typography.Text><br /><Typography.Text type="secondary">{item.assetName}</Typography.Text></> },
    { title: 'Kategori', dataIndex: 'category', width: 140 },
    { title: 'Garanti Bitişi', dataIndex: 'warrantyEndDate', width: 125, render: (value: string | null) => formatDate(value) },
    { title: 'Garanti Durumu', dataIndex: 'warrantyStatus', width: 140, render: (value: string) => <Tag>{value}</Tag> },
    { title: 'Kullanım Durumu', dataIndex: 'assetStatus', width: 130, render: (value: string) => <Tag>{value}</Tag> },
    { title: 'Zimmetli Çalışan', dataIndex: 'currentAssigneeName', width: 160, render: (value: string | null) => value ?? '—' },
    { title: 'Birim', dataIndex: 'currentAssigneeDepartment', width: 145, render: (value: string | null) => value ?? '—' },
  ]
  const exportCsv = async () => {
    try { await reportService.downloadWarrantiesCsv(filters); message.success('Garanti raporu indirildi.') }
    catch (reason: unknown) { message.error(reason instanceof Error ? reason.message : 'CSV indirilemedi.') }
  }
  return <section><PageHeader title="Garanti Raporu" description="Cihaz garanti ve güncel kullanım durumlarını raporlayın." actions={<Button icon={<DownloadOutlined />} onClick={() => void exportCsv()} type="primary">CSV İndir</Button>} /><ContentCard>{loading ? <LoadingState /> : error ? <ErrorState message={error} onRetry={() => void load()} /> : <Space className="report-page-content" direction="vertical" size="large"><Row gutter={[12, 12]}><Col xs={24} md={10}><Select allowClear options={warrantyStatuses.map((value) => ({ label: value, value }))} placeholder="Garanti Durumu" value={filters.warrantyStatus} onChange={(warrantyStatus) => setFilters((current) => ({ ...current, warrantyStatus }))} /></Col><Col xs={24} md={10}><Select allowClear options={assetStatuses.map((value) => ({ label: value, value }))} placeholder="Kullanım Durumu" value={filters.assetStatus} onChange={(assetStatus) => setFilters((current) => ({ ...current, assetStatus }))} /></Col><Col xs={24} md={4}><Button block icon={<ClearOutlined />} onClick={() => setFilters({})}>Temizle</Button></Col></Row><Typography.Text type="secondary">{records.length} kayıt bulundu</Typography.Text><Table columns={columns} dataSource={records} locale={{ emptyText: <EmptyState /> }} pagination={{ pageSize: 10, showSizeChanger: true }} rowKey="assetCode" scroll={{ x: 1030 }} size="small" tableLayout="fixed" /></Space>}</ContentCard></section>
}
export default WarrantyReportPage
