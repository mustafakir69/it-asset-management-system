import { ClearOutlined, DownloadOutlined } from '@ant-design/icons'
import { App, Button, Col, Row, Select, Space, Table, Tag, Typography } from 'antd'
import type { TableColumnsType } from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { ContentCard, EmptyState, ErrorState, LoadingState, PageHeader } from '../../components'
import { reportService } from '../../services/reportService'
import type { InventoryReportFilters, InventoryReportItem } from '../../types/report'
import { formatDate } from '../../utils'
import './ReportsPage.css'

function InventoryReportPage() {
  const { message } = App.useApp()
  const [records, setRecords] = useState<InventoryReportItem[]>([])
  const [filters, setFilters] = useState<InventoryReportFilters>({})
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const load = useCallback(async () => { setLoading(true); setError(null); try { setRecords(await reportService.getInventory(filters)) } catch (reason: unknown) { setError(reason instanceof Error ? reason.message : 'Envanter raporu alınamadı.') } finally { setLoading(false) } }, [filters])
  useEffect(() => { void load() }, [load])
  const options = useMemo(() => ({
    categories: [...new Set(records.map((item) => item.category))].sort().map((value) => ({ value, label: value })),
    statuses: [...new Set(records.map((item) => item.status))].sort().map((value) => ({ value, label: value })),
    locations: [...new Set(records.map((item) => item.location))].sort().map((value) => ({ value, label: value })),
  }), [records])
  const columns: TableColumnsType<InventoryReportItem> = [
    { title: 'Varlık Kodu', dataIndex: 'assetCode', width: 130, sorter: (a, b) => a.assetCode.localeCompare(b.assetCode, 'tr-TR'), render: (value: string) => <Typography.Text strong>{value}</Typography.Text> },
    { title: 'Kategori', dataIndex: 'category', width: 150 },
    { title: 'Cihaz', key: 'device', width: 180, render: (_, item) => <>{item.brand} {item.model}</> },
    { title: 'Seri Numarası', dataIndex: 'serialNumber', width: 150 },
    { title: 'Durum', dataIndex: 'status', width: 115, render: (value: string) => <Tag>{value}</Tag> },
    { title: 'Lokasyon', dataIndex: 'location', width: 150 },
    { title: 'Satın Alma', dataIndex: 'purchaseDate', width: 125, render: (value: string) => formatDate(value) },
    { title: 'Garanti Bitişi', dataIndex: 'warrantyEndDate', width: 125, render: (value: string) => formatDate(value) },
  ]
  const exportCsv = async () => { try { await reportService.downloadInventoryCsv(filters); void message.success('Envanter raporu indirildi.') } catch (reason: unknown) { void message.error(reason instanceof Error ? reason.message : 'CSV indirilemedi.') } }
  return <section><PageHeader title="Envanter Raporu" description="Cihaz envanterini gerçek sistem verileriyle raporlayın." actions={<Button icon={<DownloadOutlined />} onClick={() => void exportCsv()} type="primary">CSV İndir</Button>} /><ContentCard>{error ? <ErrorState message={error} onRetry={() => void load()} /> : loading ? <LoadingState message="Envanter raporu yükleniyor..." /> : <Space className="report-page-content" direction="vertical" size="large"><Row gutter={[12, 12]}><Col xs={24} md={7}><Select allowClear options={options.categories} placeholder="Kategori" value={filters.category} onChange={(category) => setFilters((current) => ({ ...current, category }))} /></Col><Col xs={24} md={7}><Select allowClear options={options.statuses} placeholder="Durum" value={filters.status} onChange={(status) => setFilters((current) => ({ ...current, status }))} /></Col><Col xs={24} md={7}><Select allowClear options={options.locations} placeholder="Lokasyon" value={filters.location} onChange={(location) => setFilters((current) => ({ ...current, location }))} /></Col><Col xs={24} md={3}><Button block icon={<ClearOutlined />} onClick={() => setFilters({})}>Temizle</Button></Col></Row><Typography.Text type="secondary">{records.length} kayıt bulundu</Typography.Text><Table columns={columns} dataSource={records} locale={{ emptyText: <EmptyState description="Filtrelere uygun envanter kaydı bulunamadı." /> }} pagination={{ defaultPageSize: 10, pageSizeOptions: ['10', '20', '50'], showSizeChanger: true }} rowKey="assetCode" scroll={{ x: 1125 }} size="small" tableLayout="fixed" /></Space>}</ContentCard></section>
}
export default InventoryReportPage
