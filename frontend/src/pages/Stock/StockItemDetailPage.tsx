import { ArrowLeftOutlined } from '@ant-design/icons'
import { Button, Descriptions, Space, Table, Tag } from 'antd'
import type { DescriptionsProps, TableColumnsType } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ContentCard, EmptyState, ErrorState, LoadingState, PageHeader } from '../../components'
import { stockService } from '../../services/stockService'
import type { StockItem, StockTransaction } from '../../types/stockItem'
import { formatDate } from '../../utils'

function StockItemDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [stockItem, setStockItem] = useState<StockItem | null>(null)
  const [transactions, setTransactions] = useState<StockTransaction[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)

  const loadStockItem = useCallback(async () => {
    if (!id) {
      setLoadError('Görüntülenecek stok ürünü bulunamadı.')
      setIsLoading(false)
      return
    }

    setIsLoading(true)
    setLoadError(null)

    try {
      const item = await stockService.getStockItemById(id)

      if (!item) {
        setLoadError('Aradığınız stok ürünü bulunamadı.')
        return
      }

      const movementHistory = await stockService.getStockTransactions(id)
      setStockItem(item)
      setTransactions(movementHistory)
    } catch (error: unknown) {
      setLoadError(error instanceof Error ? error.message : 'Stok ürünü bilgileri yüklenemedi.')
    } finally {
      setIsLoading(false)
    }
  }, [id])

  useEffect(() => {
    void loadStockItem()
  }, [loadStockItem])

  const descriptionItems: DescriptionsProps['items'] = stockItem
    ? [
        { key: 'itemCode', label: 'Ürün Kodu', children: stockItem.itemCode },
        { key: 'name', label: 'Ürün Adı', children: stockItem.name },
        { key: 'category', label: 'Kategori', children: stockItem.category },
        { key: 'brandModel', label: 'Marka / Model', children: stockItem.brandModel || '—' },
        { key: 'unit', label: 'Birim', children: stockItem.unit },
        { key: 'currentQuantity', label: 'Mevcut Stok', children: stockItem.currentQuantity },
        { key: 'minimumQuantity', label: 'Minimum Stok', children: stockItem.minimumQuantity },
        { key: 'location', label: 'Lokasyon', children: stockItem.location },
        {
          key: 'status',
          label: 'Durum',
          children: (
            <Tag color={stockItem.isCritical ? 'error' : 'success'}>
              {stockItem.isCritical ? 'Kritik' : 'Normal'}
            </Tag>
          ),
        },
      ]
    : []

  const columns: TableColumnsType<StockTransaction> = [
    {
      title: 'İşlem Tipi',
      dataIndex: 'transactionType',
      key: 'transactionType',
      width: 100,
      render: (value: StockTransaction['transactionType']) => (
        <Tag color={value === 'Giriş' ? 'success' : 'orange'}>{value}</Tag>
      ),
    },
    { title: 'Miktar', dataIndex: 'quantity', key: 'quantity', width: 80 },
    {
      title: 'İşlem Tarihi',
      dataIndex: 'transactionDate',
      key: 'transactionDate',
      width: 120,
      render: (value: string) => formatDate(value),
    },
    { title: 'İşlemi Yapan', dataIndex: 'performedByName', key: 'performedByName', ellipsis: true, width: 160 },
    { title: 'Teslim Alan', dataIndex: 'recipientEmployeeName', key: 'recipientEmployeeName', ellipsis: true, width: 160, render: (value: string | null) => value ?? '—' },
    { title: 'Not', dataIndex: 'note', key: 'note', ellipsis: true, render: (value?: string) => value || '—' },
  ]

  return (
    <section>
      <PageHeader
        title={stockItem ? stockItem.itemCode : 'Stok Ürünü Detayı'}
        description={stockItem ? stockItem.name : 'Stok ürünü ve hareket geçmişi.'}
        actions={
          <Button icon={<ArrowLeftOutlined />} onClick={() => void navigate('/stock')}>
            Stok Listesine Dön
          </Button>
        }
      />

      {isLoading ? (
        <ContentCard>
          <LoadingState message="Stok ürünü bilgileri yükleniyor..." />
        </ContentCard>
      ) : loadError ? (
        <ContentCard>
          <ErrorState message={loadError} onRetry={() => void loadStockItem()} />
        </ContentCard>
      ) : stockItem ? (
        <Space direction="vertical" size="large" style={{ width: '100%' }}>
          <ContentCard title="Ürün Bilgileri">
            <Descriptions
              bordered
              column={{ xs: 1, sm: 1, md: 2 }}
              items={descriptionItems}
              size="middle"
            />
          </ContentCard>
          <ContentCard title="Son Stok Hareketleri">
            <Table<StockTransaction>
              className="app-data-table"
              columns={columns}
              dataSource={transactions}
              locale={{ emptyText: <EmptyState description="Bu ürün için stok hareketi bulunmuyor." /> }}
              pagination={{ pageSize: 10, showSizeChanger: false }}
              rowKey="id"
              scroll={{ x: 680 }}
              size="small"
              tableLayout="fixed"
            />
          </ContentCard>
        </Space>
      ) : null}
    </section>
  )
}

export default StockItemDetailPage
