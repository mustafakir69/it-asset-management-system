import { PlusOutlined } from '@ant-design/icons'
import { Button, Space, Table, Typography } from 'antd'
import type { TableColumnsType, TablePaginationConfig } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ContentCard, EmptyState, ErrorState, LoadingState, PageHeader, StatusTag } from '../../components'
import { userService } from '../../services/userService'
import type { ManagedUser } from '../../types/user'
import './UsersPage.css'

interface UserPagination {
  current: number
  pageSize: number
}

function UsersPage() {
  const navigate = useNavigate()
  const [users, setUsers] = useState<ManagedUser[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [pagination, setPagination] = useState<UserPagination>({ current: 1, pageSize: 10 })

  const loadUsers = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)
    try {
      setUsers(await userService.getUsers())
    } catch (error: unknown) {
      setLoadError(error instanceof Error ? error.message : 'Kullanıcılar yüklenemedi.')
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadUsers()
  }, [loadUsers])

  const columns: TableColumnsType<ManagedUser> = [
    {
      title: 'Kullanıcı Adı',
      dataIndex: 'username',
      key: 'username',
      width: 160,
      render: (value: string) => <Typography.Text strong>{value}</Typography.Text>,
    },
    {
      title: 'Ad Soyad / Çalışan',
      dataIndex: 'employeeName',
      key: 'employeeName',
      ellipsis: true,
      width: 190,
      render: (value: string | null) => value ?? '—',
    },
    { title: 'E-posta', dataIndex: 'email', key: 'email', ellipsis: true, width: 220 },
    { title: 'Rol', dataIndex: 'roleDisplayName', key: 'role', width: 170 },
    {
      title: 'Durum',
      key: 'status',
      width: 100,
      render: (_value, user) => <StatusTag status={user.isActive ? 'Aktif' : 'Pasif'} />,
    },
    {
      title: 'İşlemler',
      key: 'actions',
      align: 'center',
      width: 90,
      render: () => <Typography.Text type="secondary">—</Typography.Text>,
    },
  ]

  const handlePaginationChange = (nextPagination: TablePaginationConfig) => {
    setPagination({
      current: nextPagination.current ?? 1,
      pageSize: nextPagination.pageSize ?? 10,
    })
  }

  return (
    <section className="users-page">
      <PageHeader
        title="Kullanıcılar"
        description="Sisteme erişebilen kullanıcı hesaplarını görüntüleyin."
        actions={
          <Button
            icon={<PlusOutlined />}
            onClick={() => void navigate('/admin/users/new')}
            type="primary"
          >
            Yeni Kullanıcı
          </Button>
        }
      />
      <ContentCard>
        {isLoading ? (
          <LoadingState message="Kullanıcılar yükleniyor..." />
        ) : loadError ? (
          <ErrorState message={loadError} onRetry={() => void loadUsers()} />
        ) : (
          <Space className="users-content" direction="vertical" size="large">
            <Typography.Text type="secondary">{users.length} kullanıcı bulundu</Typography.Text>
            <Table<ManagedUser>
              className="app-data-table"
              columns={columns}
              dataSource={users}
              locale={{ emptyText: <EmptyState description="Kullanıcı kaydı bulunamadı." /> }}
              onChange={handlePaginationChange}
              pagination={{
                current: pagination.current,
                pageSize: pagination.pageSize,
                pageSizeOptions: ['10', '20', '50'],
                showSizeChanger: true,
                showTotal: (total) => `Toplam ${total} kullanıcı`,
              }}
              rowKey="id"
              scroll={{ x: 930 }}
              size="small"
              tableLayout="fixed"
            />
          </Space>
        )}
      </ContentCard>
    </section>
  )
}

export default UsersPage
