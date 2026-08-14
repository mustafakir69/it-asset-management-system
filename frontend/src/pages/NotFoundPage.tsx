import { Button, Result } from 'antd'
import { useNavigate } from 'react-router-dom'

function NotFoundPage() {
  const navigate = useNavigate()

  return (
    <Result
      status="404"
      title="404"
      subTitle="Aradığınız sayfa bulunamadı."
      extra={
        <Button type="primary" onClick={() => void navigate('/dashboard')}>
          Dashboard'a Dön
        </Button>
      }
    />
  )
}

export default NotFoundPage
