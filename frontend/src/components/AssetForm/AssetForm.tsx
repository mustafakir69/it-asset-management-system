import { Button, Col, DatePicker, Form, Input, Row, Select, Space } from 'antd'
import dayjs from 'dayjs'
import type { Dayjs } from 'dayjs'
import { useEffect } from 'react'
import {
  assetCategories,
  assetLocations,
  assetStatuses,
  type Asset,
  type AssetCategory,
  type AssetInput,
  type AssetLocation,
  type AssetStatus,
} from '../../types/asset'
import './AssetForm.css'

interface AssetFormValues {
  assetCode: string
  category: AssetCategory
  brand: string
  model: string
  serialNumber: string
  status: AssetStatus
  location: AssetLocation
  purchaseDate?: Dayjs
  warrantyEndDate?: Dayjs
}

export interface AssetFormProps {
  initialValues?: Asset
  isSubmitting?: boolean
  submitLabel: string
  onCancel: () => void
  onSubmit: (values: AssetInput) => Promise<void>
}

const categoryOptions = assetCategories.map((category) => ({ label: category, value: category }))
const statusOptions = assetStatuses.map((status) => ({ label: status, value: status }))
const locationOptions = assetLocations.map((location) => ({ label: location, value: location }))

function AssetForm({
  initialValues,
  isSubmitting = false,
  submitLabel,
  onCancel,
  onSubmit,
}: AssetFormProps) {
  const [form] = Form.useForm<AssetFormValues>()

  useEffect(() => {
    form.resetFields()

    if (initialValues) {
      form.setFieldsValue({
        ...initialValues,
        purchaseDate: initialValues.purchaseDate ? dayjs(initialValues.purchaseDate) : undefined,
        warrantyEndDate: initialValues.warrantyEndDate
          ? dayjs(initialValues.warrantyEndDate)
          : undefined,
      })
    }
  }, [form, initialValues])

  const validateWarrantyEndDate = (_rule: unknown, warrantyEndDate?: Dayjs) => {
    const purchaseDate = form.getFieldValue('purchaseDate') as Dayjs | undefined

    if (purchaseDate && warrantyEndDate?.isBefore(purchaseDate, 'day')) {
      return Promise.reject(new Error('Garanti bitiş tarihi satın alma tarihinden önce olamaz.'))
    }

    return Promise.resolve()
  }

  const handleFinish = async (values: AssetFormValues) => {
    await onSubmit({
      assetCode: values.assetCode.trim(),
      category: values.category,
      brand: values.brand.trim(),
      model: values.model.trim(),
      serialNumber: values.serialNumber.trim(),
      status: values.status,
      location: values.location,
      purchaseDate: values.purchaseDate?.format('YYYY-MM-DD') ?? '',
      warrantyEndDate: values.warrantyEndDate?.format('YYYY-MM-DD') ?? '',
    })
  }

  return (
    <Form<AssetFormValues>
      className="asset-form"
      form={form}
      layout="vertical"
      onFinish={(values) => void handleFinish(values)}
      requiredMark="optional"
    >
      <Row gutter={[16, 0]}>
        <Col xs={24} md={12}>
          <Form.Item
            label="Varlık Kodu"
            name="assetCode"
            rules={[{ required: true, whitespace: true, message: 'Varlık kodunu girin.' }]}
          >
            <Input placeholder="Örn. DNT-2026-0155" />
          </Form.Item>
        </Col>

        <Col xs={24} md={12}>
          <Form.Item
            label="Kategori"
            name="category"
            rules={[{ required: true, message: 'Kategori seçin.' }]}
          >
            <Select<AssetCategory> options={categoryOptions} placeholder="Kategori seçin" />
          </Form.Item>
        </Col>

        <Col xs={24} md={12}>
          <Form.Item
            label="Marka"
            name="brand"
            rules={[{ required: true, whitespace: true, message: 'Marka bilgisini girin.' }]}
          >
            <Input placeholder="Örn. Lenovo" />
          </Form.Item>
        </Col>

        <Col xs={24} md={12}>
          <Form.Item
            label="Model"
            name="model"
            rules={[{ required: true, whitespace: true, message: 'Model bilgisini girin.' }]}
          >
            <Input placeholder="Örn. ThinkPad T14 Gen 5" />
          </Form.Item>
        </Col>

        <Col xs={24} md={12}>
          <Form.Item
            label="Seri Numarası"
            name="serialNumber"
            rules={[{ required: true, whitespace: true, message: 'Seri numarasını girin.' }]}
          >
            <Input placeholder="Cihaz seri numarası" />
          </Form.Item>
        </Col>

        <Col xs={24} md={12}>
          <Form.Item
            label="Durum"
            name="status"
            rules={[{ required: true, message: 'Cihaz durumunu seçin.' }]}
          >
            <Select<AssetStatus> options={statusOptions} placeholder="Durum seçin" />
          </Form.Item>
        </Col>

        <Col xs={24} md={12}>
          <Form.Item
            label="Lokasyon"
            name="location"
            rules={[{ required: true, message: 'Lokasyon seçin.' }]}
          >
            <Select<AssetLocation> options={locationOptions} placeholder="Lokasyon seçin" />
          </Form.Item>
        </Col>

        <Col xs={24} md={12}>
          <Form.Item
            label="Satın Alma Tarihi"
            name="purchaseDate"
            rules={[{ required: true, message: 'Satın alma tarihini seçin.' }]}
          >
            <DatePicker format="DD.MM.YYYY" placeholder="Satın alma tarihini seçin" />
          </Form.Item>
        </Col>

        <Col xs={24} md={12}>
          <Form.Item
            dependencies={['purchaseDate']}
            label="Garanti Bitiş Tarihi"
            name="warrantyEndDate"
            rules={[
              { required: true, message: 'Garanti bitiş tarihini seçin.' },
              { validator: validateWarrantyEndDate },
            ]}
          >
            <DatePicker format="DD.MM.YYYY" placeholder="Garanti bitiş tarihini seçin" />
          </Form.Item>
        </Col>
      </Row>

      <div className="asset-form-actions">
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

export default AssetForm
