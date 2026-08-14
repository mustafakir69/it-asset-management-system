import { App as AntdApp } from 'antd'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ContentCard, LicenseForm, PageHeader } from '../../components'
import { licenseService } from '../../services/licenseService'
import type { LicenseInput } from '../../types/license'

function LicenseCreatePage() {
  const navigate = useNavigate()
  const { message } = AntdApp.useApp()
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleSubmit = async (values: LicenseInput) => {
    setIsSubmitting(true)

    try {
      await licenseService.createLicense(values)
      message.success('Lisans başarıyla kaydedildi.')
      void navigate('/licenses')
    } catch (error: unknown) {
      message.error(error instanceof Error ? error.message : 'Lisans kaydedilemedi.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section>
      <PageHeader
        title="Yeni Lisans"
        description="Yeni lisansın ürün, kullanım hakkı ve tarih bilgilerini girin."
      />
      <ContentCard>
        <LicenseForm
          isSubmitting={isSubmitting}
          onCancel={() => void navigate('/licenses')}
          onSubmit={handleSubmit}
          submitLabel="Kaydet"
        />
      </ContentCard>
    </section>
  )
}

export default LicenseCreatePage
