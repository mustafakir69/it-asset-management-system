import { Space, Typography } from 'antd'
import { ContentCard, EmptyState, PageHeader } from '../../components'

function MyAssignmentsPage() {
  return (
    <section>
      <PageHeader
        title="Zimmetlerim"
        description="Giriş yapan kullanıcıya ait cihaz zimmetleri bu ekranda görüntülenecektir."
      />

      <ContentCard>
        <EmptyState
          description={
            <Space direction="vertical" size={4}>
              <Typography.Text>
                Zimmetlerim özelliği kullanıcı giriş sistemi eklendiğinde aktif olacaktır.
              </Typography.Text>
              <Typography.Text type="secondary">
                Bu ekranda giriş yapan kullanıcıya ait aktif zimmetler gösterilecektir.
              </Typography.Text>
            </Space>
          }
        />
      </ContentCard>
    </section>
  )
}

export default MyAssignmentsPage
