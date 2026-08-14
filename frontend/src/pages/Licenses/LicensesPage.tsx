import {
  CheckCircleOutlined,
  ClearOutlined,
  ClockCircleOutlined,
  CloseCircleOutlined,
  EditOutlined,
  EyeOutlined,
  FileProtectOutlined,
  MoreOutlined,
  PlusOutlined,
  SearchOutlined,
  StopOutlined,
  TeamOutlined,
} from '@ant-design/icons'
import {
  Button,
  Col,
  Dropdown,
  Input,
  Row,
  Select,
  Space,
  Statistic,
  Table,
  Tag,
  Tooltip,
  Typography,
} from 'antd'
import type { MenuProps, TableColumnsType, TablePaginationConfig } from 'antd'
import type { ReactNode } from 'react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ContentCard, EmptyState, ErrorState, LoadingState, PageHeader } from '../../components'
import { licenseService } from '../../services/licenseService'
import { licenseStatuses, type License, type LicenseStatus } from '../../types/license'
import { formatDate } from '../../utils'
import './LicensesPage.css'

interface LicenseFilters {
  search: string
  status?: LicenseStatus
}

interface LicensePagination {
  current: number
  pageSize: number
}

interface SummaryItem {
  key: string
  title: string
  value: number
  color: string
  icon: ReactNode
}

const initialFilters: LicenseFilters = { search: '' }

const statusOptions = licenseStatuses.map((status) => ({ label: status, value: status }))

const statusColors: Record<LicenseStatus, string> = {
  Aktif: 'green',
  Yaklaşıyor: 'orange',
  'Süresi Doldu': 'red',
  Pasif: 'default',
}

const compareNullableDate = (first: string | null, second: string | null): number => {
  if (first === null && second === null) return 0
  if (first === null) return 1
  if (second === null) return -1
  return first.localeCompare(second)
}

function LicensesPage() {
  const navigate = useNavigate()
  const [licenses, setLicenses] = useState<License[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [filters, setFilters] = useState<LicenseFilters>(initialFilters)
  const [pagination, setPagination] = useState<LicensePagination>({ current: 1, pageSize: 10 })

  const loadLicenses = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)

    try {
      setLicenses(await licenseService.getLicenses())
    } catch (error: unknown) {
      setLoadError(
        error instanceof Error ? error.message : 'Lisans verileri yüklenirken bir hata oluştu.',
      )
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadLicenses()
  }, [loadLicenses])

  const summaries = useMemo<SummaryItem[]>(() => {
    const countByStatus = (status: LicenseStatus) =>
      licenses.filter((license) => license.licenseStatus === status).length

    return [
      {
        key: 'total',
        title: 'Toplam Lisans',
        value: licenses.length,
        color: '#1677ff',
        icon: <FileProtectOutlined />,
      },
      {
        key: 'active',
        title: 'Aktif Lisans',
        value: countByStatus('Aktif'),
        color: '#389e0d',
        icon: <CheckCircleOutlined />,
      },
      {
        key: 'approaching',
        title: '30 Gün İçinde Bitecek',
        value: countByStatus('Yaklaşıyor'),
        color: '#d48806',
        icon: <ClockCircleOutlined />,
      },
      {
        key: 'expired',
        title: 'Süresi Dolmuş',
        value: countByStatus('Süresi Doldu'),
        color: '#cf1322',
        icon: <CloseCircleOutlined />,
      },
      {
        key: 'rights',
        title: 'Toplam Lisans Hakkı',
        value: licenses.reduce((total, license) => total + license.totalSeats, 0),
        color: '#531dab',
        icon: <TeamOutlined />,
      },
      {
        key: 'used',
        title: 'Kullanılan Lisans Hakkı',
        value: licenses.reduce((total, license) => total + license.usedSeats, 0),
        color: '#08979c',
        icon: <StopOutlined />,
      },
    ]
  }, [licenses])

  const filteredLicenses = useMemo(() => {
    const normalizedSearch = filters.search.trim().toLocaleLowerCase('tr-TR')

    return licenses.filter((license) => {
      const matchesSearch =
        normalizedSearch.length === 0 ||
        [license.licenseCode, license.productName, license.vendor].some((value) =>
          value.toLocaleLowerCase('tr-TR').includes(normalizedSearch),
        )

      return matchesSearch && (!filters.status || license.licenseStatus === filters.status)
    })
  }, [filters, licenses])

  const updateFilters = (nextFilters: Partial<LicenseFilters>) => {
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

  const getActionItems = (license: License): MenuProps['items'] => [
    {
      key: 'view',
      icon: <EyeOutlined />,
      label: 'Görüntüle',
      onClick: () => void navigate(`/licenses/${license.id}`),
    },
    {
      key: 'edit',
      icon: <EditOutlined />,
      label: 'Düzenle',
      onClick: () => void navigate(`/licenses/${license.id}/edit`),
    },
  ]

  const columns: TableColumnsType<License> = [
    {
      title: 'Lisans Kodu',
      dataIndex: 'licenseCode',
      key: 'licenseCode',
      width: 125,
      sorter: (first, second) => first.licenseCode.localeCompare(second.licenseCode, 'tr-TR'),
      render: (value: string) => <Typography.Text strong>{value}</Typography.Text>,
    },
    {
      title: 'Ürün',
      dataIndex: 'productName',
      key: 'productName',
      ellipsis: true,
      width: 170,
      sorter: (first, second) => first.productName.localeCompare(second.productName, 'tr-TR'),
    },
    { title: 'Sağlayıcı', dataIndex: 'vendor', key: 'vendor', ellipsis: true, responsive: ['lg'], width: 105 },
    { title: 'Lisans Türü', dataIndex: 'licenseType', key: 'licenseType', ellipsis: true, responsive: ['xl'], width: 115 },
    {
      title: <span>Toplam Lisans<br />Hakkı</span>,
      dataIndex: 'totalSeats',
      key: 'totalSeats',
      align: 'center',
      width: 105,
    },
    {
      title: 'Kullanılan',
      dataIndex: 'usedSeats',
      key: 'usedSeats',
      align: 'center',
      width: 85,
    },
    {
      title: 'Kalan',
      dataIndex: 'availableSeats',
      key: 'availableSeats',
      align: 'center',
      width: 75,
      sorter: (first, second) => first.availableSeats - second.availableSeats,
    },
    {
      title: 'Başlangıç',
      dataIndex: 'startDate',
      key: 'startDate',
      align: 'center',
      responsive: ['xl'],
      width: 105,
      render: (value: string) => formatDate(value),
    },
    {
      title: 'Bitiş',
      dataIndex: 'expirationDate',
      key: 'expirationDate',
      align: 'center',
      width: 105,
      sorter: (first, second) => compareNullableDate(first.expirationDate, second.expirationDate),
      render: (value: string | null) => formatDate(value),
    },
    {
      title: 'Durum',
      dataIndex: 'licenseStatus',
      key: 'licenseStatus',
      align: 'center',
      width: 110,
      render: (status: LicenseStatus) => <Tag color={statusColors[status]}>{status}</Tag>,
    },
    {
      title: 'İşlemler',
      key: 'actions',
      align: 'center',
      width: 72,
      render: (_value, license) => (
        <Dropdown menu={{ items: getActionItems(license) }} placement="bottomRight" trigger={['click']}>
          <Tooltip title="İşlemleri aç">
            <Button aria-label="Lisans işlemlerini aç" icon={<MoreOutlined />} size="small" />
          </Tooltip>
        </Dropdown>
      ),
    },
  ]

  return (
    <section className="licenses-page">
      <PageHeader
        title="Lisanslar"
        description="Kurumsal yazılım lisanslarını, kullanım haklarını ve bitiş tarihlerini yönetin."
        actions={
          <Button icon={<PlusOutlined />} onClick={() => void navigate('/licenses/new')} type="primary">
            Yeni Lisans
          </Button>
        }
      />

      {isLoading ? (
        <ContentCard>
          <LoadingState message="Lisans verileri yükleniyor..." />
        </ContentCard>
      ) : loadError ? (
        <ContentCard>
          <ErrorState message={loadError} onRetry={() => void loadLicenses()} />
        </ContentCard>
      ) : (
        <Space className="licenses-content" direction="vertical" size="large">
          <Row gutter={[16, 16]}>
            {summaries.map((summary) => (
              <Col key={summary.key} xs={24} sm={12} xl={8} xxl={4}>
                <ContentCard>
                  <Statistic
                    prefix={<span style={{ color: summary.color }}>{summary.icon}</span>}
                    title={summary.title}
                    value={summary.value}
                    valueStyle={{ color: summary.color }}
                  />
                </ContentCard>
              </Col>
            ))}
          </Row>

          <ContentCard>
            <Space className="licenses-table-content" direction="vertical" size="large">
              <Row gutter={[12, 12]}>
                <Col xs={24} md={12} xl={10}>
                  <Input
                    allowClear
                    aria-label="Lisanslarda ara"
                    onChange={(event) => updateFilters({ search: event.target.value })}
                    placeholder="Lisans kodu, ürün veya sağlayıcı ara"
                    prefix={<SearchOutlined />}
                    value={filters.search}
                  />
                </Col>
                <Col xs={24} sm={12} md={6} xl={7}>
                  <Select<LicenseStatus>
                    allowClear
                    aria-label="Lisans durumu filtresi"
                    onChange={(status) => updateFilters({ status })}
                    options={statusOptions}
                    placeholder="Lisans Durumu"
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
                {filteredLicenses.length} kayıt bulundu
              </Typography.Text>

              <Table<License>
                className="app-data-table"
                columns={columns}
                dataSource={filteredLicenses}
                locale={{ emptyText: <EmptyState description="Filtrelere uygun lisans bulunamadı." /> }}
                onChange={handlePaginationChange}
                pagination={{
                  current: pagination.current,
                  pageSize: pagination.pageSize,
                  pageSizeOptions: ['10', '20', '50'],
                  showSizeChanger: true,
                  showTotal: (total) => `Toplam ${total} kayıt`,
                }}
                rowKey="id"
                scroll={{ x: 1172 }}
                size="small"
                tableLayout="fixed"
              />
            </Space>
          </ContentCard>
        </Space>
      )}
    </section>
  )
}

export default LicensesPage
