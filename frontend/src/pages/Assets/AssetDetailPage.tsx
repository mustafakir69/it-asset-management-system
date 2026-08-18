import { ArrowLeftOutlined, EditOutlined, WarningOutlined } from '@ant-design/icons'
import {
  App as AntdApp,
  Button,
  DatePicker,
  Descriptions,
  Dropdown,
  Empty,
  Form,
  Input,
  Modal,
  Select,
  Space,
  Table,
  Tag,
  Timeline,
  Typography,
} from 'antd'
import type { DescriptionsProps, MenuProps, TableColumnsType } from 'antd'
import dayjs from 'dayjs'
import type { Dayjs } from 'dayjs'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ContentCard, ErrorState, LoadingState, PageHeader, StatusTag } from '../../components'
import { assetService } from '../../services/assetService'
import { licenseService } from '../../services/licenseService'
import { maintenanceService } from '../../services/maintenanceService'
import {
  assetDisposalMethods,
  assetScrapReasons,
  type Asset,
  type AssetDisposalMethod,
  type AssetMovement,
  type AssetScrapReason,
} from '../../types/asset'
import type { LicenseAssignment } from '../../types/license'
import type { MaintenanceTask, MaintenanceTaskStatus } from '../../types/maintenance'
import { formatDate } from '../../utils'

type LifecycleAction = 'lost' | 'scrap' | 'dispose'

interface LifecycleFormValues {
  occurredDate?: Dayjs
  description?: string
  reason?: AssetScrapReason
  method?: AssetDisposalMethod
}

const dateTimeOptions: Intl.DateTimeFormatOptions = {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
}

const actionTitles: Record<LifecycleAction, string> = {
  lost: 'Kayıp Olarak İşaretle',
  scrap: 'Hurdaya Ayır',
  dispose: 'Elden Çıkar',
}

function AssetDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { message } = AntdApp.useApp()
  const [form] = Form.useForm<LifecycleFormValues>()
  const [asset, setAsset] = useState<Asset | null>(null)
  const [movements, setMovements] = useState<AssetMovement[]>([])
  const [assignedLicenses, setAssignedLicenses] = useState<LicenseAssignment[]>([])
  const [maintenanceTasks, setMaintenanceTasks] = useState<MaintenanceTask[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [movementError, setMovementError] = useState<string | null>(null)
  const [activeAction, setActiveAction] = useState<LifecycleAction | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const loadAsset = useCallback(async () => {
    if (!id) {
      setLoadError('Görüntülenecek cihaz bilgisi bulunamadı.')
      setIsLoading(false)
      return
    }

    setIsLoading(true)
    setLoadError(null)
    setMovementError(null)

    try {
      const assetData = await assetService.getAssetById(id)
      if (!assetData) {
        setLoadError('Aradığınız cihaz envanterde bulunamadı.')
        return
      }
      setAsset(assetData)

      try {
        const [movementData, licenseData, maintenanceData] = await Promise.all([
          assetService.getAssetMovements(id),
          licenseService.getAssetAssignments(id),
          maintenanceService.getTasksByAsset(id),
        ])
        setMovements(movementData)
        setAssignedLicenses(licenseData)
        setMaintenanceTasks(maintenanceData)
      } catch (error: unknown) {
        setMovementError(
          error instanceof Error ? error.message : 'Cihaz ilişkili geçmiş bilgileri yüklenemedi.',
        )
      }
    } catch (error: unknown) {
      setLoadError(
        error instanceof Error ? error.message : 'Cihaz bilgileri yüklenirken bir hata oluştu.',
      )
    } finally {
      setIsLoading(false)
    }
  }, [id])

  useEffect(() => {
    void loadAsset()
  }, [loadAsset])

  const openLifecycleAction = (action: LifecycleAction) => {
    form.resetFields()
    form.setFieldValue('occurredDate', dayjs())
    setActiveAction(action)
  }

  const submitLifecycleAction = async () => {
    if (!id || !activeAction) return

    const values = await form.validateFields()
    const occurredDate = values.occurredDate!.format('YYYY-MM-DD')
    setIsSubmitting(true)

    try {
      if (activeAction === 'lost') {
        await assetService.markAssetLost(id, {
          lostDate: occurredDate,
          description: values.description!.trim(),
        })
      } else if (activeAction === 'scrap') {
        await assetService.scrapAsset(id, {
          scrappedDate: occurredDate,
          reason: values.reason!,
          description: values.description?.trim() || undefined,
        })
      } else {
        await assetService.disposeAsset(id, {
          disposedDate: occurredDate,
          method: values.method!,
          description: values.description?.trim() || undefined,
        })
      }

      message.success(`${actionTitles[activeAction]} işlemi kaydedildi.`)
      setActiveAction(null)
      await loadAsset()
    } catch (error: unknown) {
      message.error(error instanceof Error ? error.message : 'Cihaz durumu güncellenemedi.')
    } finally {
      setIsSubmitting(false)
    }
  }

  const descriptionItems: DescriptionsProps['items'] = asset
    ? [
        { key: 'assetCode', label: 'Varlık Kodu', children: asset.assetCode },
        { key: 'category', label: 'Kategori', children: asset.category },
        { key: 'brand', label: 'Marka', children: asset.brand },
        { key: 'model', label: 'Model', children: asset.model },
        { key: 'serialNumber', label: 'Seri Numarası', children: asset.serialNumber },
        { key: 'status', label: 'Durum', children: <StatusTag status={asset.status} /> },
        ...(asset.status === 'Zimmetli' && asset.currentAssigneeName
          ? [
              { key: 'assignee', label: 'Zimmetli Çalışan', children: asset.currentAssigneeName },
              { key: 'assigneeDepartment', label: 'Departman', children: asset.currentAssigneeDepartment ?? '—' },
              { key: 'assignmentDate', label: 'Zimmet Tarihi', children: formatDate(asset.currentAssignmentDate) },
            ]
          : []),
        { key: 'location', label: 'Lokasyon', children: asset.location },
        { key: 'purchaseDate', label: 'Satın Alma Tarihi', children: formatDate(asset.purchaseDate) },
        { key: 'warrantyEndDate', label: 'Garanti Bitiş Tarihi', children: formatDate(asset.warrantyEndDate) },
      ]
    : []

  const lifecycleMenu: MenuProps['items'] = [
    {
      key: 'lost',
      danger: true,
      disabled: asset?.status === 'Kayıp',
      label: 'Kayıp Olarak İşaretle',
      onClick: () => openLifecycleAction('lost'),
    },
    {
      key: 'scrap',
      danger: true,
      disabled: asset?.status === 'Hurda',
      label: 'Hurdaya Ayır',
      onClick: () => openLifecycleAction('scrap'),
    },
    {
      key: 'dispose',
      danger: true,
      disabled: asset?.status === 'Elden Çıkarıldı',
      label: 'Elden Çıkar',
      onClick: () => openLifecycleAction('dispose'),
    },
  ]

  const timelineItems = movements.map((movement) => ({
    color: movement.movementType.includes('Kayıp') || movement.movementType.includes('Hurda')
      ? 'red'
      : movement.movementType.includes('Zimmet')
        ? 'blue'
        : 'green',
    children: (
      <Space direction="vertical" size={2}>
        <Typography.Text type="secondary">
          {formatDate(movement.occurredAt, dateTimeOptions)}
        </Typography.Text>
        <Typography.Text strong>{movement.movementType}</Typography.Text>
        {movement.previousStatus && movement.previousStatus !== movement.newStatus && (
          <Typography.Text>
            {movement.previousStatus} → {movement.newStatus}
          </Typography.Text>
        )}
        <Typography.Text type="secondary">
          İşlemi yapan: {movement.performedByName}
        </Typography.Text>
        {movement.reason && <Typography.Text>Hurda nedeni: {movement.reason}</Typography.Text>}
        {movement.method && <Typography.Text>Yöntem: {movement.method}</Typography.Text>}
        {movement.description && <Typography.Paragraph>{movement.description}</Typography.Paragraph>}
        {movement.relatedEntityType && (
          <Typography.Text type="secondary">
            İlişkili kayıt: {movement.relatedEntityType === 'Assignment' ? 'Zimmet' : 'Bakım görevi'}
          </Typography.Text>
        )}
      </Space>
    ),
  }))

  const maintenanceColumns: TableColumnsType<MaintenanceTask> = [
    { title: 'Bakım', dataIndex: 'title' },
    { title: 'Planlanan Tarih', dataIndex: 'plannedDate', render: (value: string) => formatDate(value) },
    { title: 'Durum', dataIndex: 'displayStatus', render: (value: MaintenanceTaskStatus) => <StatusTag status={value} /> },
    { title: 'Sorumlu IT', dataIndex: 'responsibleUserName' },
    { title: 'Tamamlayan', dataIndex: 'completedByName', responsive: ['md'], render: (value: string | null) => value ?? '—' },
    { title: 'Tamamlanma Tarihi', dataIndex: 'completedDate', responsive: ['lg'], render: (value: string | null) => formatDate(value) },
    { title: 'Sonuç / Çalışma Notu', render: (_value, item) => item.result || item.workNotes || item.cancellationReason || '—', ellipsis: true },
  ]

  const licenseColumns: TableColumnsType<LicenseAssignment> = [
    {
      title: 'Lisans Kodu',
      dataIndex: 'licenseCode',
      render: (value: string, item) => (
        <Button type="link" onClick={() => void navigate(`/licenses/${item.licenseId}`)}>
          {value}
        </Button>
      ),
    },
    {
      title: 'Ürün',
      dataIndex: 'productName',
      render: (value: string, item) => (
        <Button type="link" onClick={() => void navigate(`/licenses/${item.licenseId}`)}>
          {value}
        </Button>
      ),
    },
    { title: 'Lisans Türü', dataIndex: 'licenseType', responsive: ['md'] },
    {
      title: 'Kullanıcı',
      dataIndex: 'employeeName',
      render: (value: string | null) => value ?? '—',
    },
    {
      title: 'Atama Tarihi',
      dataIndex: 'assignedAt',
      render: (value: string) => formatDate(value),
    },
    {
      title: 'Durum',
      dataIndex: 'status',
      render: (value: LicenseAssignment['status']) => (
        <Tag color={value === 'Aktif' ? 'green' : 'default'}>{value}</Tag>
      ),
    },
  ]

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
              <>
                <Dropdown menu={{ items: lifecycleMenu }} placement="bottomRight" trigger={['click']}>
                  <Button danger icon={<WarningOutlined />}>Durum İşlemleri</Button>
                </Dropdown>
                <Button
                  icon={<EditOutlined />}
                  onClick={() => void navigate(`/assets/${asset.id}/edit`)}
                  type="primary"
                >
                  Düzenle
                </Button>
              </>
            )}
          </Space>
        }
      />

      <Space direction="vertical" size="large" style={{ width: '100%' }}>
        <ContentCard title="Cihaz Bilgileri">
          {isLoading ? (
            <LoadingState message="Cihaz bilgileri yükleniyor..." />
          ) : loadError ? (
            <ErrorState message={loadError} onRetry={() => void loadAsset()} />
          ) : asset ? (
            <Descriptions bordered column={{ xs: 1, sm: 1, md: 2 }} items={descriptionItems} size="middle" />
          ) : null}
        </ContentCard>

        {!isLoading && asset && (
          <ContentCard title="Atanmış Lisanslar">
            {movementError ? (
              <ErrorState message={movementError} onRetry={() => void loadAsset()} />
            ) : (
              <Table<LicenseAssignment>
                columns={licenseColumns}
                dataSource={assignedLicenses}
                locale={{ emptyText: <Empty description="Bu cihaza atanmış aktif lisans yok." /> }}
                pagination={false}
                rowKey="id"
                scroll={{ x: 850 }}
                size="small"
              />
            )}
          </ContentCard>
        )}

        {!isLoading && asset && (
          <ContentCard title="Bakım Geçmişi">
            {movementError ? <ErrorState message={movementError} onRetry={() => void loadAsset()} /> : <Table columns={maintenanceColumns} dataSource={maintenanceTasks} locale={{ emptyText: <Empty description="Bu cihaz için bakım kaydı yok." /> }} pagination={false} rowKey="id" scroll={{ x: 1000 }} size="small" />}
          </ContentCard>
        )}

        {!isLoading && asset && (
          <ContentCard title="Hareket Geçmişi">
            {movementError ? (
              <ErrorState message={movementError} onRetry={() => void loadAsset()} />
            ) : movements.length === 0 ? (
              <Empty description="Bu cihaz için henüz hareket kaydı yok." />
            ) : (
              <Timeline items={timelineItems} />
            )}
          </ContentCard>
        )}
      </Space>

      <Modal
        cancelText="İptal"
        okButtonProps={{ danger: true, loading: isSubmitting }}
        okText="Kaydet"
        onCancel={() => setActiveAction(null)}
        onOk={() => void submitLifecycleAction()}
        open={activeAction !== null}
        title={activeAction ? actionTitles[activeAction] : undefined}
      >
        <Form<LifecycleFormValues> form={form} layout="vertical" requiredMark="optional">
          <Form.Item
            label={activeAction === 'lost' ? 'Kayıp Tarihi' : activeAction === 'scrap' ? 'Hurdaya Ayrılma Tarihi' : 'İşlem Tarihi'}
            name="occurredDate"
            rules={[{ required: true, message: 'İşlem tarihini seçin.' }]}
          >
            <DatePicker format="DD.MM.YYYY" style={{ width: '100%' }} />
          </Form.Item>

          {activeAction === 'scrap' && (
            <Form.Item label="Hurda Nedeni" name="reason" rules={[{ required: true, message: 'Hurda nedenini seçin.' }]}>
              <Select options={assetScrapReasons.map((reason) => ({ label: reason, value: reason }))} />
            </Form.Item>
          )}

          {activeAction === 'dispose' && (
            <Form.Item label="Yöntem" name="method" rules={[{ required: true, message: 'Elden çıkarma yöntemini seçin.' }]}>
              <Select options={assetDisposalMethods.map((method) => ({ label: method, value: method }))} />
            </Form.Item>
          )}

          <Form.Item
            label="Açıklama"
            name="description"
            rules={activeAction === 'lost'
              ? [{ required: true, whitespace: true, message: 'Kayıp açıklamasını girin.' }]
              : undefined}
          >
            <Input.TextArea maxLength={2000} rows={4} showCount />
          </Form.Item>
        </Form>
      </Modal>
    </section>
  )
}

export default AssetDetailPage
