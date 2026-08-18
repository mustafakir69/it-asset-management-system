import { Button, Col, DatePicker, Form, Input, InputNumber, Row, Select, Space } from 'antd'
import dayjs from 'dayjs'
import type { Dayjs } from 'dayjs'
import { useEffect } from 'react'
import type { License, LicenseInput } from '../../types/license'
import './LicenseForm.css'

interface LicenseFormValues {
  licenseCode: string
  productName: string
  vendor: string
  licenseType: string
  totalSeats: number
  startDate?: Dayjs
  expirationDate?: Dayjs
  isActive: 'Aktif' | 'Pasif'
  notes?: string
}

export interface LicenseFormProps {
  initialValues?: License
  isSubmitting?: boolean
  submitLabel: string
  onCancel: () => void
  onSubmit: (values: LicenseInput) => Promise<void>
}

function LicenseForm({
  initialValues,
  isSubmitting = false,
  submitLabel,
  onCancel,
  onSubmit,
}: LicenseFormProps) {
  const [form] = Form.useForm<LicenseFormValues>()

  useEffect(() => {
    form.resetFields()
    form.setFieldsValue({ totalSeats: 0, isActive: 'Aktif' })

    if (initialValues) {
      form.setFieldsValue({
        ...initialValues,
        isActive: initialValues.isActive ? 'Aktif' : 'Pasif',
        startDate: dayjs(initialValues.startDate),
        expirationDate: initialValues.expirationDate
          ? dayjs(initialValues.expirationDate)
          : undefined,
      })
    }
  }, [form, initialValues])

  const validateExpirationDate = (_rule: unknown, expirationDate?: Dayjs) => {
    const startDate = form.getFieldValue('startDate') as Dayjs | undefined

    if (startDate && expirationDate?.isBefore(startDate, 'day')) {
      return Promise.reject(new Error('Bitiş tarihi başlangıç tarihinden önce olamaz.'))
    }

    return Promise.resolve()
  }

  const handleFinish = async (values: LicenseFormValues) => {
    await onSubmit({
      licenseCode: values.licenseCode.trim(),
      productName: values.productName.trim(),
      vendor: values.vendor.trim(),
      licenseType: values.licenseType.trim(),
      totalSeats: values.totalSeats,
      startDate: values.startDate?.format('YYYY-MM-DD') ?? '',
      expirationDate: values.expirationDate?.format('YYYY-MM-DD') ?? null,
      isActive: values.isActive === 'Aktif',
      notes: values.notes?.trim() || undefined,
    })
  }

  return (
    <Form<LicenseFormValues>
      className="license-form"
      form={form}
      layout="vertical"
      onFinish={(values) => void handleFinish(values)}
      requiredMark="optional"
    >
      <Row gutter={[16, 0]}>
        <Col xs={24} md={12}>
          <Form.Item
            label="Lisans Kodu"
            name="licenseCode"
            rules={[{ required: true, whitespace: true, message: 'Lisans kodunu girin.' }]}
          >
            <Input maxLength={50} placeholder="Örn. LIC-M365-011" />
          </Form.Item>
        </Col>
        <Col xs={24} md={12}>
          <Form.Item
            label="Ürün Adı"
            name="productName"
            rules={[{ required: true, whitespace: true, message: 'Ürün adını girin.' }]}
          >
            <Input maxLength={150} placeholder="Örn. Microsoft 365 Business Premium" />
          </Form.Item>
        </Col>
        <Col xs={24} md={12}>
          <Form.Item
            label="Sağlayıcı"
            name="vendor"
            rules={[{ required: true, whitespace: true, message: 'Sağlayıcı bilgisini girin.' }]}
          >
            <Input maxLength={100} placeholder="Örn. Microsoft" />
          </Form.Item>
        </Col>
        <Col xs={24} md={12}>
          <Form.Item
            label="Lisans Türü"
            name="licenseType"
            rules={[{ required: true, whitespace: true, message: 'Lisans türünü girin.' }]}
          >
            <Input maxLength={100} placeholder="Örn. Yıllık Abonelik" />
          </Form.Item>
        </Col>
        <Col xs={24} md={12}>
          <Form.Item
            label="Toplam Lisans Hakkı"
            name="totalSeats"
            rules={[
              { required: true, message: 'Toplam lisans hakkını girin.' },
              { type: 'number', min: 0, message: 'Toplam lisans hakkı negatif olamaz.' },
            ]}
          >
            <InputNumber min={0} precision={0} />
          </Form.Item>
        </Col>
        <Col xs={24} md={12}>
          <Form.Item
            label="Başlangıç Tarihi"
            name="startDate"
            rules={[{ required: true, message: 'Başlangıç tarihini seçin.' }]}
          >
            <DatePicker format="DD.MM.YYYY" placeholder="Başlangıç tarihini seçin" />
          </Form.Item>
        </Col>
        <Col xs={24} md={12}>
          <Form.Item
            dependencies={['startDate']}
            label="Bitiş Tarihi"
            name="expirationDate"
            rules={[{ validator: validateExpirationDate }]}
          >
            <DatePicker allowClear format="DD.MM.YYYY" placeholder="Bitiş tarihini seçin" />
          </Form.Item>
        </Col>
        <Col xs={24} md={12}>
          <Form.Item
            label="Kayıt Durumu"
            name="isActive"
            rules={[{ required: true, message: 'Kayıt durumunu seçin.' }]}
          >
            <Select<'Aktif' | 'Pasif'>
              options={[
                { label: 'Aktif', value: 'Aktif' },
                { label: 'Pasif', value: 'Pasif' },
              ]}
            />
          </Form.Item>
        </Col>
        <Col xs={24}>
          <Form.Item label="Not" name="notes">
            <Input.TextArea maxLength={1000} rows={4} showCount />
          </Form.Item>
        </Col>
      </Row>

      <div className="license-form-actions">
        <Space wrap>
          <Button disabled={isSubmitting} onClick={onCancel}>
            İptal
          </Button>
          <Button htmlType="submit" loading={isSubmitting} type="primary">
            {submitLabel}
          </Button>
        </Space>
      </div>
    </Form>
  )
}

export default LicenseForm
