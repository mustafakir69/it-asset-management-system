import { ArrowLeftOutlined, EditOutlined } from '@ant-design/icons'
import { Button, Descriptions, Space, Tag } from 'antd'
import type { DescriptionsProps } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ContentCard, ErrorState, LoadingState, PageHeader } from '../../components'
import { licenseService } from '../../services/licenseService'
import type { License, LicenseStatus } from '../../types/license'
import { formatDate } from '../../utils'

const statusColors: Record<LicenseStatus, string> = {
  Aktif: 'green',
  Yaklaşıyor: 'orange',
  'Süresi Doldu': 'red',
  Pasif: 'default',
}

function LicenseDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [license, setLicense] = useState<License | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)

  const loadLicense = useCallback(async () => {
    if (!id) {
      setLoadError('Görüntülenecek lisans bilgisi bulunamadı.')
      setIsLoading(false)
      return
    }

    setIsLoading(true)
    setLoadError(null)

    try {
      const licenseData = await licenseService.getLicenseById(id)
      if (!licenseData) {
        setLoadError('Aradığınız lisans bulunamadı.')
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

  const descriptionItems: DescriptionsProps['items'] = license
    ? [
        { key: 'licenseCode', label: 'Lisans Kodu', children: license.licenseCode },
        { key: 'productName', label: 'Ürün Adı', children: license.productName },
        { key: 'vendor', label: 'Sağlayıcı', children: license.vendor },
        { key: 'licenseType', label: 'Lisans Türü', children: license.licenseType },
        {
          key: 'totalSeats',
          label: 'Toplam Lisans Hakkı',
          children: license.totalSeats,
        },
        {
          key: 'usedSeats',
          label: 'Kullanılan Lisans Hakkı',
          children: license.usedSeats,
        },
        {
          key: 'availableSeats',
          label: 'Kalan Lisans Hakkı',
          children: license.availableSeats,
        },
        { key: 'startDate', label: 'Başlangıç Tarihi', children: formatDate(license.startDate) },
        {
          key: 'expirationDate',
          label: 'Bitiş Tarihi',
          children: formatDate(license.expirationDate),
        },
        {
          key: 'licenseStatus',
          label: 'Durum',
          children: <Tag color={statusColors[license.licenseStatus]}>{license.licenseStatus}</Tag>,
        },
        { key: 'notes', label: 'Not', children: license.notes || '—', span: 2 },
      ]
    : []

  return (
    <section>
      <PageHeader
        title={license ? license.licenseCode : 'Lisans Detayı'}
        description={license ? license.productName : 'Lisansın temel bilgileri ve kullanım durumu.'}
        actions={
          <Space wrap>
            <Button icon={<ArrowLeftOutlined />} onClick={() => void navigate('/licenses')}>
              Lisanslara Dön
            </Button>
            {license && (
              <Button
                icon={<EditOutlined />}
                onClick={() => void navigate(`/licenses/${license.id}/edit`)}
                type="primary"
              >
                Düzenle
              </Button>
            )}
          </Space>
        }
      />

      <ContentCard title="Lisans Bilgileri">
        {isLoading ? (
          <LoadingState message="Lisans bilgileri yükleniyor..." />
        ) : loadError ? (
          <ErrorState message={loadError} onRetry={() => void loadLicense()} />
        ) : license ? (
          <Descriptions
            bordered
            column={{ xs: 1, sm: 1, md: 2 }}
            items={descriptionItems}
            size="middle"
          />
        ) : null}
      </ContentCard>
    </section>
  )
}

export default LicenseDetailPage
