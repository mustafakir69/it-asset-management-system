import type { ReactNode } from 'react'
import { Flex, Typography } from 'antd'
import './PageHeader.css'

export interface PageHeaderProps {
  title: ReactNode
  description?: ReactNode
  actions?: ReactNode
}

function PageHeader({ title, description, actions }: PageHeaderProps) {
  return (
    <Flex className="page-header" align="flex-start" gap={16} justify="space-between" wrap="wrap">
      <div className="page-header-text">
        <Typography.Title level={2}>{title}</Typography.Title>
        {description && <Typography.Text type="secondary">{description}</Typography.Text>}
      </div>
      {actions && <div className="page-header-actions">{actions}</div>}
    </Flex>
  )
}

export default PageHeader
