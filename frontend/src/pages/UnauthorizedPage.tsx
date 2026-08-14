import { Button, Result } from 'antd'
import { useNavigate } from 'react-router-dom'

function UnauthorizedPage() {
  const navigate = useNavigate()
  return <Result status="403" title="Yetkisiz Erişim" subTitle="Bu sayfayı görüntülemek için gerekli yetkiye sahip değilsiniz." extra={<Button onClick={() => void navigate('/dashboard')} type="primary">Dashboard'a Dön</Button>} />
}

export default UnauthorizedPage
