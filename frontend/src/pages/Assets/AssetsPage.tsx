import {
  ClearOutlined,
  EditOutlined,
  EyeOutlined,
  MoreOutlined,
  PlusOutlined,
  SearchOutlined,
} from '@ant-design/icons'
import { Button, Col, Dropdown, Flex, Input, Row, Select, Space, Table, Tooltip, Typography } from 'antd'
import type { MenuProps, TableColumnsType, TablePaginationConfig } from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ContentCard, EmptyState, ErrorState, PageHeader, StatusTag } from '../../components'
import { assetService } from '../../services/assetService'
import type { Asset, AssetCategory, AssetLocation, AssetStatus } from '../../types/asset'
import { formatDate } from '../../utils'
import './AssetsPage.css'

interface AssetFilters {
  search: string
  category?: AssetCategory
  status?: AssetStatus
  location?: AssetLocation
}

interface AssetPagination {
  current: number
  pageSize: number
}

const initialFilters: AssetFilters = {
  search: '',
}

function AssetsPage() {
  const navigate = useNavigate()
  const [assets, setAssets] = useState<Asset[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [filters, setFilters] = useState<AssetFilters>(initialFilters)
  const [pagination, setPagination] = useState<AssetPagination>({ current: 1, pageSize: 10 })

  const loadAssets = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)

    try {
      const assetData = await assetService.getAssets()
      setAssets(assetData)
    } catch (error: unknown) {
      setLoadError(
        error instanceof Error
          ? error.message
          : 'Envanter verileri yüklenirken bir hata oluştu.',
      )
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadAssets()
  }, [loadAssets])

  const categoryOptions = useMemo(
    () =>
      Array.from(new Set(assets.map((asset) => asset.category)))
        .sort((first, second) => first.localeCompare(second, 'tr-TR'))
        .map((category) => ({ label: category, value: category })),
    [assets],
  )

  const statusOptions = useMemo(
    () =>
      Array.from(new Set(assets.map((asset) => asset.status)))
        .sort((first, second) => first.localeCompare(second, 'tr-TR'))
        .map((status) => ({ label: status, value: status })),
    [assets],
  )

  const locationOptions = useMemo(
    () =>
      Array.from(new Set(assets.map((asset) => asset.location)))
        .sort((first, second) => first.localeCompare(second, 'tr-TR'))
        .map((location) => ({ label: location, value: location })),
    [assets],
  )

  const filteredAssets = useMemo(() => {
    const normalizedSearch = filters.search.trim().toLocaleLowerCase('tr-TR')

    return assets.filter((asset) => {
      const matchesSearch =
        normalizedSearch.length === 0 ||
        [asset.assetCode, asset.serialNumber, asset.brand, asset.model].some((value) =>
          value.toLocaleLowerCase('tr-TR').includes(normalizedSearch),
        )

      return (
        matchesSearch &&
        (!filters.category || asset.category === filters.category) &&
        (!filters.status || asset.status === filters.status) &&
        (!filters.location || asset.location === filters.location)
      )
    })
  }, [assets, filters])

  const updateFilters = (nextFilters: Partial<AssetFilters>) => {
    setFilters((current) => ({ ...current, ...nextFilters }))
    setPagination((current) => ({ ...current, current: 1 }))
  }

  const clearFilters = () => {
    setFilters(initialFilters)
    setPagination((current) => ({ ...current, current: 1 }))
  }

  const handlePaginationChange = (nextPagination: TablePaginationConfig) => {
    setPagination({
      current: nextPagination.current ?? 1,
      pageSize: nextPagination.pageSize ?? 10,
    })
  }

  const getActionItems = (asset: Asset): MenuProps['items'] => [
    {
      key: 'view',
      icon: <EyeOutlined />,
      label: 'Görüntüle',
      onClick: () => void navigate(`/assets/${asset.id}`),
    },
    {
      key: 'edit',
      icon: <EditOutlined />,
      label: 'Düzenle',
      onClick: () => void navigate(`/assets/${asset.id}/edit`),
    },
  ]

  const columns: TableColumnsType<Asset> = [
    {
      title: 'Varlık Kodu',
      dataIndex: 'assetCode',
      key: 'assetCode',
      width: 120,
      sorter: (first, second) => first.assetCode.localeCompare(second.assetCode, 'tr-TR'),
      render: (assetCode: string) => <Typography.Text strong>{assetCode}</Typography.Text>,
    },
    {
      title: 'Kategori',
      dataIndex: 'category',
      key: 'category',
      ellipsis: true,
      responsive: ['lg'],
      width: 120,
    },
    {
      title: 'Marka / Model',
      key: 'brandModel',
      width: 165,
      sorter: (first, second) =>
        `${first.brand} ${first.model}`.localeCompare(`${second.brand} ${second.model}`, 'tr-TR'),
      render: (_value, asset) => (
        <Flex vertical>
          <Typography.Text strong>{asset.brand}</Typography.Text>
          <Typography.Text type="secondary">{asset.model}</Typography.Text>
        </Flex>
      ),
    },
    {
      title: 'Seri Numarası',
      dataIndex: 'serialNumber',
      key: 'serialNumber',
      ellipsis: true,
      responsive: ['md'],
      width: 130,
    },
    {
      title: 'Durum',
      dataIndex: 'status',
      key: 'status',
      width: 100,
      render: (status: AssetStatus, item) => (
        <Space direction="vertical" size={0}>
          <StatusTag status={status} />
          {status === 'Zimmetli' && item.currentAssigneeName && (
            <Typography.Text type="secondary">— {item.currentAssigneeName}</Typography.Text>
          )}
        </Space>
      ),
    },
    {
      title: 'Lokasyon',
      dataIndex: 'location',
      key: 'location',
      ellipsis: true,
      width: 120,
    },
    {
      title: <span className="asset-date-heading">Satın Alma<br />Tarihi</span>,
      dataIndex: 'purchaseDate',
      key: 'purchaseDate',
      align: 'center',
      className: 'asset-date-column',
      responsive: ['xl'],
      width: 112,
      sorter: (first, second) => first.purchaseDate.localeCompare(second.purchaseDate),
      render: (purchaseDate: string) => formatDate(purchaseDate),
    },
    {
      title: <span className="asset-date-heading">Garanti Bitiş<br />Tarihi</span>,
      dataIndex: 'warrantyEndDate',
      key: 'warrantyEndDate',
      align: 'center',
      className: 'asset-date-column',
      width: 120,
      sorter: (first, second) => (first.warrantyEndDate ?? '').localeCompare(second.warrantyEndDate ?? ''),
      render: (warrantyEndDate: string) => formatDate(warrantyEndDate),
    },
    {
      title: 'İşlemler',
      key: 'actions',
      align: 'center',
      width: 72,
      render: (_value, asset) => (
        <Dropdown menu={{ items: getActionItems(asset) }} placement="bottomRight" trigger={['click']}>
          <Tooltip title="İşlemleri aç">
            <Button
              aria-label={`${asset.assetCode} için işlemleri aç`}
              className="asset-actions-button"
              icon={<MoreOutlined />}
              size="small"
            />
          </Tooltip>
        </Dropdown>
      ),
    },
  ]

  return (
    <section className="assets-page">
      <PageHeader
        title="Donanım Envanteri"
        description="Şirkete ait IT cihazlarını görüntüleyin ve yönetin."
        actions={
          <Button icon={<PlusOutlined />} onClick={() => void navigate('/assets/new')} type="primary">
            Yeni Cihaz
          </Button>
        }
      />

      <ContentCard>
        {loadError ? (
          <ErrorState message={loadError} onRetry={() => void loadAssets()} />
        ) : (
          <Space className="assets-content" direction="vertical" size="large">
            <Row gutter={[12, 12]}>
              <Col xs={24} lg={8}>
                <Input
                  allowClear
                  aria-label="Envanterde ara"
                  onChange={(event) => updateFilters({ search: event.target.value })}
                  placeholder="Varlık kodu, seri no, marka veya model ara"
                  prefix={<SearchOutlined />}
                  value={filters.search}
                />
              </Col>
              <Col xs={24} sm={12} lg={4}>
                <Select<AssetCategory>
                  allowClear
                  aria-label="Kategori filtresi"
                  onChange={(category) => updateFilters({ category })}
                  options={categoryOptions}
                  placeholder="Kategori"
                  value={filters.category}
                />
              </Col>
              <Col xs={24} sm={12} lg={4}>
                <Select<AssetStatus>
                  allowClear
                  aria-label="Durum filtresi"
                  onChange={(status) => updateFilters({ status })}
                  options={statusOptions}
                  placeholder="Durum"
                  value={filters.status}
                />
              </Col>
              <Col xs={24} sm={12} lg={4}>
                <Select<AssetLocation>
                  allowClear
                  aria-label="Lokasyon filtresi"
                  onChange={(location) => updateFilters({ location })}
                  options={locationOptions}
                  placeholder="Lokasyon"
                  value={filters.location}
                />
              </Col>
              <Col xs={24} sm={12} lg={4}>
                <Button block icon={<ClearOutlined />} onClick={clearFilters}>
                  Filtreleri Temizle
                </Button>
              </Col>
            </Row>

            <Typography.Text type="secondary">{filteredAssets.length} kayıt bulundu</Typography.Text>

            <Table<Asset>
              className="app-data-table"
              columns={columns}
              dataSource={filteredAssets}
              loading={isLoading}
              locale={{
                emptyText: <EmptyState description="Filtrelere uygun cihaz bulunamadı." />,
              }}
              onChange={handlePaginationChange}
              pagination={{
                current: pagination.current,
                pageSize: pagination.pageSize,
                pageSizeOptions: ['10', '20', '50'],
                showSizeChanger: true,
                showTotal: (total) => `Toplam ${total} kayıt`,
              }}
              rowKey="id"
              scroll={{ x: 1059 }}
              size="small"
              tableLayout="fixed"
            />
          </Space>
        )}
      </ContentCard>
    </section>
  )
}

export default AssetsPage
