import {
  CheckCircleOutlined,
  CrownOutlined,
  EditOutlined,
  EyeOutlined,
  KeyOutlined,
  MoreOutlined,
  PlusOutlined,
  SearchOutlined,
  SolutionOutlined,
  StopOutlined,
  TeamOutlined,
  ToolOutlined,
} from '@ant-design/icons'
import {
  App as AntdApp,
  Button,
  Col,
  Descriptions,
  Dropdown,
  Form,
  Input,
  Modal,
  Row,
  Select,
  Space,
  Table,
  Tooltip,
  Typography,
} from 'antd'
import type { MenuProps, TableColumnsType, TablePaginationConfig } from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { ActionStatisticCard, ContentCard, EmptyState, ErrorState, LoadingState, PageHeader, StatusTag } from '../../components'
import { useAuth } from '../../contexts/useAuth'
import { userService } from '../../services/userService'
import type { UserRole } from '../../types/auth'
import type { ManagedUser, UpdateUserInput } from '../../types/user'
import './UsersPage.css'

interface UserPagination {
  current: number
  pageSize: number
}

type UserView = 'all' | 'active' | 'inactive' | 'employee' | 'management'

interface PasswordResetFormValues {
  password: string
  passwordConfirmation: string
}

const roleOptions: Array<{ label: string; value: UserRole }> = [
  { label: 'Yönetici', value: 'Admin' },
  { label: 'IT Yetkilisi', value: 'IT' },
  { label: 'Çalışan', value: 'Employee' },
]

function UsersPage() {
  const navigate = useNavigate()
  const { message, modal } = AntdApp.useApp()
  const { role: currentRole, user: currentUser } = useAuth()
  const [editForm] = Form.useForm<UpdateUserInput>()
  const [passwordForm] = Form.useForm<PasswordResetFormValues>()
  const [searchParams, setSearchParams] = useSearchParams()
  const [users, setUsers] = useState<ManagedUser[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [pagination, setPagination] = useState<UserPagination>({ current: 1, pageSize: 10 })
  const [search, setSearch] = useState(() => searchParams.get('search') ?? '')
  const [view, setView] = useState<UserView>('all')
  const [viewedUser, setViewedUser] = useState<ManagedUser | null>(null)
  const [editedUser, setEditedUser] = useState<ManagedUser | null>(null)
  const [passwordUser, setPasswordUser] = useState<ManagedUser | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

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

  useEffect(() => {
    const requestedSearch = searchParams.get('search')
    if (requestedSearch !== null) {
      setSearch(requestedSearch)
      setView('all')
      setPagination((current) => ({ ...current, current: 1 }))
    }
  }, [searchParams])

  const filteredUsers = useMemo(() => {
    const term = search.trim().toLocaleLowerCase('tr-TR')
    return users.filter((user) => {
      const matchesSearch = !term || [user.fullName, user.department ?? '', user.username, user.email]
        .some((value) => value.toLocaleLowerCase('tr-TR').includes(term))
      const matchesView = view === 'all' ||
        (view === 'active' && user.isActive) ||
        (view === 'inactive' && !user.isActive) ||
        (view === 'employee' && user.role === 'Employee') ||
        (view === 'management' && (user.role === 'Admin' || user.role === 'IT'))
      return matchesSearch && matchesView
    })
  }, [search, users, view])

  const summaries = useMemo(() => ({
    total: users.length,
    active: users.filter((user) => user.isActive).length,
    inactive: users.filter((user) => !user.isActive).length,
    employee: users.filter((user) => user.role === 'Employee').length,
    management: users.filter((user) => user.role === 'Admin' || user.role === 'IT').length,
  }), [users])
  const activeAdminCount = users.filter((user) => user.role === 'Admin' && user.isActive).length

  const selectView = (nextView: UserView) => {
    setView(nextView)
    if (nextView === 'all') {
      setSearch('')
      setSearchParams({})
    }
    setPagination((current) => ({ ...current, current: 1 }))
  }

  const canManage = (user: ManagedUser) =>
    currentRole === 'Admin' || currentRole === 'IT' && user.role === 'Employee'

  const loadUserForAction = async (
    user: ManagedUser,
    onLoaded: (loadedUser: ManagedUser) => void,
  ) => {
    try {
      onLoaded(await userService.getUser(user.id))
    } catch (error: unknown) {
      message.error(error instanceof Error ? error.message : 'Kullanıcı bilgileri alınamadı.')
    }
  }

  const openEdit = (user: ManagedUser) => {
    void loadUserForAction(user, (loadedUser) => {
      editForm.setFieldsValue({
        username: loadedUser.username,
        email: loadedUser.email,
        role: loadedUser.role,
      })
      setEditedUser(loadedUser)
    })
  }

  const submitEdit = async () => {
    if (!editedUser) return
    const values = await editForm.validateFields()
    setIsSubmitting(true)
    try {
      await userService.updateUser(editedUser.id, {
        username: values.username.trim(),
        email: values.email.trim(),
        role: values.role,
      })
      message.success('Kullanıcı bilgileri güncellendi.')
      setEditedUser(null)
      await loadUsers()
    } catch (error: unknown) {
      message.error(error instanceof Error ? error.message : 'Kullanıcı güncellenemedi.')
    } finally {
      setIsSubmitting(false)
    }
  }

  const changeStatus = (user: ManagedUser) => {
    const nextActive = !user.isActive
    modal.confirm({
      title: `Kullanıcıyı ${nextActive ? 'aktif' : 'pasif'} yapmak istiyor musunuz?`,
      content: `${user.fullName} hesabının giriş durumu değiştirilecek.`,
      okText: nextActive ? 'Aktif Yap' : 'Pasif Yap',
      okButtonProps: { danger: !nextActive },
      cancelText: 'İptal',
      async onOk() {
        try {
          await userService.setUserActive(user.id, nextActive)
          message.success(`Kullanıcı ${nextActive ? 'aktif' : 'pasif'} duruma getirildi.`)
          await loadUsers()
        } catch (error: unknown) {
          message.error(error instanceof Error ? error.message : 'Kullanıcı durumu güncellenemedi.')
          throw error
        }
      },
    })
  }

  const openPasswordReset = (user: ManagedUser) => {
    passwordForm.resetFields()
    setPasswordUser(user)
  }

  const submitPasswordReset = async () => {
    if (!passwordUser) return
    const values = await passwordForm.validateFields()
    setIsSubmitting(true)
    try {
      await userService.resetPassword(passwordUser.id, values.password)
      message.success('Kullanıcı parolası güvenli şekilde sıfırlandı.')
      setPasswordUser(null)
      passwordForm.resetFields()
    } catch (error: unknown) {
      message.error(error instanceof Error ? error.message : 'Parola sıfırlanamadı.')
    } finally {
      setIsSubmitting(false)
    }
  }

  const getActionItems = (user: ManagedUser): MenuProps['items'] => {
    const manageable = canManage(user)
    const hasEmployee = user.employeeId !== null
    const isProtectedAdmin = user.id === currentUser?.id ||
      user.role === 'Admin' && user.isActive && activeAdminCount <= 1
    return [
      {
        key: 'view',
        icon: <EyeOutlined />,
        label: 'Görüntüle',
        onClick: () => void loadUserForAction(user, setViewedUser),
      },
      {
        key: 'edit',
        icon: <EditOutlined />,
        label: 'Düzenle',
        disabled: !manageable,
        onClick: () => openEdit(user),
      },
      {
        key: 'status',
        danger: user.isActive,
        icon: user.isActive ? <StopOutlined /> : <CheckCircleOutlined />,
        label: user.isActive ? 'Pasif Yap' : 'Aktif Yap',
        disabled: !manageable || isProtectedAdmin,
        onClick: () => changeStatus(user),
      },
      {
        key: 'password',
        icon: <KeyOutlined />,
        label: 'Şifre Sıfırla',
        disabled: !manageable,
        onClick: () => openPasswordReset(user),
      },
      { type: 'divider' },
      {
        key: 'assignments',
        icon: <SolutionOutlined />,
        label: 'Zimmetlerini Görüntüle',
        disabled: !hasEmployee,
        onClick: () => void navigate(`/assignments/history?employeeId=${encodeURIComponent(user.employeeId!)}`),
      },
      {
        key: 'support',
        icon: <ToolOutlined />,
        label: 'Teknik Destek Taleplerini Görüntüle',
        disabled: !hasEmployee,
        onClick: () => void navigate(`/support-requests?employeeId=${encodeURIComponent(user.employeeId!)}`),
      },
    ]
  }

  const columns: TableColumnsType<ManagedUser> = [
    {
      title: 'Ad Soyad',
      dataIndex: 'fullName',
      key: 'fullName',
      ellipsis: true,
      width: 180,
      render: (value: string) => <Typography.Text strong>{value}</Typography.Text>,
    },
    {
      title: 'Birim',
      dataIndex: 'department',
      key: 'department',
      ellipsis: true,
      width: 150,
      render: (value: string | null) => value ?? '—',
    },
    {
      title: 'Kullanıcı Adı',
      dataIndex: 'username',
      key: 'username',
      width: 150,
      responsive: ['md'],
    },
    {
      title: 'E-posta',
      dataIndex: 'email',
      key: 'email',
      ellipsis: true,
      width: 210,
      responsive: ['lg'],
    },
    { title: 'Rol', dataIndex: 'roleDisplayName', key: 'role', width: 140 },
    {
      title: 'Durum',
      key: 'status',
      width: 100,
      dataIndex: 'status',
      render: (value: ManagedUser['status']) => <StatusTag status={value} />,
    },
    {
      title: 'İşlemler',
      key: 'actions',
      align: 'center',
      width: 90,
      render: (_value, user) => (
        <Dropdown menu={{ items: getActionItems(user) }} placement="bottomRight" trigger={['click']}>
          <Tooltip title="İşlemleri aç">
            <Button
              aria-label={`${user.fullName} için işlemleri aç`}
              icon={<MoreOutlined />}
              size="small"
            />
          </Tooltip>
        </Dropdown>
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
      {!isLoading && !loadError && (
        <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
          <Col flex="1 1 180px"><ActionStatisticCard active={view === 'all'} color="#1677ff" icon={<TeamOutlined />} onClick={() => selectView('all')} title="Toplam Kullanıcı" value={summaries.total} /></Col>
          <Col flex="1 1 180px"><ActionStatisticCard active={view === 'active'} color="#389e0d" icon={<CheckCircleOutlined />} onClick={() => selectView('active')} title="Aktif" value={summaries.active} /></Col>
          <Col flex="1 1 180px"><ActionStatisticCard active={view === 'inactive'} color="#cf1322" icon={<StopOutlined />} onClick={() => selectView('inactive')} title="Pasif" value={summaries.inactive} /></Col>
          <Col flex="1 1 180px"><ActionStatisticCard active={view === 'employee'} color="#0958d9" icon={<TeamOutlined />} onClick={() => selectView('employee')} title="Çalışan" value={summaries.employee} /></Col>
          <Col flex="1 1 180px"><ActionStatisticCard active={view === 'management'} color="#531dab" icon={<CrownOutlined />} onClick={() => selectView('management')} title="Yönetici / IT" value={summaries.management} /></Col>
        </Row>
      )}
      <ContentCard>
        {isLoading ? (
          <LoadingState message="Kullanıcılar yükleniyor..." />
        ) : loadError ? (
          <ErrorState message={loadError} onRetry={() => void loadUsers()} />
        ) : (
          <Space className="users-content" direction="vertical" size="large">
            <Input allowClear onChange={(event) => { setSearch(event.target.value); setPagination((current) => ({ ...current, current: 1 })) }} placeholder="Ad soyad, birim, kullanıcı adı veya e-posta ara" prefix={<SearchOutlined />} value={search} />
            <Typography.Text type="secondary">{filteredUsers.length} kullanıcı bulundu</Typography.Text>
            <Table<ManagedUser>
              className="app-data-table"
              columns={columns}
              dataSource={filteredUsers}
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
              scroll={{ x: 900 }}
              size="small"
              tableLayout="fixed"
            />
          </Space>
        )}
      </ContentCard>

      <Modal
        cancelText="Kapat"
        footer={null}
        onCancel={() => setViewedUser(null)}
        open={viewedUser !== null}
        title="Kullanıcı Bilgileri"
      >
        {viewedUser && (
          <Descriptions
            bordered
            column={1}
            items={[
              { key: 'fullName', label: 'Ad Soyad', children: viewedUser.fullName },
              { key: 'department', label: 'Birim', children: viewedUser.department ?? '—' },
              { key: 'employeeNo', label: 'Sicil', children: viewedUser.employeeNo ?? '—' },
              { key: 'username', label: 'Kullanıcı Adı', children: viewedUser.username },
              { key: 'email', label: 'E-posta', children: viewedUser.email },
              { key: 'role', label: 'Rol', children: viewedUser.roleDisplayName },
              { key: 'status', label: 'Durum', children: <StatusTag status={viewedUser.status} /> },
            ]}
            size="small"
          />
        )}
      </Modal>

      <Modal
        cancelText="İptal"
        confirmLoading={isSubmitting}
        okText="Kaydet"
        onCancel={() => setEditedUser(null)}
        onOk={() => void submitEdit()}
        open={editedUser !== null}
        title="Kullanıcıyı Düzenle"
      >
        <Form<UpdateUserInput> form={editForm} layout="vertical" requiredMark="optional">
          <Form.Item label="Kullanıcı Adı" name="username" rules={[{ required: true, whitespace: true, message: 'Kullanıcı adını girin.' }, { min: 3, max: 100, message: 'Kullanıcı adı 3-100 karakter arasında olmalıdır.' }]}>
            <Input autoComplete="username" />
          </Form.Item>
          <Form.Item label="E-posta" name="email" rules={[{ required: true, message: 'E-posta adresini girin.' }, { type: 'email', message: 'Geçerli bir e-posta adresi girin.' }]}>
            <Input autoComplete="email" />
          </Form.Item>
          <Form.Item label="Rol" name="role" rules={[{ required: true, message: 'Kullanıcı rolünü seçin.' }]}>
            <Select<UserRole>
              options={roleOptions.filter((option) =>
                currentRole === 'IT'
                  ? option.value === 'Employee'
                  : editedUser?.employeeId === null
                    ? option.value === 'Admin'
                    : option.value !== 'Admin')}
            />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        cancelText="İptal"
        confirmLoading={isSubmitting}
        okText="Şifreyi Sıfırla"
        onCancel={() => setPasswordUser(null)}
        onOk={() => void submitPasswordReset()}
        open={passwordUser !== null}
        title="Şifre Sıfırla"
      >
        <Typography.Paragraph type="secondary">
          {passwordUser?.fullName} için yeni bir parola belirleyin. Parola yalnız güvenli hash olarak saklanır.
        </Typography.Paragraph>
        <Form<PasswordResetFormValues> form={passwordForm} layout="vertical" requiredMark="optional">
          <Form.Item label="Yeni Parola" name="password" rules={[{ required: true, message: 'Yeni parolayı girin.' }, { min: 8, max: 128, message: 'Parola 8-128 karakter arasında olmalıdır.' }]}>
            <Input.Password autoComplete="new-password" />
          </Form.Item>
          <Form.Item
            dependencies={['password']}
            label="Yeni Parola Tekrar"
            name="passwordConfirmation"
            rules={[
              { required: true, message: 'Yeni parolayı tekrar girin.' },
              ({ getFieldValue }) => ({
                validator: (_rule, value: string) =>
                  !value || getFieldValue('password') === value
                    ? Promise.resolve()
                    : Promise.reject(new Error('Parolalar eşleşmiyor.')),
              }),
            ]}
          >
            <Input.Password autoComplete="new-password" />
          </Form.Item>
        </Form>
      </Modal>
    </section>
  )
}

export default UsersPage
