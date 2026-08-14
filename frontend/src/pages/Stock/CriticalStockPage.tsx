import { EyeOutlined, MoreOutlined, SwapOutlined, WarningOutlined } from '@ant-design/icons'
import { Button, Dropdown, Space, Statistic, Table, Tooltip, Typography } from 'antd'
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
import { stockService } from '../../services/stockService'
import type { StockItem } from '../../types/stockItem'
import StockTransactionModal from './StockTransactionModal'
import './CriticalStockPage.css'

interface StockPagination {
  current: number
  pageSize: number
}

function CriticalStockPage() {
  const navigate = useNavigate()
  const [stockItems, setStockItems] = useState<StockItem[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [pagination, setPagination] = useState<StockPagination>({ current: 1, pageSize: 10 })
  const [transactionStockItem, setTransactionStockItem] = useState<StockItem | null>(null)

  const loadStockItems = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)

    try {
      setStockItems(await stockService.getStockItems())
    } catch (error: unknown) {
      setLoadError(
        error instanceof Error ? error.message : 'Kritik stok kayıtları yüklenirken bir hata oluştu.',
      )
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadStockItems()
  }, [loadStockItems])

  const criticalStockItems = useMemo(
    () => stockItems.filter((item) => item.currentQuantity <= item.minimumQuantity),
    [stockItems],
  )

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
      width: 130,
      render: (value: string) => <Typography.Text strong>{value}</Typography.Text>,
    },
    { title: 'Ürün Adı', dataIndex: 'name', key: 'name', ellipsis: true, width: 180 },
    {
      title: 'Mevcut Stok',
      dataIndex: 'currentQuantity',
      key: 'currentQuantity',
      align: 'center',
      width: 115,
    },
    {
      title: 'Minimum Stok',
      dataIndex: 'minimumQuantity',
      key: 'minimumQuantity',
      align: 'center',
      width: 115,
    },
    { title: 'Lokasyon', dataIndex: 'location', key: 'location', ellipsis: true, width: 145 },
    {
      title: 'Durum',
      key: 'status',
      align: 'center',
      width: 95,
      render: () => <StatusTag status="Kritik" />,
    },
    {
      title: 'İşlemler',
      key: 'actions',
      align: 'center',
      width: 72,
      render: (_value, item) => (
        <Dropdown menu={{ items: getActionItems(item) }} placement="bottomRight" trigger={['click']}>
          <Tooltip title="İşlemleri aç">
            <Button aria-label="Kritik stok işlemlerini aç" icon={<MoreOutlined />} size="small" />
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
    <section className="critical-stock-page">
      <PageHeader
        title="Kritik Stoklar"
        description="Minimum stok seviyesine ulaşan veya bu seviyenin altına düşen ürünleri görüntüleyin."
      />

      {isLoading ? (
        <ContentCard>
          <LoadingState message="Kritik stok kayıtları yükleniyor..." />
        </ContentCard>
      ) : loadError ? (
        <ContentCard>
          <ErrorState message={loadError} onRetry={() => void loadStockItems()} />
        </ContentCard>
      ) : (
        <Space className="critical-stock-content" direction="vertical" size="large">
          <ContentCard>
            <Statistic
              prefix={<WarningOutlined />}
              title="Kritik Stok Sayısı"
              value={criticalStockItems.length}
              valueStyle={{ color: '#cf1322' }}
            />
          </ContentCard>
          <ContentCard>
            <Table<StockItem>
              className="app-data-table"
              columns={columns}
              dataSource={criticalStockItems}
              locale={{ emptyText: <EmptyState description="Kritik seviyede stok ürünü bulunmuyor." /> }}
              onChange={handlePaginationChange}
              pagination={{
                current: pagination.current,
                pageSize: pagination.pageSize,
                pageSizeOptions: ['10', '20', '50'],
                showSizeChanger: true,
                showTotal: (total) => `Toplam ${total} kritik ürün`,
              }}
              rowKey="id"
              scroll={{ x: 852 }}
              size="small"
              tableLayout="fixed"
            />
          </ContentCard>
        </Space>
      )}

      <StockTransactionModal
        onCancel={() => setTransactionStockItem(null)}
        onSuccess={loadStockItems}
        open={transactionStockItem !== null}
        stockItem={transactionStockItem}
      />
    </section>
  )
}

export default CriticalStockPage
