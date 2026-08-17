import { DatabaseOutlined, DesktopOutlined, FileProtectOutlined, LaptopOutlined, SafetyCertificateOutlined, SolutionOutlined, ToolOutlined, WarningOutlined } from '@ant-design/icons'
import { Col, Flex, List, Row, Statistic, Typography } from 'antd'
import type { ReactNode } from 'react'
import { useCallback, useEffect, useState } from 'react'
import { ContentCard, EmptyState, ErrorState, LoadingState, PageHeader, StatusTag } from '../../components'
import { useAuth } from '../../contexts/useAuth'
import { dashboardService } from '../../services/dashboardService'
import type { DashboardSummary, DashboardSummaryKey, EmployeeDashboardSummary } from '../../types/dashboard'
import { formatDate } from '../../utils'
import './DashboardPage.css'

interface SummaryPresentation { color: string; icon: ReactNode; title: string }
const summaryPresentations: Record<DashboardSummaryKey, SummaryPresentation> = {
  totalAssets: { color: '#1677ff', icon: <DesktopOutlined />, title: 'Toplam Cihaz' },
  inStockAssets: { color: '#389e0d', icon: <DatabaseOutlined />, title: 'Stoktaki Cihaz' },
  assignedAssets: { color: '#0958d9', icon: <SolutionOutlined />, title: 'Zimmetli Cihaz' },
  maintenanceAssets: { color: '#d46b08', icon: <ToolOutlined />, title: 'Bakımdaki Cihaz' },
  expiringWarranties: { color: '#d48806', icon: <SafetyCertificateOutlined />, title: 'Yaklaşan Garantiler' },
  expiringLicenses: { color: '#531dab', icon: <FileProtectOutlined />, title: 'Yaklaşan Lisanslar' },
  criticalStockItems: { color: '#cf1322', icon: <WarningOutlined />, title: 'Kritik Stok' },
  overdueMaintenanceTasks: { color: '#a8071a', icon: <LaptopOutlined />, title: 'Geciken Bakımlar' },
  openMaintenanceRequests: { color: '#c41d7f', icon: <ToolOutlined />, title: 'Açık Bakım Talepleri' },
}
const summaryKeys = Object.keys(summaryPresentations) as DashboardSummaryKey[]
const dateTimeOptions: Intl.DateTimeFormatOptions = { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }

function CompanyDashboard() {
  const [summary, setSummary] = useState<DashboardSummary | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const load = useCallback(async () => {
    setLoading(true); setError(null)
    try { setSummary(await dashboardService.getSummary()) }
    catch (loadError: unknown) { setError(loadError instanceof Error ? loadError.message : 'Dashboard yüklenemedi.') }
    finally { setLoading(false) }
  }, [])
  useEffect(() => { void load() }, [load])

  if (loading) return <LoadingState message="Dashboard verileri yükleniyor..." />
  if (error) return <ErrorState message={error} onRetry={() => void load()} />
  if (!summary) return <EmptyState description="Dashboard verisi bulunamadı." />

  return <section className="dashboard-page">
    <PageHeader title="Dashboard" description="Donanım, stok, garanti, lisans ve bakım süreçlerinin güncel özeti." actions={<Typography.Text type="secondary">Son güncelleme: {formatDate(summary.generatedAt, dateTimeOptions)}</Typography.Text>} />
    <Row className="dashboard-summary-grid" gutter={[16, 16]}>{summaryKeys.map((key) => { const item = summaryPresentations[key]; return <Col key={key} xs={24} sm={12} xl={6}><ContentCard><Statistic prefix={<span style={{ color: item.color }}>{item.icon}</span>} title={item.title} value={summary[key]} valueStyle={{ color: item.color }} /></ContentCard></Col> })}</Row>
    <Row className="dashboard-detail-grid" gutter={[16, 16]}>
      <Col xs={24} xl={12}><ContentCard title="Son Cihaz / Zimmet Hareketleri"><List dataSource={summary.recentMovements} locale={{ emptyText: 'Hareket bulunamadı.' }} renderItem={(item) => <List.Item className="dashboard-list-item"><div className="dashboard-list-main"><Flex align="center" gap={8} justify="space-between" wrap="wrap"><Typography.Text strong>{item.assetName}</Typography.Text><StatusTag status={item.status} /></Flex><Typography.Text className="dashboard-item-code" type="secondary">{item.assetCode}</Typography.Text><Typography.Text>{item.description}</Typography.Text></div><Typography.Text className="dashboard-item-date" type="secondary">{formatDate(item.occurredAt, dateTimeOptions)}</Typography.Text></List.Item>} /></ContentCard></Col>
      <Col xs={24} xl={12}><ContentCard title="Yaklaşan Garantiler"><List dataSource={summary.upcomingWarranties} locale={{ emptyText: '30 gün içinde bitecek garanti bulunmuyor.' }} renderItem={(item) => <List.Item className="dashboard-list-item"><div className="dashboard-list-main"><Flex align="center" gap={8} justify="space-between" wrap="wrap"><Typography.Text strong>{item.assetName}</Typography.Text><StatusTag status={item.status} /></Flex><Typography.Text className="dashboard-item-code" type="secondary">{item.assetCode}</Typography.Text><Typography.Text type="secondary">Garanti bitişi: {formatDate(item.warrantyEndDate)}</Typography.Text></div><Typography.Text className="dashboard-item-countdown">{item.remainingDays} gün kaldı</Typography.Text></List.Item>} /></ContentCard></Col>
      <Col xs={24} xl={12}><ContentCard title="Kritik Stok Ürünleri"><List dataSource={summary.criticalStock} locale={{ emptyText: 'Kritik stok ürünü bulunmuyor.' }} renderItem={(item) => <List.Item className="dashboard-list-item"><div className="dashboard-list-main"><Typography.Text strong>{item.itemName}</Typography.Text><Typography.Text type="secondary">{item.itemCode} · Minimum: {item.minimumQuantity} {item.unit} · {item.location}</Typography.Text></div><div className="dashboard-stock-quantity"><Typography.Text strong>{item.currentQuantity}</Typography.Text><Typography.Text type="secondary"> {item.unit}</Typography.Text></div></List.Item>} /></ContentCard></Col>
      <Col xs={24} xl={12}><ContentCard title="Yaklaşan / Geciken Bakımlar"><List dataSource={summary.upcomingMaintenance} locale={{ emptyText: 'Yaklaşan veya geciken bakım bulunmuyor.' }} renderItem={(item) => <List.Item className="dashboard-list-item"><div className="dashboard-list-main"><Flex align="center" gap={8} justify="space-between" wrap="wrap"><Typography.Text strong>{item.title}</Typography.Text><StatusTag status={item.status} /></Flex><Typography.Text className="dashboard-item-code" type="secondary">{item.assetCode} · {item.assetName}</Typography.Text><Typography.Text type="secondary">Planlanan tarih: {formatDate(item.plannedDate)}</Typography.Text></div></List.Item>} /></ContentCard></Col>
    </Row>
  </section>
}

function EmployeeDashboard() {
  const [summary, setSummary] = useState<EmployeeDashboardSummary | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const load = useCallback(async () => {
    setLoading(true); setError(null)
    try { setSummary(await dashboardService.getMySummary()) }
    catch (loadError: unknown) { setError(loadError instanceof Error ? loadError.message : 'Dashboard yüklenemedi.') }
    finally { setLoading(false) }
  }, [])
  useEffect(() => { void load() }, [load])
  if (loading) return <LoadingState message="Kişisel dashboard yükleniyor..." />
  if (error) return <ErrorState message={error} onRetry={() => void load()} />
  if (!summary) return <EmptyState description="Kişisel dashboard verisi bulunamadı." />
  const warranty = summary.myAssetsWarrantySummary
  return <section className="dashboard-page">
    <PageHeader title="Dashboard" description="Zimmetli cihazlarınız ve teknik destek talepleriniz." />
    <Row className="dashboard-summary-grid" gutter={[16, 16]}>
      <Col xs={24} sm={12} xl={6}><ContentCard><Statistic title="Zimmetli Cihazlarım" value={summary.activeAssignmentCount} /></ContentCard></Col>
      <Col xs={24} sm={12} xl={6}><ContentCard><Statistic title="Açık Destek Talebim" value={summary.openSupportRequestCount} /></ContentCard></Col>
      <Col xs={24} sm={12} xl={6}><ContentCard><Statistic title="İşlemdeki Talebim" value={summary.inProgressSupportRequestCount} /></ContentCard></Col>
      <Col xs={24} sm={12} xl={6}><ContentCard><Statistic title="Yaklaşan Garanti" value={warranty.expiringSoon} /></ContentCard></Col>
    </Row>
    <Row gutter={[16, 16]}>
      <Col xs={24} xl={12}><ContentCard title="Cihazlarım"><List dataSource={summary.myAssets} locale={{ emptyText: 'Aktif zimmetli cihazınız bulunmuyor.' }} renderItem={(item) => <List.Item><div><Typography.Text strong>{item.assetCode} · {item.assetName}</Typography.Text><br /><Typography.Text type="secondary">{item.category} · Zimmet: {formatDate(item.assignedAt)}</Typography.Text></div></List.Item>} /></ContentCard></Col>
      <Col xs={24} xl={12}><ContentCard title="Son Teknik Destek Taleplerim"><List dataSource={summary.recentSupportRequests} locale={{ emptyText: 'Destek talebiniz bulunmuyor.' }} renderItem={(item) => <List.Item><div><Typography.Text strong>{item.requestNumber} · {item.title}</Typography.Text><br /><Typography.Text type="secondary">{item.status} · {formatDate(item.updatedAt)}</Typography.Text></div></List.Item>} /></ContentCard></Col>
    </Row>
  </section>
}

function DashboardPage() {
  const { user } = useAuth()
  return user?.role === 'Employee' ? <EmployeeDashboard /> : <CompanyDashboard />
}

export default DashboardPage
