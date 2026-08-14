import type { ReactNode } from 'react'
import { Empty } from 'antd'

export interface EmptyStateProps {
  description?: ReactNode
  action?: ReactNode
}

function EmptyState({ description = 'Gösterilecek kayıt bulunamadı.', action }: EmptyStateProps) {
  return (
    <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={description}>
      {action}
    </Empty>
  )
}

export default EmptyState
