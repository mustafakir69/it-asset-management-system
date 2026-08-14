import { App as AntdApp } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ContentCard, ErrorState, LicenseForm, LoadingState, PageHeader } from '../../components'
import { licenseService } from '../../services/licenseService'
import type { License, LicenseInput } from '../../types/license'

function LicenseEditPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { message } = AntdApp.useApp()
  const [license, setLicense] = useState<License | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [loadError, setLoadError] = useState<string | null>(null)

  const loadLicense = useCallback(async () => {
    if (!id) {
      setLoadError('Düzenlenecek lisans bilgisi bulunamadı.')
      setIsLoading(false)
      return
    }

    setIsLoading(true)
    setLoadError(null)

    try {
      const licenseData = await licenseService.getLicenseById(id)
      if (!licenseData) {
        setLoadError('Düzenlenecek lisans bulunamadı.')
        return
      }
      setLicense(licenseData)
    } catch (error: unknown) {
      setLoadError(
        error instanceof Error ? error.message : 'Lisans bilgileri yüklenirken bir hata oluştu.',
      )
    } finally {
      setIsLoading(false)
    }
  }, [id])

  useEffect(() => {
    void loadLicense()
  }, [loadLicense])

  const handleSubmit = async (values: LicenseInput) => {
    if (!id) {
      message.error('Düzenlenecek lisans bilgisi bulunamadı.')
      return
    }

    setIsSubmitting(true)

    try {
      const updatedLicense = await licenseService.updateLicense(id, values)
      message.success('Lisans bilgileri başarıyla güncellendi.')
      void navigate(`/licenses/${updatedLicense.id}`)
    } catch (error: unknown) {
      message.error(error instanceof Error ? error.message : 'Lisans bilgileri güncellenemedi.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section>
      <PageHeader
        title="Lisansı Düzenle"
        description={license ? `${license.licenseCode} kodlu lisansı güncelleyin.` : undefined}
      />
      <ContentCard>
        {isLoading ? (
          <LoadingState message="Lisans bilgileri yükleniyor..." />
        ) : loadError ? (
          <ErrorState message={loadError} onRetry={() => void loadLicense()} />
        ) : license ? (
          <LicenseForm
            initialValues={license}
            isSubmitting={isSubmitting}
            onCancel={() => void navigate(`/licenses/${license.id}`)}
            onSubmit={handleSubmit}
            submitLabel="Değişiklikleri Kaydet"
          />
        ) : null}
      </ContentCard>
    </section>
  )
}

export default LicenseEditPage
