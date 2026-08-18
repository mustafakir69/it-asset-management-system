import { ArrowLeftOutlined, EditOutlined, PlusOutlined } from '@ant-design/icons'
import { App as AntdApp, Button, DatePicker, Descriptions, Empty, Form, Modal, Popconfirm, Select, Space, Table, Tag, Typography } from 'antd'
import type { DescriptionsProps, TableColumnsType } from 'antd'
import dayjs from 'dayjs'
import type { Dayjs } from 'dayjs'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ContentCard, ErrorState, LoadingState, PageHeader } from '../../components'
import { assetService } from '../../services/assetService'
import { assignmentService } from '../../services/assignmentService'
import { licenseService } from '../../services/licenseService'
import type { Asset } from '../../types/asset'
import type { Employee } from '../../types/assignment'
import type { License, LicenseAssignment, LicenseStatus } from '../../types/license'
import { formatDate } from '../../utils'

const statusColors: Record<LicenseStatus, string> = { Aktif: 'green', Yaklaşıyor: 'orange', 'Süresi Doldu': 'red', Pasif: 'default' }
interface AssignmentFormValues { employeeId?: string; assetId?: string; assignedAt?: Dayjs }

function LicenseDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { message } = AntdApp.useApp()
  const [form] = Form.useForm<AssignmentFormValues>()
  const [license, setLicense] = useState<License | null>(null)
  const [assignments, setAssignments] = useState<LicenseAssignment[]>([])
  const [employees, setEmployees] = useState<Employee[]>([])
  const [assets, setAssets] = useState<Asset[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [isModalOpen, setIsModalOpen] = useState(false)

  const loadLicense = useCallback(async () => {
    if (!id) { setLoadError('Görüntülenecek lisans bilgisi bulunamadı.'); setIsLoading(false); return }
    setIsLoading(true); setLoadError(null)
    try {
      const [licenseData, assignmentData, employeeData, assetData] = await Promise.all([
        licenseService.getLicenseById(id), licenseService.getAssignments(id),
        assignmentService.getEmployees(), assetService.getAssets(),
      ])
      if (!licenseData) throw new Error('Aradığınız lisans bulunamadı.')
      setLicense(licenseData); setAssignments(assignmentData); setEmployees(employeeData); setAssets(assetData)
    } catch (error: unknown) {
      setLoadError(error instanceof Error ? error.message : 'Lisans bilgileri yüklenirken bir hata oluştu.')
    } finally { setIsLoading(false) }
  }, [id])

  useEffect(() => { void loadLicense() }, [loadLicense])

  const openAssignmentModal = () => {
    form.resetFields(); form.setFieldValue('assignedAt', dayjs()); setIsModalOpen(true)
  }

  const submitAssignment = async () => {
    if (!id) return
    const values = await form.validateFields()
    if (!values.employeeId && !values.assetId) { message.error('En az bir çalışan veya cihaz seçin.'); return }
    setIsSubmitting(true)
    try {
      await licenseService.createAssignment(id, { employeeId: values.employeeId, assetId: values.assetId, assignedAt: values.assignedAt!.toISOString() })
      message.success('Lisans ataması oluşturuldu.'); setIsModalOpen(false); await loadLicense()
    } catch (error: unknown) { message.error(error instanceof Error ? error.message : 'Lisans atanamadı.') }
    finally { setIsSubmitting(false) }
  }

  const revokeAssignment = async (assignmentId: string) => {
    if (!id) return
    try { await licenseService.revokeAssignment(id, assignmentId); message.success('Lisans ataması kaldırıldı; geçmiş kaydı korundu.'); await loadLicense() }
    catch (error: unknown) { message.error(error instanceof Error ? error.message : 'Lisans ataması kaldırılamadı.') }
  }

  const descriptionItems: DescriptionsProps['items'] = license ? [
    { key: 'licenseCode', label: 'Lisans Kodu', children: license.licenseCode }, { key: 'productName', label: 'Ürün Adı', children: license.productName },
    { key: 'vendor', label: 'Sağlayıcı', children: license.vendor }, { key: 'licenseType', label: 'Lisans Türü', children: license.licenseType },
    { key: 'totalSeats', label: 'Toplam Lisans Hakkı', children: license.totalSeats }, { key: 'usedSeats', label: 'Kullanılan Lisans Hakkı', children: license.usedSeats },
    { key: 'availableSeats', label: 'Kalan Lisans Hakkı', children: license.availableSeats }, { key: 'startDate', label: 'Başlangıç Tarihi', children: formatDate(license.startDate) },
    { key: 'expirationDate', label: 'Bitiş Tarihi', children: formatDate(license.expirationDate) },
    { key: 'licenseStatus', label: 'Durum', children: <Tag color={statusColors[license.licenseStatus]}>{license.licenseStatus}</Tag> },
    { key: 'notes', label: 'Not', children: license.notes || '—', span: 2 },
  ] : []

  const columns: TableColumnsType<LicenseAssignment> = [
    { title: 'Kullanıcı', dataIndex: 'employeeName', render: (value: string | null) => value ?? '—' },
    { title: 'Birim', dataIndex: 'employeeDepartment', responsive: ['md'], render: (value: string | null) => value ?? '—' },
    {
      title: 'Cihaz',
      render: (_value, item) => item.assetId && item.assetCode ? (
        <Button type="link" onClick={() => void navigate(`/assets/${item.assetId}`)}>
          {item.assetCode} — {item.assetName}
        </Button>
      ) : '—',
    },
    { title: 'Atama Tarihi', dataIndex: 'assignedAt', render: (value: string) => formatDate(value) },
    { title: 'Atayan', dataIndex: 'assignedByName', responsive: ['lg'] },
    { title: 'Durum', dataIndex: 'status', render: (value: LicenseAssignment['status']) => <Tag color={value === 'Aktif' ? 'green' : 'default'}>{value}</Tag> },
    { title: 'İşlemler', align: 'center', render: (_value, item) => item.status === 'Aktif' ? <Popconfirm cancelText="Vazgeç" okText="Kaldır" onConfirm={() => void revokeAssignment(item.id)} title="Lisans ataması kaldırılsın mı?"><Button danger size="small">Atamayı Kaldır</Button></Popconfirm> : <Typography.Text type="secondary">{formatDate(item.revokedAt)}</Typography.Text> },
  ]

  return <section>
    <PageHeader title={license?.licenseCode ?? 'Lisans Detayı'} description={license?.productName ?? 'Lisansın temel bilgileri ve kullanım durumu.'} actions={<Space wrap>
      <Button icon={<ArrowLeftOutlined />} onClick={() => void navigate('/licenses')}>Lisanslara Dön</Button>
      {license && <Button icon={<PlusOutlined />} onClick={openAssignmentModal}>Lisans Ata</Button>}
      {license && <Button icon={<EditOutlined />} onClick={() => void navigate(`/licenses/${license.id}/edit`)} type="primary">Düzenle</Button>}
    </Space>} />
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      <ContentCard title="Lisans Bilgileri">{isLoading ? <LoadingState message="Lisans bilgileri yükleniyor..." /> : loadError ? <ErrorState message={loadError} onRetry={() => void loadLicense()} /> : license ? <Descriptions bordered column={{ xs: 1, md: 2 }} items={descriptionItems} /> : null}</ContentCard>
      {!isLoading && license && <ContentCard title="Lisans Atamaları"><Table columns={columns} dataSource={assignments} locale={{ emptyText: <Empty description="Henüz lisans ataması yok." /> }} pagination={false} rowKey="id" scroll={{ x: 900 }} size="small" /></ContentCard>}
    </Space>
    <Modal cancelText="İptal" okText="Ata" confirmLoading={isSubmitting} onCancel={() => setIsModalOpen(false)} onOk={() => void submitAssignment()} open={isModalOpen} title="Lisans Ata">
      <Form form={form} layout="vertical">
        <Form.Item label="Çalışan" name="employeeId"><Select allowClear optionFilterProp="label" options={employees.map((employee) => ({ label: `${employee.fullName} — ${employee.department}`, value: employee.id }))} placeholder="Çalışan seçin" showSearch /></Form.Item>
        <Form.Item label="Cihaz" name="assetId"><Select allowClear optionFilterProp="label" options={assets.map((asset) => ({ label: `${asset.assetCode} — ${asset.brand} ${asset.model}`, value: asset.id }))} placeholder="Cihaz seçin" showSearch /></Form.Item>
        <Form.Item label="Atama Tarihi" name="assignedAt" rules={[{ required: true, message: 'Atama tarihini seçin.' }]}><DatePicker showTime style={{ width: '100%' }} /></Form.Item>
      </Form>
    </Modal>
  </section>
}

export default LicenseDetailPage
