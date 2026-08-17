import { DownloadOutlined } from '@ant-design/icons'
import { App, Button, Space, Table, Tag, Typography } from 'antd'
import type { TableColumnsType } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { ContentCard, EmptyState, ErrorState, LoadingState, PageHeader } from '../../components'
import { reportService } from '../../services/reportService'
import type { AssignmentReportItem } from '../../types/report'
import { formatDate } from '../../utils'

function AssignmentReportPage() {
  const { message } = App.useApp(); const [records, setRecords] = useState<AssignmentReportItem[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState<string | null>(null)
  const load = useCallback(async () => { setLoading(true); setError(null); try { setRecords(await reportService.getAssignments({})) } catch (reason: unknown) { setError(reason instanceof Error ? reason.message : 'Zimmet raporu alınamadı.') } finally { setLoading(false) } }, [])
  useEffect(() => { void load() }, [load])
  const columns: TableColumnsType<AssignmentReportItem> = [
    { title: 'Varlık Kodu', dataIndex: 'assetCode', width: 125, render: (value: string) => <Typography.Text strong>{value}</Typography.Text> }, { title: 'Cihaz', dataIndex: 'assetName', width: 180 },
    { title: 'Çalışan', dataIndex: 'employeeName', width: 160 }, { title: 'Departman', dataIndex: 'department', width: 140 },
    { title: 'Zimmet Tarihi', dataIndex: 'assignedAt', width: 125, render: (value: string) => formatDate(value) }, { title: 'İade Tarihi', dataIndex: 'returnedAt', width: 125, render: (value: string | null) => formatDate(value) },
    { title: 'Durum', dataIndex: 'status', width: 110, render: (value: string) => <Tag>{value}</Tag> }, { title: 'Zimmetleyen', dataIndex: 'assignedByName', width: 145 },
    { title: 'İade Alan', dataIndex: 'returnedByName', width: 145, render: (value: string | null) => value ?? '—' },
  ]
  const exportCsv = async () => { try { await reportService.downloadAssignmentsCsv({}); message.success('Zimmet raporu indirildi.') } catch (reason: unknown) { message.error(reason instanceof Error ? reason.message : 'CSV indirilemedi.') } }
  return <section><PageHeader title="Zimmet Raporu" description="Aktif ve iade edilmiş zimmet kayıtları." actions={<Button icon={<DownloadOutlined />} onClick={() => void exportCsv()} type="primary">CSV İndir</Button>} /><ContentCard>{loading ? <LoadingState /> : error ? <ErrorState message={error} onRetry={() => void load()} /> : <Space direction="vertical" size="large" style={{ width: '100%' }}><Typography.Text type="secondary">{records.length} kayıt bulundu</Typography.Text><Table columns={columns} dataSource={records} locale={{ emptyText: <EmptyState /> }} pagination={{ pageSize: 10, showSizeChanger: true }} rowKey={(item) => `${item.assetCode}-${item.assignedAt}`} scroll={{ x: 1260 }} size="small" tableLayout="fixed" /></Space>}</ContentCard></section>
}
export default AssignmentReportPage
