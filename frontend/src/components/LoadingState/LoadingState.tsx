import { Flex, Spin, Typography } from 'antd'

export interface LoadingStateProps {
  message?: string
}

function LoadingState({ message = 'Yükleniyor...' }: LoadingStateProps) {
  return (
    <Flex align="center" aria-live="polite" gap={12} justify="center" role="status">
      <Spin size="small" />
      <Typography.Text type="secondary">{message}</Typography.Text>
    </Flex>
  )
}

export default LoadingState
