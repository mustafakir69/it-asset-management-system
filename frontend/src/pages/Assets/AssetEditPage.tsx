import { App as AntdApp } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { AssetForm, ContentCard, ErrorState, LoadingState, PageHeader } from '../../components'
import { assetService } from '../../services/assetService'
import type { Asset, AssetInput } from '../../types/asset'

function AssetEditPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { message } = AntdApp.useApp()
  const [asset, setAsset] = useState<Asset | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [loadError, setLoadError] = useState<string | null>(null)

  const loadAsset = useCallback(async () => {
    if (!id) {
      setLoadError('Düzenlenecek cihaz bilgisi bulunamadı.')
      setIsLoading(false)
      return
    }

    setIsLoading(true)
    setLoadError(null)

    try {
      const assetData = await assetService.getAssetById(id)

      if (!assetData) {
        setLoadError('Düzenlenecek cihaz bulunamadı.')
        return
      }

      setAsset(assetData)
    } catch (error: unknown) {
      setLoadError(
        error instanceof Error ? error.message : 'Cihaz bilgileri yüklenirken bir hata oluştu.',
      )
    } finally {
      setIsLoading(false)
    }
  }, [id])

  useEffect(() => {
    void loadAsset()
  }, [loadAsset])

  const handleSubmit = async (values: AssetInput) => {
    if (!id) {
      message.error('Düzenlenecek cihaz bilgisi bulunamadı.')
      return
    }

    setIsSubmitting(true)

    try {
      await assetService.updateAsset(id, values)
      message.success('Cihaz bilgileri başarıyla güncellendi.')
      void navigate('/assets')
    } catch (error: unknown) {
      message.error(error instanceof Error ? error.message : 'Cihaz bilgileri güncellenemedi.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section>
      <PageHeader
        title="Cihazı Düzenle"
        description={asset ? `${asset.assetCode} kodlu cihazın bilgilerini güncelleyin.` : undefined}
      />
      <ContentCard>
        {isLoading ? (
          <LoadingState message="Cihaz bilgileri yükleniyor..." />
        ) : loadError ? (
          <ErrorState message={loadError} onRetry={() => void loadAsset()} />
        ) : asset ? (
          <AssetForm
            initialValues={asset}
            isSubmitting={isSubmitting}
            onCancel={() => void navigate('/assets')}
            onSubmit={handleSubmit}
            submitLabel="Değişiklikleri Kaydet"
          />
        ) : null}
      </ContentCard>
    </section>
  )
}

export default AssetEditPage
