import { DownloadOutlined } from '@ant-design/icons'
import { App, Button, Row, Col, Space, Statistic, Table, Tag, Typography } from 'antd'
import type { TableColumnsType } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { ContentCard, EmptyState, ErrorState, LoadingState, PageHeader } from '../../components'
import { reportService } from '../../services/reportService'
import type { MaintenanceReportItem, MaintenanceReportResponse } from '../../types/report'
import { formatDate } from '../../utils'

function MaintenanceReportPage() {
  const { message } = App.useApp(); const [report, setReport] = useState<MaintenanceReportResponse | null>(null); const [loading, setLoading] = useState(true); const [error, setError] = useState<string | null>(null)
  const load = useCallback(async () => { setLoading(true); setError(null); try { setReport(await reportService.getMaintenance({})) } catch (reason: unknown) { setError(reason instanceof Error ? reason.message : 'Bakım raporu alınamadı.') } finally { setLoading(false) } }, [])
  useEffect(() => { void load() }, [load])
  const columns: TableColumnsType<MaintenanceReportItem> = [
    { title: 'Cihaz', width: 180, render: (_value, item) => <><Typography.Text strong>{item.assetCode}</Typography.Text><br /><Typography.Text type="secondary">{item.assetName}</Typography.Text></> },
    { title: 'Bakım / Destek', dataIndex: 'title', width: 190 }, { title: 'Kayıt Türü', dataIndex: 'recordType', width: 125 },
    { title: 'Planlanan', dataIndex: 'plannedDate', width: 120, render: (value: string | null) => formatDate(value) }, { title: 'Tamamlanan', dataIndex: 'completedAt', width: 120, render: (value: string | null) => formatDate(value) },
    { title: 'İşlem Aktörü', dataIndex: 'actorName', width: 145, render: (value: string | null) => value ?? '—' }, { title: 'Sonuç', dataIndex: 'result', width: 180, ellipsis: true },
    { title: 'Durum', dataIndex: 'status', width: 115, render: (value: string) => <Tag>{value}</Tag> },
  ]
  const exportCsv = async () => { try { await reportService.downloadMaintenanceCsv({}); message.success('Bakım raporu indirildi.') } catch (reason: unknown) { message.error(reason instanceof Error ? reason.message : 'CSV indirilemedi.') } }
  return <section><PageHeader title="Bakım ve Destek Raporu" description="Periyodik bakım ve teknik destek sonuçları." actions={<Button icon={<DownloadOutlined />} onClick={() => void exportCsv()} type="primary">CSV İndir</Button>} /><ContentCard>{loading ? <LoadingState /> : error ? <ErrorState message={error} onRetry={() => void load()} /> : !report ? <EmptyState /> : <Space direction="vertical" size="large" style={{ width: '100%' }}><Row gutter={[12, 12]}><Col xs={12} md={6}><Statistic title="Planlanan" value={report.summary.planned} /></Col><Col xs={12} md={6}><Statistic title="Tamamlanan" value={report.summary.completed} /></Col><Col xs={12} md={6}><Statistic title="Geciken" value={report.summary.overdue} /></Col><Col xs={12} md={6}><Statistic title="İptal" value={report.summary.cancelled} /></Col></Row><Table columns={columns} dataSource={report.records} locale={{ emptyText: <EmptyState /> }} pagination={{ pageSize: 10, showSizeChanger: true }} rowKey={(item) => `${item.recordType}-${item.id}`} scroll={{ x: 1175 }} size="small" tableLayout="fixed" /></Space>}</ContentCard></section>
}
export default MaintenanceReportPage
