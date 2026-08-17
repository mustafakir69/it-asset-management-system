import { ClearOutlined, PlusOutlined, SearchOutlined } from '@ant-design/icons'
import { Button, Col, Input, Row, Select, Space, Table, Tabs, Typography } from 'antd'
import type { TableColumnsType } from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import {
  ContentCard,
  EmptyState,
  ErrorState,
  LoadingState,
  MaintenanceRequestActions,
  PageHeader,
  StatusTag,
} from '../../components'
import { useAuth } from '../../contexts/useAuth'
import { maintenanceService } from '../../services/maintenanceService'
import {
  maintenanceRequestPriorities,
  type MaintenanceRequest,
  type MaintenanceRequestPriority,
  type MaintenanceRequestStatus,
} from '../../types/maintenance'
import { formatDate } from '../../utils'
import './MaintenancePage.css'

type SupportView = 'all' | 'open' | 'assigned' | 'in-progress' | 'completed' | 'cancelled'

const supportViewStatuses: Record<SupportView, MaintenanceRequestStatus | null> = {
  all: null,
  open: 'Açık',
  assigned: 'Atandı',
  'in-progress': 'İşlemde',
  completed: 'Tamamlandı',
  cancelled: 'İptal Edildi',
}

const isSupportView = (value: string | null): value is SupportView =>
  value !== null && Object.hasOwn(supportViewStatuses, value)

const matchesSupportView = (
  request: MaintenanceRequest,
  view: SupportView,
): boolean => {
  const expectedStatus = supportViewStatuses[view]
  return expectedStatus === null || request.status === expectedStatus
}

const supportViewLabels: Record<SupportView, string> = {
  all: 'Tümü',
  open: 'Açık',
  assigned: 'Atandı',
  'in-progress': 'İşlemde',
  completed: 'Tamamlandı',
  cancelled: 'İptal Edildi',
}

function MaintenanceRequestsPage() {
  const navigate = useNavigate()
  const { user } = useAuth()
  const [searchParams, setSearchParams] = useSearchParams()
  const [requests, setRequests] = useState<MaintenanceRequest[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [search, setSearch] = useState('')
  const [priority, setPriority] = useState<MaintenanceRequestPriority>()
  const requestedView = searchParams.get('view')
  const supportView: SupportView = isSupportView(requestedView)
    ? requestedView
    : 'all'
  const isEmployee = user?.role === 'Employee'

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)

    try {
      const data = user?.role === 'Employee'
        ? await maintenanceService.getMyRequests()
        : await maintenanceService.getRequests()
      setRequests(data)
    } catch (reason: unknown) {
      setError(reason instanceof Error ? reason.message : 'Teknik destek talepleri yüklenemedi.')
    } finally {
      setLoading(false)
    }
  }, [user?.role])

  useEffect(() => {
    void load()
  }, [load])

  const supportCounts = useMemo(() => Object.fromEntries(
    (Object.keys(supportViewStatuses) as SupportView[]).map((view) => [
      view,
      requests.filter((request) => matchesSupportView(request, view)).length,
    ]),
  ) as Record<SupportView, number>, [requests])

  const filtered = useMemo(() => {
    const term = search.trim().toLocaleLowerCase('tr-TR')

    return requests.filter((item) => {
      const matchesSearch = !term || [
        item.assetCode,
        item.assetName,
        item.title,
        item.requestedByName,
        item.assignedToName ?? '',
      ].some((value) => value.toLocaleLowerCase('tr-TR').includes(term))

      return matchesSearch
        && matchesSupportView(item, supportView)
        && (!priority || item.priority === priority)
    })
  }, [priority, requests, search, supportView])

  const columns: TableColumnsType<MaintenanceRequest> = [
    {
      title: 'Talep No',
      dataIndex: 'requestNumber',
      width: 120,
      render: (value: string) => <Typography.Text strong>{value}</Typography.Text>,
    },
    {
      title: 'Cihaz',
      width: 180,
      render: (_value, item) => (
        <Space direction="vertical" size={0}>
          <Typography.Text>{item.assetCode}</Typography.Text>
          <Typography.Text type="secondary">{item.assetName}</Typography.Text>
        </Space>
      ),
    },
    { title: 'Konu', dataIndex: 'title', width: 190, ellipsis: true },
    {
      title: 'Öncelik',
      dataIndex: 'priority',
      width: 95,
      render: (value: MaintenanceRequestPriority) => <StatusTag status={value} />,
    },
    {
      title: 'Talebi Açan',
      dataIndex: 'requestedByName',
      width: 145,
      ellipsis: true,
      responsive: ['lg'],
    },
    {
      title: 'Atanan IT',
      dataIndex: 'assignedToName',
      width: 145,
      ellipsis: true,
      responsive: ['xl'],
      render: (value: string | null) => value ?? '—',
    },
    {
      title: 'Oluşturulma',
      dataIndex: 'createdAt',
      width: 120,
      render: (value: string) => formatDate(value),
    },
    {
      title: 'Durum',
      dataIndex: 'status',
      width: 115,
      render: (value: MaintenanceRequestStatus) => <StatusTag status={value} />,
    },
    {
      title: 'İşlemler',
      width: 75,
      align: 'center',
      render: (_value, item) => (
        <MaintenanceRequestActions onSuccess={() => void load()} request={item} />
      ),
    },
  ]

  const clearFilters = () => {
    setSearch('')
    setPriority(undefined)
    setSearchParams({})
  }

  const emptyDescription = supportView === 'all'
    ? isEmployee ? 'Teknik destek talebiniz bulunmuyor.' : 'Teknik destek talebi bulunmuyor.'
    : `${supportViewLabels[supportView]} durumunda teknik destek talebi bulunmuyor.`

  return (
    <section className="maintenance-page">
      <PageHeader
        title="Teknik Destek"
        description={isEmployee
          ? 'Kendi teknik destek taleplerinizi izleyin.'
          : 'Şirket teknik destek taleplerini yönetin.'}
        actions={isEmployee ? (
          <Button
            icon={<PlusOutlined />}
            onClick={() => void navigate('/support-requests/new')}
            type="primary"
          >
            Yeni Destek Talebi
          </Button>
        ) : undefined}
      />

      {loading ? (
        <ContentCard><LoadingState /></ContentCard>
      ) : error ? (
        <ContentCard><ErrorState message={error} onRetry={() => void load()} /></ContentCard>
      ) : (
        <ContentCard>
          <Space className="maintenance-table-content" direction="vertical" size="large">
            <Tabs
              activeKey={supportView}
              items={(Object.keys(supportViewStatuses) as SupportView[]).map((view) => ({
                key: view,
                label: `${supportViewLabels[view]} (${supportCounts[view]})`,
              }))}
              onChange={(key) => {
                if (isSupportView(key)) {
                  setSearchParams(key === 'all' ? {} : { view: key })
                }
              }}
            />

            <Row gutter={[12, 12]}>
              <Col xs={24} lg={17}>
                <Input
                  allowClear
                  onChange={(event) => setSearch(event.target.value)}
                  placeholder={isEmployee ? 'Cihaz veya konu ara' : 'Cihaz, konu, çalışan veya IT personeli ara'}
                  prefix={<SearchOutlined />}
                  value={search}
                />
              </Col>
              <Col xs={24} sm={12} lg={3}>
                <Select<MaintenanceRequestPriority>
                  allowClear
                  onChange={setPriority}
                  options={maintenanceRequestPriorities.map((value) => ({ label: value, value }))}
                  placeholder="Öncelik"
                  value={priority}
                />
              </Col>
              <Col xs={24} sm={12} lg={4}>
                <Button block icon={<ClearOutlined />} onClick={clearFilters}>
                  Filtreleri Temizle
                </Button>
              </Col>
            </Row>

            <Typography.Text type="secondary">{filtered.length} kayıt bulundu</Typography.Text>

            <Table<MaintenanceRequest>
              className="app-data-table"
              columns={columns}
              dataSource={filtered}
              locale={{ emptyText: <EmptyState description={emptyDescription} /> }}
              pagination={{
                pageSize: 10,
                pageSizeOptions: ['10', '20', '50'],
                showSizeChanger: true,
              }}
              rowKey="id"
              scroll={{ x: 1120 }}
              size="small"
              tableLayout="fixed"
            />
          </Space>
        </ContentCard>
      )}
    </section>
  )
}

export default MaintenanceRequestsPage
