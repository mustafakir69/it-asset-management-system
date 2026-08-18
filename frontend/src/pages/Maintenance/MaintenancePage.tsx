import { PlusOutlined } from '@ant-design/icons'
import { Button, Col, Row, Space, Table, Typography } from 'antd'
import type { TableColumnsType } from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import {
  ActionStatisticCard,
  ContentCard,
  EmptyState,
  ErrorState,
  LoadingState,
  MaintenanceTaskActions,
  PageHeader,
  StatusTag,
} from '../../components'
import { maintenanceService } from '../../services/maintenanceService'
import type { MaintenancePlan, MaintenanceTask } from '../../types/maintenance'
import { formatDate } from '../../utils'
import './MaintenancePage.css'

type MaintenanceView = 'upcoming' | 'overdue' | 'completed' | 'plans'

const isMaintenanceView = (value: string | null): value is MaintenanceView =>
  value === 'upcoming' || value === 'overdue' || value === 'completed' || value === 'plans'

const emptyDescriptions: Record<MaintenanceView, string> = {
  upcoming: 'Yaklaşan veya planlanmış bakım görevi bulunmuyor.',
  overdue: 'Gecikmiş bakım görevi bulunmuyor.',
  completed: 'Tamamlanmış bakım görevi bulunmuyor.',
  plans: 'Bakım planı bulunmuyor.',
}

function MaintenancePage() {
  const navigate = useNavigate()
  const [searchParams, setSearchParams] = useSearchParams()
  const [plans, setPlans] = useState<MaintenancePlan[]>([])
  const [tasks, setTasks] = useState<MaintenanceTask[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const requestedView = searchParams.get('view')
  const activeView: MaintenanceView = isMaintenanceView(requestedView) ? requestedView : 'upcoming'

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)

    try {
      const [planData, taskData] = await Promise.all([
        maintenanceService.getPlans(),
        maintenanceService.getTasks(),
      ])
      setPlans(planData)
      setTasks(taskData)
    } catch (reason: unknown) {
      setError(reason instanceof Error ? reason.message : 'Periyodik bakım verileri yüklenemedi.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  const taskColumns: TableColumnsType<MaintenanceTask> = [
    {
      title: 'Cihaz',
      width: 190,
      render: (_value, item) => (
        <Space direction="vertical" size={0}>
          <Typography.Text strong>{item.assetCode}</Typography.Text>
          <Typography.Text type="secondary">{item.assetName}</Typography.Text>
        </Space>
      ),
    },
    { title: 'Bakım', dataIndex: 'title', ellipsis: true },
    {
      title: 'Planlanan Tarih',
      dataIndex: 'plannedDate',
      width: 135,
      render: (value: string) => formatDate(value),
    },
    { title: 'Sorumlu IT', dataIndex: 'responsibleUserName', width: 155, ellipsis: true },
    {
      title: 'Durum',
      dataIndex: 'displayStatus',
      width: 115,
      render: (value: MaintenanceTask['displayStatus']) => <StatusTag status={value} />,
    },
    {
      title: 'İşlemler',
      width: 75,
      align: 'center',
      render: (_value, item) => (
        <MaintenanceTaskActions onSuccess={() => void load()} task={item} />
      ),
    },
  ]

  const planColumns: TableColumnsType<MaintenancePlan> = [
    {
      title: 'Cihaz',
      width: 190,
      render: (_value, item) => `${item.assetCode} · ${item.assetName}`,
    },
    { title: 'Plan Adı', dataIndex: 'name', ellipsis: true },
    { title: 'Sorumlu IT', dataIndex: 'responsibleUserName', width: 150 },
    {
      title: 'Sıklık',
      dataIndex: 'frequencyDays',
      width: 90,
      render: (value: number) => `${value} gün`,
    },
    {
      title: 'Sonraki Bakım',
      dataIndex: 'nextDueAt',
      width: 125,
      render: (value: string) => formatDate(value),
    },
    {
      title: 'Durum',
      dataIndex: 'isActive',
      width: 90,
      render: (value: boolean) => <StatusTag status={value ? 'Aktif' : 'Pasif'} />,
    },
  ]

  const taskSets = useMemo(
    () => ({
      upcoming: tasks.filter(
        (item) => item.displayStatus === 'Yaklaşıyor' || item.displayStatus === 'Planlandı',
      ),
      overdue: tasks.filter((item) => item.displayStatus === 'Gecikti'),
      completed: tasks.filter((item) => item.status === 'Tamamlandı'),
    }),
    [tasks],
  )

  const taskTable = (view: Exclude<MaintenanceView, 'plans'>) => (
    <Table<MaintenanceTask>
      columns={taskColumns}
      dataSource={taskSets[view]}
      locale={{ emptyText: <EmptyState description={emptyDescriptions[view]} /> }}
      pagination={{ pageSize: 10 }}
      rowKey="id"
      scroll={{ x: 930 }}
      size="small"
      tableLayout="fixed"
    />
  )

  const changeView = (key: string) => {
    if (isMaintenanceView(key)) setSearchParams(key === 'upcoming' ? {} : { view: key })
  }

  return (
    <section className="maintenance-page">
      <PageHeader
        title="Periyodik Bakım"
        description="Planlı ve tekrarlayan IT bakım süreçlerini yönetin."
        actions={(
          <Button
            icon={<PlusOutlined />}
            onClick={() => void navigate('/maintenance/plans/new')}
            type="primary"
          >
            Yeni Bakım Planı
          </Button>
        )}
      />

      {loading ? (
        <ContentCard><LoadingState /></ContentCard>
      ) : error ? (
        <ContentCard><ErrorState message={error} onRetry={() => void load()} /></ContentCard>
      ) : (
        <Space direction="vertical" size="large" style={{ width: '100%' }}>
          <Row gutter={[16, 16]}>
            <Col xs={24} sm={12} xl={6}>
              <ActionStatisticCard active={activeView === 'upcoming'} onClick={() => changeView('upcoming')} title="Yaklaşan" value={taskSets.upcoming.length} />
            </Col>
            <Col xs={24} sm={12} xl={6}>
              <ActionStatisticCard active={activeView === 'overdue'} onClick={() => changeView('overdue')} title="Geciken" value={taskSets.overdue.length} />
            </Col>
            <Col xs={24} sm={12} xl={6}>
              <ActionStatisticCard active={activeView === 'completed'} onClick={() => changeView('completed')} title="Tamamlanan" value={taskSets.completed.length} />
            </Col>
            <Col xs={24} sm={12} xl={6}>
              <ActionStatisticCard active={activeView === 'plans'} onClick={() => changeView('plans')} title="Planlar" value={plans.length} />
            </Col>
          </Row>

          <ContentCard>
            {activeView === 'plans' ? (
              <Table<MaintenancePlan>
                columns={planColumns}
                dataSource={plans}
                locale={{ emptyText: <EmptyState description={emptyDescriptions.plans} /> }}
                pagination={{ pageSize: 10 }}
                rowKey="id"
                scroll={{ x: 850 }}
                size="small"
                tableLayout="fixed"
              />
            ) : taskTable(activeView)}
          </ContentCard>
        </Space>
      )}
    </section>
  )
}

export default MaintenancePage
