import { ClearOutlined, EyeOutlined, MoreOutlined, SearchOutlined } from '@ant-design/icons'
import { Button, Col, Dropdown, Flex, Input, Row, Select, Space, Table, Tooltip, Typography } from 'antd'
import type { MenuProps, TableColumnsType, TablePaginationConfig } from 'antd'
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

  const getActionItems = (assignment: Assignment): MenuProps['items'] => [
    {
      key: 'view',
      icon: <EyeOutlined />,
      label: 'Görüntüle',
      onClick: () => void navigate(`/assignments/${assignment.id}`),
    },
  ]

  const columns: TableColumnsType<Assignment> = [
    {
      title: 'Varlık Kodu',
      dataIndex: 'assetCode',
      key: 'assetCode',
      width: 125,
      render: (assetCode: string) => <Typography.Text strong>{assetCode}</Typography.Text>,
    },
    {
      title: 'Cihaz',
      key: 'asset',
      width: 165,
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
      width: 145,
    },
    {
      title: 'Departman',
      dataIndex: 'department',
      key: 'department',
      ellipsis: true,
      responsive: ['lg'],
      width: 140,
    },
    {
      title: 'Zimmet Tarihi',
      dataIndex: 'assignedAt',
      key: 'assignedAt',
      width: 115,
      render: (assignedAt: string) => formatDate(assignedAt),
    },
    {
      title: 'İade Tarihi',
      dataIndex: 'returnedAt',
      key: 'returnedAt',
      width: 115,
      render: (returnedAt: string | null) => returnedAt ? formatDate(returnedAt) : '—',
    },
    {
      title: 'Durum',
      key: 'status',
      width: 110,
      render: (_value, assignment) => <StatusTag status={getAssignmentStatus(assignment)} />,
    },
    {
      title: 'Zimmetleyen',
      dataIndex: 'assignedBy',
      key: 'assignedBy',
      ellipsis: true,
      responsive: ['xl'],
      width: 130,
    },
    {
      title: 'İşlemler',
      key: 'actions',
      align: 'center',
      width: 72,
      render: (_value, assignment) => (
        <Dropdown
          menu={{ items: getActionItems(assignment) }}
          placement="bottomRight"
          trigger={['click']}
        >
          <Tooltip title="İşlemleri aç">
            <Button
              aria-label={`${assignment.assetCode} için işlemleri aç`}
              icon={<MoreOutlined />}
              size="small"
            />
          </Tooltip>
        </Dropdown>
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
              className="app-data-table"
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
              scroll={{ x: 1117 }}
              size="small"
              tableLayout="fixed"
            />
          </Space>
        )}
      </ContentCard>
    </section>
  )
}

export default AssignmentHistoryPage
