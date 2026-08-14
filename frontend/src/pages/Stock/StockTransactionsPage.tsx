import { ClearOutlined, SearchOutlined } from '@ant-design/icons'
import { Button, Col, DatePicker, Input, Row, Select, Space, Table, Tag, Typography } from 'antd'
import type { TableColumnsType, TablePaginationConfig } from 'antd'
import type { Dayjs } from 'dayjs'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { ContentCard, EmptyState, ErrorState, LoadingState, PageHeader } from '../../components'
import { stockService } from '../../services/stockService'
import type {
  StockTransactionListItem,
  StockTransactionType,
} from '../../types/stockItem'
import { formatDate } from '../../utils'
import './StockTransactionsPage.css'

interface TransactionFilters {
  search: string
  transactionType?: StockTransactionType
  dateRange: [Dayjs | null, Dayjs | null] | null
}

interface TransactionPagination {
  current: number
  pageSize: number
}

const initialFilters: TransactionFilters = { search: '', dateRange: null }

function StockTransactionsPage() {
  const [transactions, setTransactions] = useState<StockTransactionListItem[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [filters, setFilters] = useState<TransactionFilters>(initialFilters)
  const [pagination, setPagination] = useState<TransactionPagination>({ current: 1, pageSize: 10 })

  const loadTransactions = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)

    try {
      setTransactions(await stockService.getAllStockTransactions())
    } catch (error: unknown) {
      setLoadError(
        error instanceof Error ? error.message : 'Stok hareketleri yüklenirken bir hata oluştu.',
      )
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadTransactions()
  }, [loadTransactions])

  const filteredTransactions = useMemo(() => {
    const normalizedSearch = filters.search.trim().toLocaleLowerCase('tr-TR')
    const startDate = filters.dateRange?.[0]?.startOf('day')
    const endDate = filters.dateRange?.[1]?.endOf('day')

    return transactions.filter((transaction) => {
      const matchesSearch =
        normalizedSearch.length === 0 ||
        [transaction.itemCode, transaction.itemName, transaction.personName].some((value) =>
          value.toLocaleLowerCase('tr-TR').includes(normalizedSearch),
        )
      const transactionDate = new Date(transaction.transactionDate).getTime()
      const matchesStartDate = !startDate || transactionDate >= startDate.valueOf()
      const matchesEndDate = !endDate || transactionDate <= endDate.valueOf()

      return (
        matchesSearch &&
        matchesStartDate &&
        matchesEndDate &&
        (!filters.transactionType || transaction.transactionType === filters.transactionType)
      )
    })
  }, [filters, transactions])

  const updateFilters = (nextFilters: Partial<TransactionFilters>) => {
    setFilters((current) => ({ ...current, ...nextFilters }))
    setPagination((current) => ({ ...current, current: 1 }))
  }

  const clearFilters = () => {
    setFilters(initialFilters)
    setPagination((current) => ({ ...current, current: 1 }))
  }

  const columns: TableColumnsType<StockTransactionListItem> = [
    {
      title: 'Tarih',
      dataIndex: 'transactionDate',
      key: 'transactionDate',
      width: 115,
      render: (value: string) => formatDate(value),
    },
    {
      title: 'Ürün Kodu',
      dataIndex: 'itemCode',
      key: 'itemCode',
      width: 130,
      render: (value: string) => <Typography.Text strong>{value}</Typography.Text>,
    },
    { title: 'Ürün Adı', dataIndex: 'itemName', key: 'itemName', ellipsis: true, width: 175 },
    {
      title: 'İşlem Tipi',
      dataIndex: 'transactionType',
      key: 'transactionType',
      align: 'center',
      width: 105,
      render: (value: StockTransactionType) => (
        <Tag color={value === 'Giriş' ? 'success' : 'orange'}>{value}</Tag>
      ),
    },
    { title: 'Miktar', dataIndex: 'quantity', key: 'quantity', align: 'center', width: 80 },
    {
      title: 'İşlemi Yapan / Teslim Alan',
      dataIndex: 'personName',
      key: 'personName',
      ellipsis: true,
      width: 180,
    },
    {
      title: 'Not',
      dataIndex: 'note',
      key: 'note',
      ellipsis: true,
      responsive: ['xl'],
      width: 190,
      render: (value?: string) => value || '—',
    },
  ]

  const handlePaginationChange = (nextPagination: TablePaginationConfig) => {
    setPagination({
      current: nextPagination.current ?? 1,
      pageSize: nextPagination.pageSize ?? 10,
    })
  }

  return (
    <section className="stock-transactions-page">
      <PageHeader
        title="Stok Hareketleri"
        description="Gerçekleşen tüm stok giriş ve çıkış hareketlerini görüntüleyin."
      />

      <ContentCard>
        {isLoading ? (
          <LoadingState message="Stok hareketleri yükleniyor..." />
        ) : loadError ? (
          <ErrorState message={loadError} onRetry={() => void loadTransactions()} />
        ) : (
          <Space className="stock-transactions-content" direction="vertical" size="large">
            <Row gutter={[12, 12]}>
              <Col xs={24} lg={8}>
                <Input
                  allowClear
                  aria-label="Stok hareketlerinde ara"
                  onChange={(event) => updateFilters({ search: event.target.value })}
                  placeholder="Ürün kodu, ürün adı veya kişi ara"
                  prefix={<SearchOutlined />}
                  value={filters.search}
                />
              </Col>
              <Col xs={24} sm={12} lg={5}>
                <Select<StockTransactionType>
                  allowClear
                  aria-label="İşlem tipi filtresi"
                  onChange={(transactionType) => updateFilters({ transactionType })}
                  options={[
                    { label: 'Giriş', value: 'Giriş' },
                    { label: 'Çıkış', value: 'Çıkış' },
                  ]}
                  placeholder="İşlem Tipi"
                  value={filters.transactionType}
                />
              </Col>
              <Col xs={24} sm={12} lg={7}>
                <DatePicker.RangePicker
                  aria-label="İşlem tarihi aralığı"
                  format="DD.MM.YYYY"
                  onChange={(dates) =>
                    updateFilters({ dateRange: dates ? [dates[0], dates[1]] : null })
                  }
                  placeholder={['Başlangıç Tarihi', 'Bitiş Tarihi']}
                  value={filters.dateRange}
                />
              </Col>
              <Col xs={24} lg={4}>
                <Button block icon={<ClearOutlined />} onClick={clearFilters}>
                  Filtreleri Temizle
                </Button>
              </Col>
            </Row>

            <Typography.Text type="secondary">
              {filteredTransactions.length} hareket bulundu
            </Typography.Text>

            <Table<StockTransactionListItem>
              className="app-data-table"
              columns={columns}
              dataSource={filteredTransactions}
              locale={{ emptyText: <EmptyState description="Stok hareketi bulunamadı." /> }}
              onChange={handlePaginationChange}
              pagination={{
                current: pagination.current,
                pageSize: pagination.pageSize,
                pageSizeOptions: ['10', '20', '50'],
                showSizeChanger: true,
                showTotal: (total) => `Toplam ${total} hareket`,
              }}
              rowKey="id"
              scroll={{ x: 975 }}
              size="small"
              tableLayout="fixed"
            />
          </Space>
        )}
      </ContentCard>
    </section>
  )
}

export default StockTransactionsPage
