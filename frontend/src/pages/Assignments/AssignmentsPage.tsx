import {
  ClearOutlined,
  CalendarOutlined,
  CheckCircleOutlined,
  EyeOutlined,
  PlusOutlined,
  RollbackOutlined,
  SearchOutlined,
} from '@ant-design/icons'
import { App as AntdApp, Button, Col, Flex, Input, Row, Select, Space, Table, Typography } from 'antd'
import type { TableColumnsType, TablePaginationConfig } from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ActionStatisticCard, ContentCard, EmptyState, ErrorState, LoadingState, PageHeader } from '../../components'
import { useAuth } from '../../contexts/useAuth'
import { assignmentService } from '../../services/assignmentService'
import type { Assignment, ReturnAssignmentInput } from '../../types/assignment'
import { formatDate } from '../../utils'
import './AssignmentsPage.css'
import ReturnAssignmentModal from './ReturnAssignmentModal'

interface AssignmentFilters {
  search: string
  department?: string
}

interface AssignmentPagination {
  current: number
  pageSize: number
}

const initialFilters: AssignmentFilters = {
  search: '',
}

function AssignmentsPage() {
  const navigate = useNavigate()
  const { message } = AntdApp.useApp()
  const { role } = useAuth()
  const canManageAssignments = role === 'Admin' || role === 'IT'
  const [assignments, setAssignments] = useState<Assignment[]>([])
  const [assignmentHistory, setAssignmentHistory] = useState<Assignment[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [filters, setFilters] = useState<AssignmentFilters>(initialFilters)
  const [pagination, setPagination] = useState<AssignmentPagination>({ current: 1, pageSize: 10 })
  const [selectedAssignment, setSelectedAssignment] = useState<Assignment | null>(null)
  const [isReturning, setIsReturning] = useState(false)

  const loadAssignments = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)

    try {
      const [activeAssignments, history] = await Promise.all([
        assignmentService.getAssignments(),
        assignmentService.getAssignmentHistory(),
      ])
      setAssignments(activeAssignments)
      setAssignmentHistory(history)
    } catch (error: unknown) {
      setLoadError(
        error instanceof Error ? error.message : 'Aktif zimmetler yüklenirken bir hata oluştu.',
      )
    } finally {
      setIsLoading(false)
    }
  }, [])

  const summaries = useMemo(() => {
    const now = new Date()
    const isCurrentMonth = (value: string) => {
      const date = new Date(value)
      return date.getFullYear() === now.getFullYear() && date.getMonth() === now.getMonth()
    }
    return {
      active: assignments.length,
      assignedThisMonth: assignmentHistory.filter((item) => isCurrentMonth(item.assignedAt)).length,
      returnedThisMonth: assignmentHistory.filter((item) => item.returnedAt && isCurrentMonth(item.returnedAt)).length,
    }
  }, [assignmentHistory, assignments])

  useEffect(() => {
    void loadAssignments()
  }, [loadAssignments])

  const departmentOptions = useMemo(
    () =>
      Array.from(new Set(assignments.map((assignment) => assignment.department)))
        .sort((first, second) => first.localeCompare(second, 'tr-TR'))
        .map((department) => ({ label: department, value: department })),
    [assignments],
  )

  const filteredAssignments = useMemo(() => {
    const normalizedSearch = filters.search.trim().toLocaleLowerCase('tr-TR')

    return assignments.filter((assignment) => {
      const matchesSearch =
        normalizedSearch.length === 0 ||
        [
          assignment.employeeName,
          assignment.assetCode,
          assignment.assetBrand,
          assignment.assetModel,
        ].some((value) => value.toLocaleLowerCase('tr-TR').includes(normalizedSearch))

      return matchesSearch &&
        (!filters.department || assignment.department === filters.department)
    })
  }, [assignments, filters])

  const updateFilters = (nextFilters: Partial<AssignmentFilters>) => {
    setFilters((current) => ({ ...current, ...nextFilters }))
    setPagination((current) => ({ ...current, current: 1 }))
  }

  const clearFilters = () => {
    setFilters(initialFilters)
    setPagination((current) => ({ ...current, current: 1 }))
  }

  const handlePaginationChange = (nextPagination: TablePaginationConfig) => {
    setPagination({
      current: nextPagination.current ?? 1,
      pageSize: nextPagination.pageSize ?? 10,
    })
  }

  const handleReturnAssignment = async (input: ReturnAssignmentInput) => {
    if (!selectedAssignment) {
      return
    }

    setIsReturning(true)

    try {
      await assignmentService.returnAssignment(selectedAssignment.id, input)
      message.success('Cihaz iadesi başarıyla tamamlandı.')
      setSelectedAssignment(null)
      setPagination((current) => ({ ...current, current: 1 }))
      await loadAssignments()
    } catch (error: unknown) {
      message.error(error instanceof Error ? error.message : 'Cihaz iadesi tamamlanamadı.')
    } finally {
      setIsReturning(false)
    }
  }

  const columns: TableColumnsType<Assignment> = [
    {
      title: 'Varlık Kodu',
      dataIndex: 'assetCode',
      key: 'assetCode',
      width: 130,
      render: (assetCode: string) => <Typography.Text strong>{assetCode}</Typography.Text>,
    },
    {
      title: 'Cihaz',
      key: 'asset',
      width: 175,
      render: (_value, assignment) => (
        <Flex vertical>
          <Typography.Text strong>{assignment.assetBrand}</Typography.Text>
          <Typography.Text type="secondary">{assignment.assetModel}</Typography.Text>
        </Flex>
      ),
    },
    {
      title: 'Çalışan',
      dataIndex: 'employeeName',
      key: 'employeeName',
      ellipsis: true,
      width: 150,
    },
    {
      title: 'Departman',
      dataIndex: 'department',
      key: 'department',
      ellipsis: true,
      responsive: ['lg'],
      width: 145,
    },
    {
      title: 'Zimmet Tarihi',
      dataIndex: 'assignedAt',
      key: 'assignedAt',
      width: 120,
      render: (assignedAt: string) => formatDate(assignedAt),
    },
    {
      title: 'Zimmetleyen',
      dataIndex: 'assignedByName',
      key: 'assignedByName',
      ellipsis: true,
      responsive: ['xl'],
      width: 135,
    },
    {
      title: 'İşlemler',
      key: 'actions',
      width: 165,
      render: (_value, assignment) => (
        <Space size={0}>
          <Button
            icon={<EyeOutlined />}
            onClick={() => void navigate(`/assignments/${assignment.id}`)}
            size="small"
            type="link"
          >
            Görüntüle
          </Button>
          {canManageAssignments && (
            <Button
              icon={<RollbackOutlined />}
              onClick={() => setSelectedAssignment(assignment)}
              size="small"
              type="link"
            >
              İade Al
            </Button>
          )}
        </Space>
      ),
    },
  ]

  return (
    <section className="assignments-page">
      <PageHeader
        title="Aktif Zimmetler"
        description="Çalışanlara teslim edilmiş ve henüz iade alınmamış cihazları görüntüleyin."
        actions={canManageAssignments ? (
          <Button
            icon={<PlusOutlined />}
            onClick={() => void navigate('/assignments/new')}
            type="primary"
          >
            Yeni Zimmet
          </Button>
        ) : undefined}
      />

      {!isLoading && !loadError && (
        <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
          <Col xs={24} md={8}><ActionStatisticCard color="#0958d9" icon={<CheckCircleOutlined />} title="Aktif Zimmet" value={summaries.active} /></Col>
          <Col xs={24} md={8}><ActionStatisticCard color="#389e0d" icon={<CalendarOutlined />} title="Bu Ay Verilen" value={summaries.assignedThisMonth} /></Col>
          <Col xs={24} md={8}><ActionStatisticCard color="#d46b08" icon={<RollbackOutlined />} title="Bu Ay İade Alınan" value={summaries.returnedThisMonth} /></Col>
        </Row>
      )}

      <ContentCard>
        {loadError ? (
          <ErrorState message={loadError} onRetry={() => void loadAssignments()} />
        ) : isLoading ? (
          <LoadingState message="Aktif zimmetler yükleniyor..." />
        ) : (
          <Space className="assignments-content" direction="vertical" size="large">
            <Row gutter={[12, 12]}>
              <Col xs={24} md={12} lg={10}>
                <Input
                  allowClear
                  aria-label="Aktif zimmetlerde ara"
                  onChange={(event) => updateFilters({ search: event.target.value })}
                  placeholder="Çalışan, varlık kodu, marka veya model ara"
                  prefix={<SearchOutlined />}
                  value={filters.search}
                />
              </Col>
              <Col xs={24} sm={12} md={6} lg={7}>
                <Select<string>
                  allowClear
                  aria-label="Departman filtresi"
                  onChange={(department) => updateFilters({ department })}
                  options={departmentOptions}
                  placeholder="Departman"
                  value={filters.department}
                />
              </Col>
              <Col xs={24} sm={12} md={6} lg={7}>
                <Button block icon={<ClearOutlined />} onClick={clearFilters}>
                  Filtreleri Temizle
                </Button>
              </Col>
            </Row>

            <Typography.Text type="secondary">
              {filteredAssignments.length} aktif zimmet bulundu
            </Typography.Text>

            <Table<Assignment>
              className="app-data-table"
              columns={columns}
              dataSource={filteredAssignments}
              locale={{
                emptyText: <EmptyState description="Filtrelere uygun aktif zimmet bulunamadı." />,
              }}
              onChange={handlePaginationChange}
              pagination={{
                current: pagination.current,
                pageSize: pagination.pageSize,
                pageSizeOptions: ['10', '20', '50'],
                showSizeChanger: true,
                showTotal: (total) => `Toplam ${total} aktif zimmet`,
              }}
              rowKey="id"
              scroll={{ x: 1020 }}
              size="small"
              tableLayout="fixed"
            />
          </Space>
        )}
      </ContentCard>

      <ReturnAssignmentModal
        assignment={selectedAssignment}
        isSubmitting={isReturning}
        onCancel={() => setSelectedAssignment(null)}
        onSubmit={handleReturnAssignment}
      />
    </section>
  )
}

export default AssignmentsPage
