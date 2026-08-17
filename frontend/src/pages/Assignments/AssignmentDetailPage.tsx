import { ArrowLeftOutlined } from '@ant-design/icons'
import { Button, Descriptions } from 'antd'
import type { DescriptionsProps } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ContentCard, ErrorState, LoadingState, PageHeader, StatusTag } from '../../components'
import { assignmentService } from '../../services/assignmentService'
import { useAuth } from '../../contexts/useAuth'
import type { Assignment, AssignmentStatus } from '../../types/assignment'
import { formatDate } from '../../utils'

const getAssignmentStatus = (assignment: Assignment): AssignmentStatus =>
  assignment.returnedAt === null ? 'Aktif' : 'İade Edildi'

function AssignmentDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { user } = useAuth()
  const [assignment, setAssignment] = useState<Assignment | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)

  const loadAssignment = useCallback(async () => {
    if (!id) {
      setLoadError('Görüntülenecek zimmet bilgisi bulunamadı.')
      setIsLoading(false)
      return
    }

    setIsLoading(true)
    setLoadError(null)

    try {
      const assignmentData = await assignmentService.getAssignmentById(id)

      if (!assignmentData) {
        setLoadError('Aradığınız zimmet kaydı bulunamadı.')
        return
      }

      setAssignment(assignmentData)
    } catch (error: unknown) {
      setLoadError(
        error instanceof Error ? error.message : 'Zimmet bilgileri yüklenirken bir hata oluştu.',
      )
    } finally {
      setIsLoading(false)
    }
  }, [id])

  useEffect(() => {
    void loadAssignment()
  }, [loadAssignment])

  const descriptionItems: DescriptionsProps['items'] = assignment
    ? [
        { key: 'assetCode', label: 'Varlık Kodu', children: assignment.assetCode },
        {
          key: 'asset',
          label: 'Marka / Model',
          children: `${assignment.assetBrand} ${assignment.assetModel}`,
        },
        { key: 'employeeName', label: 'Çalışan', children: assignment.employeeName },
        { key: 'department', label: 'Departman', children: assignment.department },
        {
          key: 'assignedAt',
          label: 'Zimmet Tarihi',
          children: formatDate(assignment.assignedAt),
        },
        { key: 'assignedBy', label: 'Zimmetleyen', children: assignment.assignedByName },
        { key: 'notes', label: 'Zimmet Notu', children: assignment.notes || '—', span: 2 },
        {
          key: 'returnedAt',
          label: 'İade Tarihi',
          children: assignment.returnedAt ? formatDate(assignment.returnedAt) : '—',
        },
        { key: 'returnedBy', label: 'İade Alan', children: assignment.returnedByName || '—' },
        {
          key: 'returnNotes',
          label: 'İade Notu',
          children: assignment.returnNotes || '—',
          span: 2,
        },
        {
          key: 'status',
          label: 'Durum',
          children: <StatusTag status={getAssignmentStatus(assignment)} />,
          span: 2,
        },
      ]
    : []

  return (
    <section>
      <PageHeader
        title={assignment ? assignment.assetCode : 'Zimmet Detayı'}
        description={
          assignment
            ? `${assignment.assetBrand} ${assignment.assetModel} · ${assignment.employeeName}`
            : 'Zimmet kaydının temel bilgileri.'
        }
        actions={
          <Button
            icon={<ArrowLeftOutlined />}
            onClick={() => void navigate(user?.role === 'Employee' ? '/assignments/mine' : '/assignments')}
          >
            {user?.role === 'Employee' ? 'Zimmetlerime Dön' : 'Zimmetlere Dön'}
          </Button>
        }
      />

      <ContentCard title="Zimmet Bilgileri">
        {isLoading ? (
          <LoadingState message="Zimmet bilgileri yükleniyor..." />
        ) : loadError ? (
          <ErrorState message={loadError} onRetry={() => void loadAssignment()} />
        ) : assignment ? (
          <Descriptions
            bordered
            column={{ xs: 1, sm: 1, md: 2 }}
            items={descriptionItems}
            size="middle"
          />
        ) : null}
      </ContentCard>
    </section>
  )
}

export default AssignmentDetailPage
