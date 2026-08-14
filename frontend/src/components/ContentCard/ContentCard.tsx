import type { ReactNode } from 'react'
import { Card } from 'antd'

export interface ContentCardProps {
  children: ReactNode
  title?: ReactNode
  extra?: ReactNode
}

function ContentCard({ children, title, extra }: ContentCardProps) {
  return (
    <Card title={title} extra={extra} variant="outlined">
      {children}
    </Card>
  )
}

export default ContentCard
