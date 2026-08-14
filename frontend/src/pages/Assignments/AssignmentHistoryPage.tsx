import { ClearOutlined, EyeOutlined, SearchOutlined } from '@ant-design/icons'
import { Button, Col, Flex, Input, Row, Select, Space, Table, Typography } from 'antd'
import type { TableColumnsType, TablePaginationConfig } from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  ContentCard,
  EmptyState,
  ErrorState,
  LoadingState,
  PageHeader,
  StatusTag,
} from '../../components'
import { assignmentService } from '../../services/assignmentService'
import type { Assignment, AssignmentStatus } from '../../types/assignment'
import { formatDate } from '../../utils'
import './AssignmentHistoryPage.css'

interface HistoryFilters {
  search: string
  department?: string
  status?: AssignmentStatus
}

interface HistoryPagination {
  current: number
  pageSize: number
}

const initialFilters: HistoryFilters = {
  search: '',
}

const statusOptions: Array<{ label: AssignmentStatus; value: AssignmentStatus }> = [
  { label: 'Aktif', value: 'Aktif' },
  { label: 'İade Edildi', value: 'İade Edildi' },
]

const getAssignmentStatus = (assignment: Assignment): AssignmentStatus =>
  assignment.returnedAt === null ? 'Aktif' : 'İade Edildi'

function AssignmentHistoryPage() {
  const navigate = useNavigate()
  const [assignments, setAssignments] = useState<Assignment[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [filters, setFilters] = useState<HistoryFilters>(initialFilters)
  const [pagination, setPagination] = useState<HistoryPagination>({ current: 1, pageSize: 10 })

  const loadHistory = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)

    try {
      const history = await assignmentService.getAssignmentHistory()
      setAssignments(history)
    } catch {
      setLoadError('Zimmet geçmişi yüklenirken bir hata oluştu.')
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadHistory()
  }, [loadHistory])

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

      return (
        matchesSearch &&
        (!filters.department || assignment.department === filters.department) &&
        (!filters.status || getAssignmentStatus(assignment) === filters.status)
      )
    })
  }, [assignments, filters])

  const updateFilters = (nextFilters: Partial<HistoryFilters>) => {
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

  const columns: TableColumnsType<Assignment> = [
    {
      title: 'Varlık Kodu',
      dataIndex: 'assetCode',
      key: 'assetCode',
      width: 150,
      render: (assetCode: string) => <Typography.Text strong>{assetCode}</Typography.Text>,
    },
    {
      title: 'Cihaz',
      key: 'asset',
      width: 220,
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
      width: 190,
    },
    {
      title: 'Departman',
      dataIndex: 'department',
      key: 'department',
      width: 190,
    },
    {
      title: 'Zimmet Tarihi',
      dataIndex: 'assignedAt',
      key: 'assignedAt',
      width: 150,
      render: (assignedAt: string) => formatDate(assignedAt),
    },
    {
      title: 'İade Tarihi',
      dataIndex: 'returnedAt',
      key: 'returnedAt',
      width: 145,
      render: (returnedAt: string | null) => returnedAt ? formatDate(returnedAt) : '—',
    },
    {
      title: 'Durum',
      key: 'status',
      width: 130,
      render: (_value, assignment) => <StatusTag status={getAssignmentStatus(assignment)} />,
    },
    {
      title: 'Zimmetleyen',
      dataIndex: 'assignedBy',
      key: 'assignedBy',
      width: 170,
    },
    {
      title: 'İşlemler',
      key: 'actions',
      fixed: 'right',
      width: 120,
      render: (_value, assignment) => (
        <Button
          icon={<EyeOutlined />}
          onClick={() => void navigate(`/assignments/${assignment.id}`)}
          size="small"
          type="link"
        >
          Görüntüle
        </Button>
      ),
    },
  ]

  return (
    <section className="assignment-history-page">
      <PageHeader
        title="Zimmet Geçmişi"
        description="Aktif ve iade edilmiş tüm zimmet kayıtlarını görüntüleyin."
      />

      <ContentCard>
        {loadError ? (
          <ErrorState message={loadError} onRetry={() => void loadHistory()} />
        ) : isLoading ? (
          <LoadingState message="Zimmet geçmişi yükleniyor..." />
        ) : (
          <Space className="assignment-history-content" direction="vertical" size="large">
            <Row gutter={[12, 12]}>
              <Col xs={24} lg={8}>
                <Input
                  allowClear
                  aria-label="Zimmet geçmişinde ara"
                  onChange={(event) => updateFilters({ search: event.target.value })}
                  placeholder="Çalışan, varlık kodu, marka veya model ara"
                  prefix={<SearchOutlined />}
                  value={filters.search}
                />
              </Col>
              <Col xs={24} sm={12} lg={5}>
                <Select<string>
                  allowClear
                  aria-label="Departman filtresi"
                  onChange={(department) => updateFilters({ department })}
                  options={departmentOptions}
                  placeholder="Departman"
                  value={filters.department}
                />
              </Col>
              <Col xs={24} sm={12} lg={5}>
                <Select<AssignmentStatus>
                  allowClear
                  aria-label="Zimmet durumu filtresi"
                  onChange={(status) => updateFilters({ status })}
                  options={statusOptions}
                  placeholder="Durum"
                  value={filters.status}
                />
              </Col>
              <Col xs={24} lg={6}>
                <Button block icon={<ClearOutlined />} onClick={clearFilters}>
                  Filtreleri Temizle
                </Button>
              </Col>
            </Row>

            <Typography.Text type="secondary">
              {filteredAssignments.length} zimmet kaydı bulundu
            </Typography.Text>

            <Table<Assignment>
              columns={columns}
              dataSource={filteredAssignments}
              locale={{
                emptyText: <EmptyState description="Filtrelere uygun zimmet kaydı bulunamadı." />,
              }}
              onChange={handlePaginationChange}
              pagination={{
                current: pagination.current,
                pageSize: pagination.pageSize,
                pageSizeOptions: ['10', '20', '50'],
                showSizeChanger: true,
                showTotal: (total) => `Toplam ${total} zimmet kaydı`,
              }}
              rowKey="id"
              scroll={{ x: 1490 }}
            />
          </Space>
        )}
      </ContentCard>
    </section>
  )
}

export default AssignmentHistoryPage
