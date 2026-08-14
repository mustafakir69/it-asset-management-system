import { Space, Typography } from 'antd'
import { ContentCard, EmptyState, PageHeader } from '../../components'

function MyMaintenanceRequestsPage() {
  return <section><PageHeader title="Bakım Taleplerim" description="Giriş yapan kullanıcıya ait bakım talepleri." /><ContentCard><EmptyState description={<Space direction="vertical" size="small"><Typography.Text>Bakım Taleplerim özelliği bakım talepleri güvenli çalışan ilişkisiyle kullanıcıya bağlandığında aktif olacaktır.</Typography.Text><Typography.Text type="secondary">Bu ekranda giriş yapan kullanıcıya ait bakım talepleri gösterilecektir.</Typography.Text></Space>} /></ContentCard></section>
}

export default MyMaintenanceRequestsPage
