import { Space, Typography } from 'antd'
import { ContentCard, EmptyState, PageHeader } from '../../components'

interface FeatureInfoPageProps {
  title: string
  description: string
  message: string
  detail?: string
}

function FeatureInfoPage({ title, description, message, detail }: FeatureInfoPageProps) {
  return <section><PageHeader title={title} description={description} /><ContentCard><EmptyState description={<Space direction="vertical" size="small"><Typography.Text>{message}</Typography.Text>{detail && <Typography.Text type="secondary">{detail}</Typography.Text>}</Space>} /></ContentCard></section>
}
export default FeatureInfoPage
