import { LockOutlined, UserOutlined } from '@ant-design/icons'
import { App, Button, Card, Form, Input, Typography } from 'antd'
import { useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../../contexts/useAuth'
import type { LoginCredentials } from '../../types/auth'
import './LoginPage.css'

interface LoginLocationState {
  from?: string
}

function LoginPage() {
  const { message } = App.useApp()
  const { login } = useAuth()
  const location = useLocation()
  const navigate = useNavigate()

  const handleSubmit = async (values: LoginCredentials) => {
    try {
      await login(values)
      void message.success('Giriş başarılı.')
      const target = (location.state as LoginLocationState | null)?.from ?? '/dashboard'
      void navigate(target, { replace: true })
    } catch (error: unknown) {
      void message.error(error instanceof Error ? error.message : 'Giriş işlemi başarısız oldu.')
    }
  }

  return (
    <main className="login-page">
      <Card className="login-card">
        <div className="login-heading">
          <Typography.Title level={2}>Takip Sistemi</Typography.Title>
          <Typography.Text type="secondary">
            Donanım ve lisans takip sistemine giriş yapın.
          </Typography.Text>
        </div>

        <Form<LoginCredentials> layout="vertical" onFinish={handleSubmit} requiredMark={false}>
          <Form.Item
            label="Kullanıcı Adı / E-posta"
            name="identifier"
            rules={[{ required: true, whitespace: true, message: 'Kullanıcı adı veya e-posta girin.' }]}
          >
            <Input autoComplete="username" prefix={<UserOutlined />} size="large" />
          </Form.Item>

          <Form.Item
            label="Parola"
            name="password"
            rules={[{ required: true, message: 'Parolanızı girin.' }]}
          >
            <Input.Password autoComplete="current-password" prefix={<LockOutlined />} size="large" />
          </Form.Item>

          <Button block htmlType="submit" size="large" type="primary">
            Giriş Yap
          </Button>
        </Form>
      </Card>
    </main>
  )
}

export default LoginPage
