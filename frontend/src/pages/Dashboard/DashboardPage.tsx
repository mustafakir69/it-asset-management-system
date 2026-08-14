import {
  DatabaseOutlined,
  DesktopOutlined,
  FileProtectOutlined,
  LaptopOutlined,
  SafetyCertificateOutlined,
  SolutionOutlined,
  ToolOutlined,
  WarningOutlined,
} from '@ant-design/icons'
import { Col, Flex, List, Row, Statistic, Typography } from 'antd'
import type { ReactNode } from 'react'
import { ContentCard, PageHeader, StatusTag } from '../../components'
import {
  criticalStockItems,
  dashboardSummaries,
  expiringWarranties,
  maintenanceTasks,
  recentDeviceMovements,
} from '../../mocks/dashboard'
import type { DashboardSummaryKey } from '../../types/dashboard'
import { formatDate } from '../../utils'
import './DashboardPage.css'

interface SummaryPresentation {
  color: string
  icon: ReactNode
}

const summaryPresentations: Record<DashboardSummaryKey, SummaryPresentation> = {
  totalDevices: { color: '#1677ff', icon: <DesktopOutlined /> },
  inStockDevices: { color: '#389e0d', icon: <DatabaseOutlined /> },
  assignedDevices: { color: '#0958d9', icon: <SolutionOutlined /> },
  maintenanceDevices: { color: '#d46b08', icon: <ToolOutlined /> },
  expiringWarranties: { color: '#d48806', icon: <SafetyCertificateOutlined /> },
  upcomingLicenseRenewals: { color: '#531dab', icon: <FileProtectOutlined /> },
  criticalStockItems: { color: '#cf1322', icon: <WarningOutlined /> },
  overdueMaintenanceTasks: { color: '#a8071a', icon: <LaptopOutlined /> },
}

const dateTimeOptions: Intl.DateTimeFormatOptions = {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
}

function DashboardPage() {
  return (
    <section className="dashboard-page">
      <PageHeader
        title="Dashboard"
        description="Donanım, stok, garanti, lisans ve bakım süreçlerinin güncel özeti."
        actions={<Typography.Text type="secondary">Son güncelleme: 13.08.2026 10:00</Typography.Text>}
      />

      <Row className="dashboard-summary-grid" gutter={[16, 16]}>
        {dashboardSummaries.map((summary) => {
          const presentation = summaryPresentations[summary.key]

          return (
            <Col key={summary.key} xs={24} sm={12} xl={6}>
              <ContentCard>
                <Statistic
                  prefix={<span style={{ color: presentation.color }}>{presentation.icon}</span>}
                  title={summary.title}
                  value={summary.value}
                  valueStyle={{ color: presentation.color }}
                />
              </ContentCard>
            </Col>
          )
        })}
      </Row>

      <Row className="dashboard-detail-grid" gutter={[16, 16]}>
        <Col xs={24} xl={12}>
          <ContentCard title="Son Cihaz Hareketleri">
            <List
              dataSource={recentDeviceMovements}
              renderItem={(movement) => (
                <List.Item className="dashboard-list-item">
                  <div className="dashboard-list-main">
                    <Flex align="center" gap={8} justify="space-between" wrap="wrap">
                      <Typography.Text strong>{movement.deviceName}</Typography.Text>
                      <StatusTag status={movement.status} />
                    </Flex>
                    <Typography.Text className="dashboard-item-code" type="secondary">
                      {movement.assetCode}
                    </Typography.Text>
                    <Typography.Text>{movement.description}</Typography.Text>
                  </div>
                  <Typography.Text className="dashboard-item-date" type="secondary">
                    {formatDate(movement.occurredAt, dateTimeOptions)}
                  </Typography.Text>
                </List.Item>
              )}
            />
          </ContentCard>
        </Col>

        <Col xs={24} xl={12}>
          <ContentCard title="Yaklaşan Garantiler">
            <List
              dataSource={expiringWarranties}
              renderItem={(warranty) => (
                <List.Item className="dashboard-list-item">
                  <div className="dashboard-list-main">
                    <Flex align="center" gap={8} justify="space-between" wrap="wrap">
                      <Typography.Text strong>{warranty.deviceName}</Typography.Text>
                      <StatusTag status={warranty.status} />
                    </Flex>
                    <Typography.Text className="dashboard-item-code" type="secondary">
                      {warranty.assetCode}
                    </Typography.Text>
                    <Typography.Text type="secondary">
                      Garanti bitişi: {formatDate(warranty.expiresAt)}
                    </Typography.Text>
                  </div>
                  <Typography.Text className="dashboard-item-countdown">
                    {warranty.remainingDays} gün kaldı
                  </Typography.Text>
                </List.Item>
              )}
            />
          </ContentCard>
        </Col>

        <Col xs={24} xl={12}>
          <ContentCard title="Kritik Stok Ürünleri">
            <List
              dataSource={criticalStockItems}
              renderItem={(stockItem) => (
                <List.Item className="dashboard-list-item">
                  <div className="dashboard-list-main">
                    <Typography.Text strong>{stockItem.productName}</Typography.Text>
                    <Typography.Text type="secondary">
                      Minimum seviye: {stockItem.minimumQuantity} {stockItem.unit}
                    </Typography.Text>
                  </div>
                  <div className="dashboard-stock-quantity">
                    <Typography.Text strong>{stockItem.currentQuantity}</Typography.Text>
                    <Typography.Text type="secondary"> {stockItem.unit}</Typography.Text>
                  </div>
                </List.Item>
              )}
            />
          </ContentCard>
        </Col>

        <Col xs={24} xl={12}>
          <ContentCard title="Yaklaşan / Geciken Bakım Görevleri">
            <List
              dataSource={maintenanceTasks}
              renderItem={(task) => (
                <List.Item className="dashboard-list-item">
                  <div className="dashboard-list-main">
                    <Flex align="center" gap={8} justify="space-between" wrap="wrap">
                      <Typography.Text strong>{task.taskName}</Typography.Text>
                      <StatusTag status={task.status} />
                    </Flex>
                    <Typography.Text className="dashboard-item-code" type="secondary">
                      {task.assetCode} · {task.deviceName}
                    </Typography.Text>
                    <Typography.Text type="secondary">
                      Planlanan tarih: {formatDate(task.dueDate)}
                    </Typography.Text>
                  </div>
                </List.Item>
              )}
            />
          </ContentCard>
        </Col>
      </Row>
    </section>
  )
}

export default DashboardPage
