import { EyeOutlined, MoreOutlined } from '@ant-design/icons'
import { Button, Dropdown, Space, Table, Tooltip, Typography } from 'antd'
import type { MenuProps, TableColumnsType, TablePaginationConfig } from 'antd'
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
import { licenseService } from '../../services/licenseService'
import type { License } from '../../types/license'
import { formatDate } from '../../utils'
import './LicensesPage.css'

interface LicensePagination {
  current: number
  pageSize: number
}

const calculateRemainingDays = (expirationDate: string | null): number | null => {
  if (!expirationDate) return null

  const endDate = new Date(`${expirationDate}T00:00:00`)
  const today = new Date()
  today.setHours(0, 0, 0, 0)

  if (Number.isNaN(endDate.getTime())) return null

  return Math.round((endDate.getTime() - today.getTime()) / 86_400_000)
}

function ExpiringLicensesPage() {
  const navigate = useNavigate()
  const [licenses, setLicenses] = useState<License[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [pagination, setPagination] = useState<LicensePagination>({ current: 1, pageSize: 10 })

  const loadLicenses = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)

    try {
      setLicenses(await licenseService.getLicenses())
    } catch (error: unknown) {
      setLoadError(
        error instanceof Error
          ? error.message
          : 'Süresi yaklaşan lisanslar yüklenirken bir hata oluştu.',
      )
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadLicenses()
  }, [loadLicenses])

  const expiringLicenses = useMemo(
    () =>
      licenses
        .filter((license) => license.licenseStatus === 'Yaklaşıyor')
        .sort((first, second) =>
          (first.expirationDate ?? '').localeCompare(second.expirationDate ?? ''),
        ),
    [licenses],
  )

  const getActionItems = (license: License): MenuProps['items'] => [
    {
      key: 'view',
      icon: <EyeOutlined />,
      label: 'Görüntüle',
      onClick: () => void navigate(`/licenses/${license.id}`),
    },
  ]

  const columns: TableColumnsType<License> = [
    {
      title: 'Lisans Kodu',
      dataIndex: 'licenseCode',
      key: 'licenseCode',
      width: 125,
      render: (value: string) => <Typography.Text strong>{value}</Typography.Text>,
    },
    { title: 'Ürün', dataIndex: 'productName', key: 'productName', ellipsis: true, width: 170 },
    {
      title: 'Sağlayıcı',
      dataIndex: 'vendor',
      key: 'vendor',
      ellipsis: true,
      responsive: ['lg'],
      width: 105,
    },
    {
      title: 'Lisans Türü',
      dataIndex: 'licenseType',
      key: 'licenseType',
      ellipsis: true,
      responsive: ['xl'],
      width: 115,
    },
    {
      title: <span>Toplam Lisans<br />Hakkı</span>,
      dataIndex: 'totalSeats',
      key: 'totalSeats',
      align: 'center',
      width: 105,
    },
    { title: 'Kullanılan', dataIndex: 'usedSeats', key: 'usedSeats', align: 'center', width: 85 },
    {
      title: 'Kalan',
      dataIndex: 'availableSeats',
      key: 'availableSeats',
      align: 'center',
      width: 75,
    },
    {
      title: 'Bitiş Tarihi',
      dataIndex: 'expirationDate',
      key: 'expirationDate',
      align: 'center',
      width: 105,
      render: (value: string | null) => formatDate(value),
    },
    {
      title: 'Kalan Gün',
      key: 'remainingDays',
      align: 'center',
      width: 95,
      render: (_value, license) => {
        const remainingDays = calculateRemainingDays(license.expirationDate)
        return remainingDays === null ? '—' : `${remainingDays} gün`
      },
    },
    {
      title: 'Durum',
      key: 'status',
      align: 'center',
      width: 110,
      render: () => <StatusTag status="Yaklaşıyor" />,
    },
    {
      title: 'İşlemler',
      key: 'actions',
      align: 'center',
      width: 72,
      render: (_value, license) => (
        <Dropdown
          menu={{ items: getActionItems(license) }}
          placement="bottomRight"
          trigger={['click']}
        >
          <Tooltip title="İşlemleri aç">
            <Button
              aria-label={`${license.licenseCode} için işlemleri aç`}
              icon={<MoreOutlined />}
              size="small"
            />
          </Tooltip>
        </Dropdown>
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
    <section className="licenses-page">
      <PageHeader
        title="Süresi Yaklaşan Lisanslar"
        description="Önümüzdeki 30 gün içinde sona erecek lisansları görüntüleyin."
      />

      <ContentCard>
        {isLoading ? (
          <LoadingState message="Süresi yaklaşan lisanslar yükleniyor..." />
        ) : loadError ? (
          <ErrorState message={loadError} onRetry={() => void loadLicenses()} />
        ) : (
          <Space className="licenses-table-content" direction="vertical" size="large">
            <Typography.Text type="secondary">
              {expiringLicenses.length} yaklaşan lisans bulundu
            </Typography.Text>
            <Table<License>
              className="app-data-table"
              columns={columns}
              dataSource={expiringLicenses}
              locale={{
                emptyText: (
                  <EmptyState description="30 gün içinde süresi dolacak lisans bulunmuyor." />
                ),
              }}
              onChange={handlePaginationChange}
              pagination={{
                current: pagination.current,
                pageSize: pagination.pageSize,
                pageSizeOptions: ['10', '20', '50'],
                showSizeChanger: true,
                showTotal: (total) => `Toplam ${total} yaklaşan lisans`,
              }}
              rowKey="id"
              scroll={{ x: 1162 }}
              size="small"
              tableLayout="fixed"
            />
          </Space>
        )}
      </ContentCard>
    </section>
  )
}

export default ExpiringLicensesPage
