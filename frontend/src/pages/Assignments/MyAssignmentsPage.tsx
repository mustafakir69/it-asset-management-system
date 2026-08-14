import { EyeOutlined } from '@ant-design/icons'
import { Button, Flex, Space, Table, Typography } from 'antd'
import type { TableColumnsType, TablePaginationConfig } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ContentCard, EmptyState, ErrorState, LoadingState, PageHeader, StatusTag } from '../../components'
import { assignmentService } from '../../services/assignmentService'
import type { Assignment } from '../../types/assignment'
import { formatDate } from '../../utils'
import './AssignmentsPage.css'

interface AssignmentPagination {
  current: number
  pageSize: number
}

function MyAssignmentsPage() {
  const navigate = useNavigate()
  const [assignments, setAssignments] = useState<Assignment[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [pagination, setPagination] = useState<AssignmentPagination>({ current: 1, pageSize: 10 })

  const loadAssignments = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)
    try {
      setAssignments(await assignmentService.getMyAssignments())
    } catch (error: unknown) {
      setLoadError(error instanceof Error ? error.message : 'Zimmetleriniz yüklenemedi.')
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadAssignments()
  }, [loadAssignments])

  const columns: TableColumnsType<Assignment> = [
    {
      title: 'Varlık Kodu',
      dataIndex: 'assetCode',
      key: 'assetCode',
      width: 140,
      render: (value: string) => <Typography.Text strong>{value}</Typography.Text>,
    },
    {
      title: 'Cihaz',
      key: 'asset',
      width: 220,
      render: (_value, assignment) => (
        <Flex vertical>
          <Typography.Text strong>{assignment.assetName}</Typography.Text>
          <Typography.Text type="secondary">{assignment.assetCategory}</Typography.Text>
        </Flex>
      ),
    },
    {
      title: 'Zimmet Tarihi',
      dataIndex: 'assignedAt',
      key: 'assignedAt',
      width: 140,
      render: (value: string) => formatDate(value),
    },
    {
      title: 'Durum',
      key: 'status',
      width: 110,
      render: () => <StatusTag status="Aktif" />,
    },
    {
      title: 'İşlemler',
      key: 'actions',
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

  const handlePaginationChange = (nextPagination: TablePaginationConfig) => {
    setPagination({
      current: nextPagination.current ?? 1,
      pageSize: nextPagination.pageSize ?? 10,
    })
  }

  return (
    <section className="assignments-page">
      <PageHeader
        title="Zimmetlerim"
        description="Size zimmetlenmiş aktif cihazları görüntüleyin."
      />
      <ContentCard>
        {isLoading ? (
          <LoadingState message="Zimmetleriniz yükleniyor..." />
        ) : loadError ? (
          <ErrorState message={loadError} onRetry={() => void loadAssignments()} />
        ) : (
          <Space className="assignments-content" direction="vertical" size="large">
            <Typography.Text type="secondary">
              {assignments.length} aktif zimmet bulundu
            </Typography.Text>
            <Table<Assignment>
              className="app-data-table"
              columns={columns}
              dataSource={assignments}
              locale={{ emptyText: <EmptyState description="Aktif zimmetiniz bulunmuyor." /> }}
              onChange={handlePaginationChange}
              pagination={{
                current: pagination.current,
                pageSize: pagination.pageSize,
                pageSizeOptions: ['10', '20', '50'],
                showSizeChanger: true,
                showTotal: (total) => `Toplam ${total} aktif zimmet`,
              }}
              rowKey="id"
              scroll={{ x: 730 }}
              size="small"
              tableLayout="fixed"
            />
          </Space>
        )}
      </ContentCard>
    </section>
  )
}

export default MyAssignmentsPage
