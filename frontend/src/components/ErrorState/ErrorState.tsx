import { Button, Result } from 'antd'

export interface ErrorStateProps {
  title?: string
  message?: string
  onRetry?: () => void
}

function ErrorState({
  title = 'Bir hata oluştu',
  message = 'İçerik yüklenirken beklenmeyen bir hata oluştu.',
  onRetry,
}: ErrorStateProps) {
  return (
    <Result
      status="error"
      title={title}
      subTitle={message}
      extra={
        onRetry ? (
          <Button type="primary" onClick={onRetry}>
            Tekrar Dene
          </Button>
        ) : undefined
      }
    />
  )
}

export default ErrorState
