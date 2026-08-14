import {
  CheckCircleOutlined,
  ClearOutlined,
  ClockCircleOutlined,
  CloseCircleOutlined,
  EyeOutlined,
  QuestionCircleOutlined,
  SearchOutlined,
} from '@ant-design/icons'
import {
  Button,
  Col,
  Input,
  Row,
  Select,
  Space,
  Statistic,
  Table,
  Tag,
  Typography,
} from 'antd'
import type { TableColumnsType, TablePaginationConfig } from 'antd'
import type { ReactNode } from 'react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ContentCard, EmptyState, ErrorState, LoadingState, PageHeader } from '../../components'
import { warrantyService } from '../../services/warrantyService'
import {
  warrantyStatuses,
  type WarrantyAsset,
  type WarrantyStatus,
} from '../../types/warranty'
import { formatDate } from '../../utils'
import './WarrantiesPage.css'

interface WarrantyFilters {
  search: string
  status?: WarrantyStatus
}

interface WarrantyPagination {
  current: number
  pageSize: number
}

interface WarrantyStatusPresentation {
  color: string
  icon: ReactNode
}

const initialFilters: WarrantyFilters = { search: '' }

const statusPresentations: Record<WarrantyStatus, WarrantyStatusPresentation> = {
  Aktif: { color: 'green', icon: <CheckCircleOutlined /> },
  Yaklaşıyor: { color: 'orange', icon: <ClockCircleOutlined /> },
  'Süresi Doldu': { color: 'red', icon: <CloseCircleOutlined /> },
  'Garanti Bilgisi Yok': { color: 'default', icon: <QuestionCircleOutlined /> },
}

const summaryDefinitions: Array<{ status: WarrantyStatus; title: string }> = [
  { status: 'Aktif', title: 'Aktif Garantiler' },
  { status: 'Yaklaşıyor', title: '30 Gün İçinde Bitecek' },
  { status: 'Süresi Doldu', title: 'Süresi Dolan' },
  { status: 'Garanti Bilgisi Yok', title: 'Garanti Bilgisi Olmayan' },
]

const statusOptions = warrantyStatuses.map((status) => ({ label: status, value: status }))

const compareNullableNumber = (first: number | null, second: number | null): number => {
  if (first === null && second === null) return 0
  if (first === null) return 1
  if (second === null) return -1
  return first - second
}

const compareNullableDate = (first: string | null, second: string | null): number => {
  if (first === null && second === null) return 0
  if (first === null) return 1
  if (second === null) return -1
  return first.localeCompare(second)
}

const formatRemainingDays = (remainingDays: number | null): string => {
  if (remainingDays === null) return '—'
  if (remainingDays < 0) return `${Math.abs(remainingDays)} gün geçti`
  if (remainingDays === 0) return 'Bugün sona eriyor'
  return `${remainingDays} gün`
}

function WarrantyStatusTag({ status }: { status: WarrantyStatus }) {
  const presentation = statusPresentations[status]
  return (
    <Tag color={presentation.color} icon={presentation.icon}>
      {status}
    </Tag>
  )
}

function WarrantiesPage() {
  const navigate = useNavigate()
  const [warranties, setWarranties] = useState<WarrantyAsset[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [filters, setFilters] = useState<WarrantyFilters>(initialFilters)
  const [pagination, setPagination] = useState<WarrantyPagination>({ current: 1, pageSize: 10 })

  const loadWarranties = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)

    try {
      setWarranties(await warrantyService.getWarranties())
    } catch (error: unknown) {
      setLoadError(
        error instanceof Error ? error.message : 'Garanti verileri yüklenirken bir hata oluştu.',
      )
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadWarranties()
  }, [loadWarranties])

  const summaryCounts = useMemo(
    () =>
      warrantyStatuses.reduce<Record<WarrantyStatus, number>>(
        (counts, status) => ({
          ...counts,
          [status]: warranties.filter((item) => item.warrantyStatus === status).length,
        }),
        { Aktif: 0, Yaklaşıyor: 0, 'Süresi Doldu': 0, 'Garanti Bilgisi Yok': 0 },
      ),
    [warranties],
  )

  const filteredWarranties = useMemo(() => {
    const normalizedSearch = filters.search.trim().toLocaleLowerCase('tr-TR')

    return warranties.filter((item) => {
      const matchesSearch =
        normalizedSearch.length === 0 ||
        [item.assetCode, item.serialNumber, item.brand, item.model].some((value) =>
          value.toLocaleLowerCase('tr-TR').includes(normalizedSearch),
        )

      return matchesSearch && (!filters.status || item.warrantyStatus === filters.status)
    })
  }, [filters, warranties])

  const updateFilters = (nextFilters: Partial<WarrantyFilters>) => {
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

  const columns: TableColumnsType<WarrantyAsset> = [
    {
      title: 'Cihaz Kodu',
      dataIndex: 'assetCode',
      key: 'assetCode',
      width: 145,
      sorter: (first, second) => first.assetCode.localeCompare(second.assetCode, 'tr-TR'),
      render: (value: string) => <Typography.Text strong>{value}</Typography.Text>,
    },
    {
      title: 'Cihaz',
      key: 'device',
      width: 210,
      render: (_value, item) => (
        <Space direction="vertical" size={0}>
          <Typography.Text strong>{item.brand}</Typography.Text>
          <Typography.Text type="secondary">{item.model}</Typography.Text>
        </Space>
      ),
    },
    { title: 'Seri No', dataIndex: 'serialNumber', key: 'serialNumber', width: 170 },
    { title: 'Lokasyon', dataIndex: 'location', key: 'location', width: 150 },
    {
      title: 'Satın Alma Tarihi',
      dataIndex: 'purchaseDate',
      key: 'purchaseDate',
      align: 'center',
      width: 145,
      render: (value: string) => formatDate(value),
    },
    {
      title: 'Garanti Bitiş Tarihi',
      dataIndex: 'warrantyEndDate',
      key: 'warrantyEndDate',
      align: 'center',
      width: 160,
      sorter: (first, second) =>
        compareNullableDate(first.warrantyEndDate, second.warrantyEndDate),
      render: (value: string | null) => formatDate(value),
    },
    {
      title: 'Kalan Gün',
      dataIndex: 'remainingDays',
      key: 'remainingDays',
      align: 'center',
      width: 130,
      sorter: (first, second) =>
        compareNullableNumber(first.remainingDays, second.remainingDays),
      render: (value: number | null) => formatRemainingDays(value),
    },
    {
      title: 'Durum',
      dataIndex: 'warrantyStatus',
      key: 'warrantyStatus',
      align: 'center',
      width: 155,
      render: (status: WarrantyStatus) => <WarrantyStatusTag status={status} />,
    },
    {
      title: 'İşlemler',
      key: 'actions',
      align: 'center',
      fixed: 'right',
      width: 120,
      render: (_value, item) => (
        <Button
          icon={<EyeOutlined />}
          onClick={() => void navigate(`/assets/${item.assetId}`)}
          type="link"
        >
          Görüntüle
        </Button>
      ),
    },
  ]

  return (
    <section className="warranties-page">
      <PageHeader
        title="Garanti Takibi"
        description="Cihaz garantilerinin güncel durumunu ve yaklaşan bitiş tarihlerini izleyin."
      />

      {isLoading ? (
        <ContentCard>
          <LoadingState message="Garanti verileri yükleniyor..." />
        </ContentCard>
      ) : loadError ? (
        <ContentCard>
          <ErrorState message={loadError} onRetry={() => void loadWarranties()} />
        </ContentCard>
      ) : (
        <Space className="warranties-content" direction="vertical" size="large">
          <Row gutter={[16, 16]}>
            {summaryDefinitions.map(({ status, title }) => {
              const presentation = statusPresentations[status]
              return (
                <Col key={status} xs={24} sm={12} xl={6}>
                  <ContentCard>
                    <Statistic
                      prefix={<span style={{ color: presentation.color }}>{presentation.icon}</span>}
                      title={title}
                      value={summaryCounts[status]}
                      valueStyle={{
                        color: presentation.color === 'default' ? undefined : presentation.color,
                      }}
                    />
                  </ContentCard>
                </Col>
              )
            })}
          </Row>

          <ContentCard>
            <Space className="warranties-table-content" direction="vertical" size="large">
              <Row gutter={[12, 12]}>
                <Col xs={24} md={12} xl={10}>
                  <Input
                    allowClear
                    aria-label="Garanti kayıtlarında ara"
                    onChange={(event) => updateFilters({ search: event.target.value })}
                    placeholder="Cihaz kodu, seri no, marka veya model ara"
                    prefix={<SearchOutlined />}
                    value={filters.search}
                  />
                </Col>
                <Col xs={24} sm={12} md={6} xl={7}>
                  <Select<WarrantyStatus>
                    allowClear
                    aria-label="Garanti durumu filtresi"
                    onChange={(status) => updateFilters({ status })}
                    options={statusOptions}
                    placeholder="Garanti Durumu"
                    value={filters.status}
                  />
                </Col>
                <Col xs={24} sm={12} md={6} xl={7}>
                  <Button block icon={<ClearOutlined />} onClick={clearFilters}>
                    Filtreleri Temizle
                  </Button>
                </Col>
              </Row>

              <Typography.Text type="secondary">
                {filteredWarranties.length} kayıt bulundu
              </Typography.Text>

              <Table<WarrantyAsset>
                columns={columns}
                dataSource={filteredWarranties}
                locale={{
                  emptyText: <EmptyState description="Filtrelere uygun garanti kaydı bulunamadı." />,
                }}
                onChange={handlePaginationChange}
                pagination={{
                  current: pagination.current,
                  pageSize: pagination.pageSize,
                  pageSizeOptions: ['10', '20', '50'],
                  showSizeChanger: true,
                  showTotal: (total) => `Toplam ${total} kayıt`,
                }}
                rowClassName={(item) =>
                  item.warrantyStatus === 'Yaklaşıyor' ? 'warranty-row-approaching' : ''
                }
                rowKey="assetId"
                scroll={{ x: 1335 }}
              />
            </Space>
          </ContentCard>
        </Space>
      )}
    </section>
  )
}

export default WarrantiesPage
