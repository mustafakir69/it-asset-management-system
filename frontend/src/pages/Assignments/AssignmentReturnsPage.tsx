import { RollbackOutlined } from '@ant-design/icons'
import { App as AntdApp, Button, Flex, Space, Table, Typography } from 'antd'
import type { TableColumnsType, TablePaginationConfig } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { ContentCard, EmptyState, ErrorState, LoadingState, PageHeader } from '../../components'
import { assignmentService } from '../../services/assignmentService'
import type { Assignment, ReturnAssignmentInput } from '../../types/assignment'
import { formatDate } from '../../utils'
import './AssignmentsPage.css'
import ReturnAssignmentModal from './ReturnAssignmentModal'

interface AssignmentPagination {
  current: number
  pageSize: number
}

function AssignmentReturnsPage() {
  const { message } = AntdApp.useApp()
  const [assignments, setAssignments] = useState<Assignment[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [pagination, setPagination] = useState<AssignmentPagination>({ current: 1, pageSize: 10 })
  const [selectedAssignment, setSelectedAssignment] = useState<Assignment | null>(null)
  const [isReturning, setIsReturning] = useState(false)

  const loadAssignments = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)

    try {
      setAssignments(await assignmentService.getActiveAssignments())
    } catch (error: unknown) {
      setLoadError(
        error instanceof Error
          ? error.message
          : 'İade alınabilecek aktif zimmetler yüklenirken bir hata oluştu.',
      )
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadAssignments()
  }, [loadAssignments])

  const handleReturnAssignment = async (input: ReturnAssignmentInput) => {
    if (!selectedAssignment) return

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
      title: 'Varlık Kodu',
      dataIndex: 'assetCode',
      key: 'assetCode',
      width: 130,
      render: (value: string) => <Typography.Text strong>{value}</Typography.Text>,
    },
    { title: 'Personel', dataIndex: 'employeeName', key: 'employeeName', ellipsis: true, width: 150 },
    { title: 'Departman', dataIndex: 'department', key: 'department', ellipsis: true, responsive: ['lg'], width: 140 },
    {
      title: 'Zimmet Tarihi',
      dataIndex: 'assignedAt',
      key: 'assignedAt',
      width: 120,
      render: (value: string) => formatDate(value),
    },
    {
      title: 'İşlemler',
      key: 'actions',
      align: 'center',
      width: 115,
      render: (_value, assignment) => (
        <Button
          icon={<RollbackOutlined />}
          onClick={() => setSelectedAssignment(assignment)}
          type="link"
        >
          İade Al
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
        title="İade İşlemleri"
        description="Aktif zimmetleri görüntüleyin ve teslim alınan cihazların iade işlemini tamamlayın."
      />

      <ContentCard>
        {isLoading ? (
          <LoadingState message="Aktif zimmetler yükleniyor..." />
        ) : loadError ? (
          <ErrorState message={loadError} onRetry={() => void loadAssignments()} />
        ) : (
          <Space className="assignments-content" direction="vertical" size="large">
            <Typography.Text type="secondary">
              {assignments.length} iade bekleyen zimmet bulundu
            </Typography.Text>
            <Table<Assignment>
              className="app-data-table"
              columns={columns}
              dataSource={assignments}
              locale={{
                emptyText: <EmptyState description="İade alınmayı bekleyen aktif zimmet bulunmuyor." />,
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
              scroll={{ x: 830 }}
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

export default AssignmentReturnsPage
