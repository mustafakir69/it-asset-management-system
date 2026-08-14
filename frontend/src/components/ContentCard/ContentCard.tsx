import type { ReactNode } from 'react'
import { Card } from 'antd'

export interface ContentCardProps {
  children: ReactNode
  title?: ReactNode
  extra?: ReactNode
  className?: string
}

function ContentCard({ children, title, extra, className }: ContentCardProps) {
  return (
    <Card className={className} title={title} extra={extra} variant="outlined">
      {children}
    </Card>
  )
}

export default ContentCard
