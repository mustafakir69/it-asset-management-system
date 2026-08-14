import { App as AntdApp, Button, Col, Form, Input, InputNumber, Row, Space } from 'antd'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ContentCard, PageHeader } from '../../components'
import { stockService } from '../../services/stockService'
import type { StockItemInput } from '../../types/stockItem'
import './StockItemCreatePage.css'

function StockItemCreatePage() {
  const navigate = useNavigate()
  const { message } = AntdApp.useApp()
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleSubmit = async (values: StockItemInput) => {
    setIsSubmitting(true)

    try {
      await stockService.createStockItem({
        ...values,
        itemCode: values.itemCode.trim(),
        name: values.name.trim(),
        category: values.category.trim(),
        brandModel: values.brandModel.trim(),
        unit: values.unit.trim(),
        location: values.location.trim(),
      })
      message.success('Stok ürünü başarıyla kaydedildi.')
      void navigate('/stock')
    } catch (error: unknown) {
      message.error(error instanceof Error ? error.message : 'Stok ürünü kaydedilemedi.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section className="stock-item-create-page">
      <PageHeader
        title="Yeni Stok Ürünü"
        description="Takip edilecek stok ürününün başlangıç bilgilerini girin."
      />

      <ContentCard>
        <Form<StockItemInput>
          initialValues={{ currentQuantity: 0, minimumQuantity: 0 }}
          layout="vertical"
          onFinish={(values) => void handleSubmit(values)}
          requiredMark="optional"
        >
          <Row gutter={[16, 0]}>
            <Col xs={24} md={12}>
              <Form.Item
                label="Ürün Kodu"
                name="itemCode"
                rules={[{ required: true, whitespace: true, message: 'Ürün kodunu girin.' }]}
              >
                <Input maxLength={50} placeholder="Örn. STK-2026-0019" />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item
                label="Ürün Adı"
                name="name"
                rules={[{ required: true, whitespace: true, message: 'Ürün adını girin.' }]}
              >
                <Input maxLength={150} placeholder="Örn. Kablosuz Mouse" />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item
                label="Kategori"
                name="category"
                rules={[{ required: true, whitespace: true, message: 'Kategori bilgisini girin.' }]}
              >
                <Input maxLength={100} placeholder="Örn. Çevre Birimi" />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item label="Marka / Model" name="brandModel">
                <Input maxLength={150} placeholder="Örn. Logitech M185" />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item
                label="Birim"
                name="unit"
                rules={[{ required: true, whitespace: true, message: 'Birim bilgisini girin.' }]}
              >
                <Input maxLength={30} placeholder="Örn. Adet" />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item
                label="Lokasyon"
                name="location"
                rules={[{ required: true, whitespace: true, message: 'Lokasyon bilgisini girin.' }]}
              >
                <Input maxLength={150} placeholder="Örn. İstanbul Depo" />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item
                label="Başlangıç Stok"
                name="currentQuantity"
                rules={[
                  { required: true, message: 'Başlangıç stok miktarını girin.' },
                  { type: 'number', min: 0, message: 'Başlangıç stok miktarı negatif olamaz.' },
                ]}
              >
                <InputNumber min={0} precision={0} />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item
                label="Minimum Stok"
                name="minimumQuantity"
                rules={[
                  { required: true, message: 'Minimum stok miktarını girin.' },
                  { type: 'number', min: 0, message: 'Minimum stok miktarı negatif olamaz.' },
                ]}
              >
                <InputNumber min={0} precision={0} />
              </Form.Item>
            </Col>
          </Row>

          <div className="stock-item-create-actions">
            <Space wrap>
              <Button disabled={isSubmitting} onClick={() => void navigate('/stock')}>
                İptal
              </Button>
              <Button htmlType="submit" loading={isSubmitting} type="primary">
                Kaydet
              </Button>
            </Space>
          </div>
        </Form>
      </ContentCard>
    </section>
  )
}

export default StockItemCreatePage
