import { ArrowLeftOutlined, EditOutlined } from '@ant-design/icons'
import { Button, Descriptions, Space } from 'antd'
import type { DescriptionsProps } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ContentCard, ErrorState, LoadingState, PageHeader, StatusTag } from '../../components'
import { assetService } from '../../services/assetService'
import type { Asset } from '../../types/asset'
import { formatDate } from '../../utils'

function AssetDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [asset, setAsset] = useState<Asset | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)

  const loadAsset = useCallback(async () => {
    if (!id) {
      setLoadError('Görüntülenecek cihaz bilgisi bulunamadı.')
      setIsLoading(false)
      return
    }

    setIsLoading(true)
    setLoadError(null)

    try {
      const assetData = await assetService.getAssetById(id)

      if (!assetData) {
        setLoadError('Aradığınız cihaz envanterde bulunamadı.')
        return
      }

      setAsset(assetData)
    } catch {
      setLoadError('Cihaz bilgileri yüklenirken bir hata oluştu.')
    } finally {
      setIsLoading(false)
    }
  }, [id])

  useEffect(() => {
    void loadAsset()
  }, [loadAsset])

  const descriptionItems: DescriptionsProps['items'] = asset
    ? [
        { key: 'assetCode', label: 'Varlık Kodu', children: asset.assetCode },
        { key: 'category', label: 'Kategori', children: asset.category },
        { key: 'brand', label: 'Marka', children: asset.brand },
        { key: 'model', label: 'Model', children: asset.model },
        { key: 'serialNumber', label: 'Seri Numarası', children: asset.serialNumber },
        { key: 'status', label: 'Durum', children: <StatusTag status={asset.status} /> },
        { key: 'location', label: 'Lokasyon', children: asset.location },
        {
          key: 'purchaseDate',
          label: 'Satın Alma Tarihi',
          children: formatDate(asset.purchaseDate),
        },
        {
          key: 'warrantyEndDate',
          label: 'Garanti Bitiş Tarihi',
          children: formatDate(asset.warrantyEndDate),
        },
      ]
    : []

  return (
    <section>
      <PageHeader
        title={asset ? asset.assetCode : 'Cihaz Detayı'}
        description={asset ? `${asset.brand} ${asset.model}` : 'Cihazın temel envanter bilgileri.'}
        actions={
          <Space wrap>
            <Button icon={<ArrowLeftOutlined />} onClick={() => void navigate('/assets')}>
              Envantere Dön
            </Button>
            {asset && (
              <Button
                icon={<EditOutlined />}
                onClick={() => void navigate(`/assets/${asset.id}/edit`)}
                type="primary"
              >
                Düzenle
              </Button>
            )}
          </Space>
        }
      />

      <ContentCard title="Cihaz Bilgileri">
        {isLoading ? (
          <LoadingState message="Cihaz bilgileri yükleniyor..." />
        ) : loadError ? (
          <ErrorState message={loadError} onRetry={() => void loadAsset()} />
        ) : asset ? (
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

export default AssetDetailPage
