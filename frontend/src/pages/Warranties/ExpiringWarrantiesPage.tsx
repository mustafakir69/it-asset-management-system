import { ClockCircleOutlined, EyeOutlined } from '@ant-design/icons'
import { Button, Space, Statistic, Table, Typography } from 'antd'
import type { TableColumnsType, TablePaginationConfig } from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  ContentCard,
  EmptyState,
  ErrorState,
  LoadingState,
  PageHeader,
  StatusTag,
} from '../../components'
import { warrantyService } from '../../services/warrantyService'
import type { WarrantyAsset } from '../../types/warranty'
import { formatDate } from '../../utils'
import './ExpiringWarrantiesPage.css'

interface WarrantyPagination {
  current: number
  pageSize: number
}

const formatRemainingDays = (remainingDays: number | null): string => {
  if (remainingDays === null) return '—'
  if (remainingDays === 0) return 'Bugün sona eriyor'
  return `${remainingDays} gün`
}

function ExpiringWarrantiesPage() {
  const navigate = useNavigate()
  const [warranties, setWarranties] = useState<WarrantyAsset[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [pagination, setPagination] = useState<WarrantyPagination>({ current: 1, pageSize: 10 })

  const loadWarranties = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)

    try {
      setWarranties(await warrantyService.getWarranties())
    } catch (error: unknown) {
      setLoadError(
        error instanceof Error
          ? error.message
          : 'Süresi yaklaşan garantiler yüklenirken bir hata oluştu.',
      )
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadWarranties()
  }, [loadWarranties])

  const expiringWarranties = useMemo(
    () => warranties.filter((warranty) => warranty.warrantyStatus === 'Yaklaşıyor'),
    [warranties],
  )

  const columns: TableColumnsType<WarrantyAsset> = [
    {
      title: 'Cihaz Kodu',
      dataIndex: 'assetCode',
      key: 'assetCode',
      width: 125,
      render: (value: string) => <Typography.Text strong>{value}</Typography.Text>,
    },
    {
      title: 'Cihaz',
      key: 'device',
      width: 175,
      render: (_value, warranty) => (
        <Space direction="vertical" size={0}>
          <Typography.Text strong>{warranty.brand}</Typography.Text>
          <Typography.Text type="secondary">{warranty.model}</Typography.Text>
        </Space>
      ),
    },
    { title: 'Seri No', dataIndex: 'serialNumber', key: 'serialNumber', ellipsis: true, responsive: ['md'], width: 140 },
    { title: 'Lokasyon', dataIndex: 'location', key: 'location', ellipsis: true, responsive: ['lg'], width: 130 },
    {
      title: 'Garanti Bitiş Tarihi',
      dataIndex: 'warrantyEndDate',
      key: 'warrantyEndDate',
      align: 'center',
      width: 130,
      render: (value: string | null) => formatDate(value),
    },
    {
      title: 'Kalan Gün',
      dataIndex: 'remainingDays',
      key: 'remainingDays',
      align: 'center',
      width: 105,
      render: (value: number | null) => formatRemainingDays(value),
    },
    {
      title: 'Durum',
      key: 'status',
      align: 'center',
      width: 115,
      render: () => <StatusTag status="Yaklaşıyor" />,
    },
    {
      title: 'İşlemler',
      key: 'actions',
      align: 'center',
      width: 110,
      render: (_value, warranty) => (
        <Button
          icon={<EyeOutlined />}
          onClick={() => void navigate(`/assets/${warranty.assetId}`)}
          type="link"
        >
          Görüntüle
        </Button>
      ),
    },
  ]

  const handlePaginationChange = (nextPagination: TablePaginationConfig) => {
    setPagination({
      current: nextPagination.current ?? 1,
      pageSize: nextPagination.pageSize ?? 10,
    })
  }

  return (
    <section className="expiring-warranties-page">
      <PageHeader
        title="Süresi Yaklaşan Garantiler"
        description="Önümüzdeki 30 gün içinde sona erecek cihaz garantilerini görüntüleyin."
      />

      {isLoading ? (
        <ContentCard>
          <LoadingState message="Süresi yaklaşan garantiler yükleniyor..." />
        </ContentCard>
      ) : loadError ? (
        <ContentCard>
          <ErrorState message={loadError} onRetry={() => void loadWarranties()} />
        </ContentCard>
      ) : (
        <Space className="expiring-warranties-content" direction="vertical" size="large">
          <ContentCard>
            <Statistic
              prefix={<ClockCircleOutlined />}
              title="30 Gün İçinde Bitecek"
              value={expiringWarranties.length}
              valueStyle={{ color: '#d46b08' }}
            />
          </ContentCard>
          <ContentCard>
            <Table<WarrantyAsset>
              className="app-data-table"
              columns={columns}
              dataSource={expiringWarranties}
              locale={{
                emptyText: (
                  <EmptyState description="30 gün içinde süresi dolacak garanti bulunmuyor." />
                ),
              }}
              onChange={handlePaginationChange}
              pagination={{
                current: pagination.current,
                pageSize: pagination.pageSize,
                pageSizeOptions: ['10', '20', '50'],
                showSizeChanger: true,
                showTotal: (total) => `Toplam ${total} yaklaşan garanti`,
              }}
              rowKey="assetId"
              scroll={{ x: 1030 }}
              size="small"
              tableLayout="fixed"
            />
          </ContentCard>
        </Space>
      )}
    </section>
  )
}

export default ExpiringWarrantiesPage
