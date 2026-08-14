import { App as AntdApp } from 'antd'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { AssetForm, ContentCard, PageHeader } from '../../components'
import { assetService } from '../../services/assetService'
import type { AssetInput } from '../../types/asset'

function AssetCreatePage() {
  const navigate = useNavigate()
  const { message } = AntdApp.useApp()
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleSubmit = async (values: AssetInput) => {
    setIsSubmitting(true)

    try {
      await assetService.createAsset(values)
      message.success('Cihaz başarıyla kaydedildi.')
      void navigate('/assets')
    } catch (error: unknown) {
      message.error(error instanceof Error ? error.message : 'Cihaz kaydedilemedi.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section>
      <PageHeader
        title="Yeni Cihaz"
        description="Envantere eklenecek cihazın temel bilgilerini girin."
      />
      <ContentCard>
        <AssetForm
          isSubmitting={isSubmitting}
          onCancel={() => void navigate('/assets')}
          onSubmit={handleSubmit}
          submitLabel="Kaydet"
        />
      </ContentCard>
    </section>
  )
}

export default AssetCreatePage
