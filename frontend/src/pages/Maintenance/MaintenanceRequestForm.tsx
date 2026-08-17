import { Button, Col, Form, Input, Row, Select, Space } from 'antd'
import { maintenanceRequestPriorities, type MaintenanceRequestInput, type MaintenanceRequestPriority } from '../../types/maintenance'

interface AssetOption { id: string; assetCode: string; assetName: string }
interface Props { assets: AssetOption[]; submitting: boolean; onSubmit: (input: MaintenanceRequestInput) => Promise<void>; onCancel: () => void }

function MaintenanceRequestForm({ assets, submitting, onSubmit, onCancel }: Props) {
  return <Form<MaintenanceRequestInput> layout="vertical" onFinish={(values) => void onSubmit({ ...values, title: values.title.trim(), description: values.description.trim() })} requiredMark="optional">
    <Row gutter={[16, 0]}>
      <Col xs={24} md={12}><Form.Item label="Cihaz" name="assetId" rules={[{ required: true, message: 'Cihaz seçimi zorunludur.' }]}><Select showSearch optionFilterProp="label" options={assets.map((asset) => ({ value: asset.id, label: `${asset.assetCode} · ${asset.assetName}` }))} placeholder="Size zimmetli cihazı seçin" /></Form.Item></Col>
      <Col xs={24} md={12}><Form.Item label="Öncelik" name="priority" rules={[{ required: true, message: 'Öncelik zorunludur.' }]}><Select<MaintenanceRequestPriority> options={maintenanceRequestPriorities.map((value) => ({ label: value, value }))} /></Form.Item></Col>
      <Col span={24}><Form.Item label="Konu" name="title" rules={[{ required: true, whitespace: true, message: 'Konu zorunludur.' }, { max: 150 }]}><Input /></Form.Item></Col>
      <Col span={24}><Form.Item label="Açıklama" name="description" rules={[{ required: true, whitespace: true, message: 'Açıklama zorunludur.' }, { max: 2000 }]}><Input.TextArea rows={5} /></Form.Item></Col>
    </Row>
    <Space><Button htmlType="submit" loading={submitting} type="primary">Talebi Oluştur</Button><Button disabled={submitting} onClick={onCancel}>İptal</Button></Space>
  </Form>
}
export default MaintenanceRequestForm
