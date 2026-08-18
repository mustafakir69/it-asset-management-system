import { ClearOutlined, DownloadOutlined } from '@ant-design/icons'
import { App, Button, Col, Row, Select, Space, Table, Tag, Typography } from 'antd'
import type { TableColumnsType } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { ContentCard, EmptyState, ErrorState, LoadingState, PageHeader } from '../../components'
import { reportService } from '../../services/reportService'
import type { LicenseReportFilters, LicenseReportItem } from '../../types/report'
import { formatDate } from '../../utils'
import './ReportsPage.css'

const statuses = ['Aktif', 'Yaklaşıyor', 'Süresi Doldu', 'Pasif']

function LicenseReportPage() {
  const { message } = App.useApp()
  const [records, setRecords] = useState<LicenseReportItem[]>([])
  const [filters, setFilters] = useState<LicenseReportFilters>({})
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const load = useCallback(async () => {
    setLoading(true); setError(null)
    try { setRecords(await reportService.getLicenses(filters)) }
    catch (reason: unknown) { setError(reason instanceof Error ? reason.message : 'Lisans raporu alınamadı.') }
    finally { setLoading(false) }
  }, [filters])
  useEffect(() => { void load() }, [load])
  const columns: TableColumnsType<LicenseReportItem> = [
    { title: 'Lisans Kodu', dataIndex: 'licenseCode', width: 140, render: (value: string) => <Typography.Text strong>{value}</Typography.Text> },
    { title: 'Ürün', dataIndex: 'productName', width: 180 }, { title: 'Sağlayıcı', dataIndex: 'vendor', width: 130 },
    { title: 'Tür', dataIndex: 'licenseType', width: 120 }, { title: 'Toplam', dataIndex: 'totalSeats', align: 'center', width: 80 },
    { title: 'Kullanılan', dataIndex: 'usedSeats', align: 'center', width: 90 }, { title: 'Kalan', dataIndex: 'availableSeats', align: 'center', width: 80 },
    { title: 'Bitiş', dataIndex: 'expirationDate', width: 115, render: (value: string | null) => formatDate(value) },
    { title: 'Durum', dataIndex: 'status', width: 115, render: (value: string) => <Tag>{value}</Tag> },
  ]
  const exportCsv = async () => {
    try { await reportService.downloadLicensesCsv(filters); message.success('Lisans raporu indirildi.') }
    catch (reason: unknown) { message.error(reason instanceof Error ? reason.message : 'CSV indirilemedi.') }
  }
  return <section><PageHeader title="Lisans Raporu" description="Lisans haklarını ve ilişkisel atama kullanımını raporlayın." actions={<Button icon={<DownloadOutlined />} onClick={() => void exportCsv()} type="primary">CSV İndir</Button>} /><ContentCard>{loading ? <LoadingState /> : error ? <ErrorState message={error} onRetry={() => void load()} /> : <Space className="report-page-content" direction="vertical" size="large"><Row gutter={[12, 12]}><Col xs={24} md={18}><Select allowClear options={statuses.map((value) => ({ label: value, value }))} placeholder="Lisans Durumu" value={filters.status} onChange={(status) => setFilters({ status })} /></Col><Col xs={24} md={6}><Button block icon={<ClearOutlined />} onClick={() => setFilters({})}>Temizle</Button></Col></Row><Typography.Text type="secondary">{records.length} kayıt bulundu</Typography.Text><Table columns={columns} dataSource={records} locale={{ emptyText: <EmptyState /> }} pagination={{ pageSize: 10, showSizeChanger: true }} rowKey="licenseCode" scroll={{ x: 1050 }} size="small" tableLayout="fixed" /></Space>}</ContentCard></section>
}
export default LicenseReportPage
