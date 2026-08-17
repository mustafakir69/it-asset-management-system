import { ArrowLeftOutlined } from '@ant-design/icons'
import { App as AntdApp, Button, Col, Form, Input, Row, Select, Space, Typography } from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ContentCard, ErrorState, LoadingState, PageHeader } from '../../components'
import { useAuth } from '../../contexts/useAuth'
import { assignmentService } from '../../services/assignmentService'
import { userService } from '../../services/userService'
import type { Employee } from '../../types/assignment'
import type { UserRole } from '../../types/auth'
import type { CreateUserInput } from '../../types/user'
import './UsersPage.css'

interface UserFormValues {
  employeeId?: string
  username: string
  email: string
  password: string
  role: UserRole
}

const roleOptions: Array<{ label: string; value: UserRole }> = [
  { label: 'Sistem Yöneticisi', value: 'Admin' },
  { label: 'IT Yetkilisi', value: 'IT' },
  { label: 'Çalışan', value: 'Employee' },
]

function UserCreatePage() {
  const navigate = useNavigate()
  const { message } = AntdApp.useApp()
  const { role: currentRole } = useAuth()
  const [form] = Form.useForm<UserFormValues>()
  const selectedRole = Form.useWatch('role', form)
  const [employees, setEmployees] = useState<Employee[]>([])
  const [usedEmployeeIds, setUsedEmployeeIds] = useState<ReadonlySet<string>>(new Set())
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [loadError, setLoadError] = useState<string | null>(null)

  const loadFormData = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)
    try {
      const [activeEmployees, users] = await Promise.all([
        assignmentService.getEmployees(),
        userService.getUsers(),
      ])
      setEmployees(activeEmployees)
      setUsedEmployeeIds(new Set(users.flatMap((user) => user.employeeId ? [user.employeeId] : [])))
    } catch (error: unknown) {
      setLoadError(error instanceof Error ? error.message : 'Form verileri yüklenemedi.')
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadFormData()
  }, [loadFormData])

  useEffect(() => {
    if (selectedRole === 'Admin') {
      form.setFieldValue('employeeId', undefined)
    }
  }, [form, selectedRole])

  const employeeOptions = useMemo(
    () => employees
      .filter((employee) => !usedEmployeeIds.has(employee.id))
      .map((employee) => ({
        label: `${employee.employeeNo} · ${employee.fullName} · ${employee.department}`,
        value: employee.id,
      })),
    [employees, usedEmployeeIds],
  )

  const handleSubmit = async (values: UserFormValues) => {
    const input: CreateUserInput = {
      employeeId: values.employeeId ?? null,
      username: values.username.trim(),
      email: values.email.trim(),
      password: values.password,
      role: currentRole === 'IT' ? 'Employee' : values.role,
    }

    setIsSubmitting(true)
    try {
      await userService.createUser(input)
      message.success('Kullanıcı hesabı başarıyla oluşturuldu.')
      void navigate('/admin/users')
    } catch (error: unknown) {
      message.error(error instanceof Error ? error.message : 'Kullanıcı oluşturulamadı.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section className="user-create-page">
      <PageHeader
        title="Yeni Kullanıcı"
        description="Sisteme erişecek kullanıcı hesabını oluşturun."
        actions={
          <Button icon={<ArrowLeftOutlined />} onClick={() => void navigate('/admin/users')}>
            Kullanıcılara Dön
          </Button>
        }
      />
      <ContentCard>
        {isLoading ? (
          <LoadingState message="Form verileri yükleniyor..." />
        ) : loadError ? (
          <ErrorState message={loadError} onRetry={() => void loadFormData()} />
        ) : (
          <Form<UserFormValues>
            form={form}
            initialValues={{ role: 'Employee' }}
            layout="vertical"
            onFinish={(values) => void handleSubmit(values)}
            requiredMark="optional"
          >
            <Row gutter={[16, 0]}>
              <Col xs={24} md={12}>
                <Form.Item
                  label="Rol"
                  name="role"
                  rules={[{ required: true, message: 'Kullanıcı rolünü seçin.' }]}
                >
                  <Select<UserRole>
                    disabled={currentRole === 'IT'}
                    options={currentRole === 'IT' ? roleOptions.filter((item) => item.value === 'Employee') : roleOptions}
                  />
                </Form.Item>
              </Col>
              <Col xs={24} md={12}>
                <Form.Item
                  label="Çalışan"
                  name="employeeId"
                  rules={[
                    {
                      required: selectedRole === 'Employee' || selectedRole === 'IT',
                      message: 'Çalışan veya IT rolü için personel seçin.',
                    },
                  ]}
                >
                  <Select<string>
                    allowClear={selectedRole === 'Admin'}
                    disabled={selectedRole === 'Admin'}
                    optionFilterProp="label"
                    options={employeeOptions}
                    placeholder="Çalışan seçin"
                    showSearch
                  />
                </Form.Item>
              </Col>
              <Col xs={24} md={12}>
                <Form.Item
                  label="Kullanıcı Adı"
                  name="username"
                  rules={[
                    { required: true, whitespace: true, message: 'Kullanıcı adını girin.' },
                    { min: 3, max: 100, message: 'Kullanıcı adı 3-100 karakter arasında olmalıdır.' },
                  ]}
                >
                  <Input autoComplete="username" placeholder="Örn. kullanici.adi" />
                </Form.Item>
              </Col>
              <Col xs={24} md={12}>
                <Form.Item
                  label="E-posta"
                  name="email"
                  rules={[
                    { required: true, message: 'E-posta adresini girin.' },
                    { type: 'email', message: 'Geçerli bir e-posta adresi girin.' },
                  ]}
                >
                  <Input autoComplete="email" placeholder="kullanici@example.com" />
                </Form.Item>
              </Col>
              <Col xs={24} md={12}>
                <Form.Item
                  label="Parola"
                  name="password"
                  rules={[
                    { required: true, message: 'Parolayı girin.' },
                    { min: 8, max: 128, message: 'Parola en az 8, en fazla 128 karakter olmalıdır.' },
                  ]}
                >
                  <Input.Password autoComplete="new-password" placeholder="En az 8 karakter" />
                </Form.Item>
              </Col>
            </Row>
            {currentRole === 'IT' && (
              <Typography.Paragraph type="secondary">
                IT yetkilileri yalnızca Çalışan rolünde kullanıcı hesabı oluşturabilir.
              </Typography.Paragraph>
            )}
            <div className="user-create-actions">
              <Space wrap>
                <Button disabled={isSubmitting} onClick={() => void navigate('/admin/users')}>
                  İptal
                </Button>
                <Button htmlType="submit" loading={isSubmitting} type="primary">
                  Kullanıcı Oluştur
                </Button>
              </Space>
            </div>
          </Form>
        )}
      </ContentCard>
    </section>
  )
}

export default UserCreatePage
