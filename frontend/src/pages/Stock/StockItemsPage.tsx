import {
  ClearOutlined,
  EyeOutlined,
  MoreOutlined,
  PlusOutlined,
  SearchOutlined,
  SwapOutlined,
} from '@ant-design/icons'
import {
  Button,
  Col,
  Dropdown,
  Input,
  Row,
  Select,
  Space,
  Table,
  Tag,
  Tooltip,
  Typography,
} from 'antd'
import type { MenuProps, TableColumnsType, TablePaginationConfig } from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ContentCard, EmptyState, ErrorState, LoadingState, PageHeader } from '../../components'
import { stockService } from '../../services/stockService'
import type { StockItem, StockStatus } from '../../types/stockItem'
import StockTransactionModal from './StockTransactionModal'
import './StockItemsPage.css'

interface StockFilters {
  search: string
  category?: string
  status?: StockStatus
  location?: string
}

interface StockPagination {
  current: number
  pageSize: number
}

const initialFilters: StockFilters = {
  search: '',
}

const statusOptions: Array<{ label: StockStatus; value: StockStatus }> = [
  { label: 'Normal', value: 'Normal' },
  { label: 'Kritik', value: 'Kritik' },
]

const getStockStatus = (item: StockItem): StockStatus =>
  item.isCritical ? 'Kritik' : 'Normal'

function StockItemsPage() {
  const navigate = useNavigate()
  const [stockItems, setStockItems] = useState<StockItem[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [filters, setFilters] = useState<StockFilters>(initialFilters)
  const [pagination, setPagination] = useState<StockPagination>({ current: 1, pageSize: 10 })
  const [transactionStockItem, setTransactionStockItem] = useState<StockItem | null>(null)

  const loadStockItems = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)

    try {
      setStockItems(await stockService.getStockItems())
    } catch (error: unknown) {
      setLoadError(
        error instanceof Error ? error.message : 'Stok verileri yüklenirken bir hata oluştu.',
      )
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadStockItems()
  }, [loadStockItems])

  const categoryOptions = useMemo(
    () =>
      Array.from(new Set(stockItems.map((item) => item.category)))
        .sort((first, second) => first.localeCompare(second, 'tr-TR'))
        .map((category) => ({ label: category, value: category })),
    [stockItems],
  )

  const locationOptions = useMemo(
    () =>
      Array.from(new Set(stockItems.map((item) => item.location)))
        .sort((first, second) => first.localeCompare(second, 'tr-TR'))
        .map((location) => ({ label: location, value: location })),
    [stockItems],
  )

  const filteredStockItems = useMemo(() => {
    const normalizedSearch = filters.search.trim().toLocaleLowerCase('tr-TR')

    return stockItems.filter((item) => {
      const matchesSearch =
        normalizedSearch.length === 0 ||
        [item.itemCode, item.name, item.brandModel].some((value) =>
          value.toLocaleLowerCase('tr-TR').includes(normalizedSearch),
        )

      return (
        matchesSearch &&
        (!filters.category || item.category === filters.category) &&
        (!filters.status || getStockStatus(item) === filters.status) &&
        (!filters.location || item.location === filters.location)
      )
    })
  }, [filters, stockItems])

  const updateFilters = (nextFilters: Partial<StockFilters>) => {
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

  const getActionItems = (item: StockItem): MenuProps['items'] => [
    {
      key: 'view',
      icon: <EyeOutlined />,
      label: 'Görüntüle',
      onClick: () => void navigate(`/stock/${item.id}`),
    },
    {
      key: 'movement',
      icon: <SwapOutlined />,
      label: 'Stok Hareketi',
      onClick: () => setTransactionStockItem(item),
    },
  ]

  const columns: TableColumnsType<StockItem> = [
    {
      title: 'Ürün Kodu',
      dataIndex: 'itemCode',
      key: 'itemCode',
      width: 125,
      sorter: (first, second) => first.itemCode.localeCompare(second.itemCode, 'tr-TR'),
      render: (itemCode: string) => <Typography.Text strong>{itemCode}</Typography.Text>,
    },
    {
      title: 'Ürün Adı',
      dataIndex: 'name',
      key: 'name',
      ellipsis: true,
      width: 155,
      sorter: (first, second) => first.name.localeCompare(second.name, 'tr-TR'),
    },
    {
      title: 'Kategori',
      dataIndex: 'category',
      key: 'category',
      ellipsis: true,
      responsive: ['lg'],
      width: 115,
    },
    {
      title: 'Marka / Model',
      dataIndex: 'brandModel',
      key: 'brandModel',
      ellipsis: true,
      responsive: ['xl'],
      width: 155,
    },
    {
      title: 'Birim',
      dataIndex: 'unit',
      key: 'unit',
      align: 'center',
      responsive: ['lg'],
      width: 70,
    },
    {
      title: 'Mevcut Stok',
      dataIndex: 'currentQuantity',
      key: 'currentQuantity',
      align: 'center',
      width: 100,
      sorter: (first, second) => first.currentQuantity - second.currentQuantity,
    },
    {
      title: 'Minimum Stok',
      dataIndex: 'minimumQuantity',
      key: 'minimumQuantity',
      align: 'center',
      width: 100,
      sorter: (first, second) => first.minimumQuantity - second.minimumQuantity,
    },
    {
      title: 'Lokasyon',
      dataIndex: 'location',
      key: 'location',
      ellipsis: true,
      width: 120,
    },
    {
      title: 'Durum',
      key: 'status',
      align: 'center',
      width: 85,
      render: (_value, item) => {
        const status = getStockStatus(item)
        return <Tag color={status === 'Kritik' ? 'error' : 'success'}>{status}</Tag>
      },
    },
    {
      title: 'İşlemler',
      key: 'actions',
      align: 'center',
      width: 72,
      render: (_value, item) => (
        <Dropdown menu={{ items: getActionItems(item) }} placement="bottomRight" trigger={['click']}>
          <Tooltip title="İşlemleri aç">
            <Button aria-label="Stok ürünü işlemlerini aç" icon={<MoreOutlined />} size="small" />
          </Tooltip>
        </Dropdown>
      ),
    },
  ]

  return (
    <section className="stock-items-page">
      <PageHeader
        title="Stok Durumu"
        description="Stok ürünlerinin güncel miktarlarını ve kritik seviyelerini görüntüleyin."
        actions={
          <Button icon={<PlusOutlined />} onClick={() => void navigate('/stock/new')} type="primary">
            Yeni Stok Ürünü
          </Button>
        }
      />

      <ContentCard>
        {isLoading ? (
          <LoadingState message="Stok verileri yükleniyor..." />
        ) : loadError ? (
          <ErrorState message={loadError} onRetry={() => void loadStockItems()} />
        ) : (
          <Space className="stock-items-content" direction="vertical" size="large">
            <Row gutter={[12, 12]}>
              <Col xs={24} lg={8}>
                <Input
                  allowClear
                  aria-label="Stok ürünlerinde ara"
                  onChange={(event) => updateFilters({ search: event.target.value })}
                  placeholder="Ürün kodu, ürün adı veya marka/model ara"
                  prefix={<SearchOutlined />}
                  value={filters.search}
                />
              </Col>
              <Col xs={24} sm={12} lg={4}>
                <Select<string>
                  allowClear
                  aria-label="Kategori filtresi"
                  onChange={(category) => updateFilters({ category })}
                  options={categoryOptions}
                  placeholder="Kategori"
                  value={filters.category}
                />
              </Col>
              <Col xs={24} sm={12} lg={4}>
                <Select<StockStatus>
                  allowClear
                  aria-label="Durum filtresi"
                  onChange={(status) => updateFilters({ status })}
                  options={statusOptions}
                  placeholder="Durum"
                  value={filters.status}
                />
              </Col>
              <Col xs={24} sm={12} lg={4}>
                <Select<string>
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

            <Typography.Text type="secondary">
              {filteredStockItems.length} kayıt bulundu
            </Typography.Text>

            <Table<StockItem>
              className="app-data-table"
              columns={columns}
              dataSource={filteredStockItems}
              locale={{
                emptyText: <EmptyState description="Filtrelere uygun stok ürünü bulunamadı." />,
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
              scroll={{ x: 1097 }}
              size="small"
              tableLayout="fixed"
            />
          </Space>
        )}
      </ContentCard>

      <StockTransactionModal
        onCancel={() => setTransactionStockItem(null)}
        onSuccess={loadStockItems}
        open={transactionStockItem !== null}
        stockItem={transactionStockItem}
      />
    </section>
  )
}

export default StockItemsPage
