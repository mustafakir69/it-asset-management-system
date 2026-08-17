import { Button, Col, DatePicker, Form, Input, InputNumber, Row, Select, Space } from 'antd'
import dayjs from 'dayjs'
import type { Dayjs } from 'dayjs'
import type { Asset } from '../../types/asset'
import type { MaintenancePlan, MaintenancePlanInput } from '../../types/maintenance'
import type { ItStaffMember } from '../../types/user'

interface Values {
  assetId: string; name: string; description?: string; responsibleUserId: string
  frequencyDays: number; startDate: Dayjs; estimatedDurationMinutes: number; reminderLeadDays: number
}
interface Props {
  assets: Asset[]; itStaff?: ItStaffMember[]; initialPlan?: MaintenancePlan; submitting: boolean
  onSubmit: (input: MaintenancePlanInput) => Promise<void>; onCancel: () => void
}

function MaintenancePlanForm({ assets, itStaff = [], initialPlan, submitting, onSubmit, onCancel }: Props) {
  const assetOptions = assets.map((asset) => ({ value: asset.id, label: `${asset.assetCode} · ${asset.brand} ${asset.model}` }))
  const staffOptions = itStaff.map((staff) => ({ value: staff.userId, label: `${staff.fullName} · ${staff.email}` }))
  const initialValues: Partial<Values> = initialPlan ? {
    assetId: initialPlan.assetId, name: initialPlan.name, description: initialPlan.description ?? undefined,
    responsibleUserId: initialPlan.responsibleUserId, frequencyDays: initialPlan.frequencyDays,
    startDate: dayjs(initialPlan.startDate), estimatedDurationMinutes: initialPlan.estimatedDurationMinutes,
    reminderLeadDays: initialPlan.reminderLeadDays,
  } : { frequencyDays: 180, estimatedDurationMinutes: 60, reminderLeadDays: 7 }
  return <Form<Values> initialValues={initialValues} layout="vertical" onFinish={(values) => void onSubmit({
    assetId: values.assetId, name: values.name.trim(), description: values.description?.trim() || undefined,
    responsibleUserId: values.responsibleUserId, frequencyDays: values.frequencyDays,
    startDate: values.startDate.format('YYYY-MM-DD'), estimatedDurationMinutes: values.estimatedDurationMinutes,
    reminderLeadDays: values.reminderLeadDays,
  })} requiredMark="optional">
    <Row gutter={[16, 0]}>
      <Col xs={24} md={12}><Form.Item label="Cihaz" name="assetId" rules={[{ required: true, message: 'Lütfen bir cihaz seçin.' }]}><Select showSearch optionFilterProp="label" options={assetOptions} /></Form.Item></Col>
      <Col xs={24} md={12}><Form.Item label="Bakım Adı" name="name" rules={[{ required: true, whitespace: true, message: 'Bakım adı zorunludur.' }, { max: 150 }]}><Input /></Form.Item></Col>
      <Col xs={24} md={12}><Form.Item label="Sorumlu IT Personeli" name="responsibleUserId" rules={[{ required: true, message: 'Sorumlu IT personeli zorunludur.' }]}><Select showSearch optionFilterProp="label" options={staffOptions} /></Form.Item></Col>
      <Col xs={24} md={12}><Form.Item label="Başlangıç Tarihi" name="startDate" rules={[{ required: true, message: 'Başlangıç tarihi zorunludur.' }]}><DatePicker format="DD.MM.YYYY" style={{ width: '100%' }} /></Form.Item></Col>
      <Col xs={24} md={8}><Form.Item label="Tekrar Sıklığı (Gün)" name="frequencyDays" rules={[{ required: true }, { type: 'number', min: 1 }]}><InputNumber min={1} precision={0} style={{ width: '100%' }} /></Form.Item></Col>
      <Col xs={24} md={8}><Form.Item label="Tahmini Süre (Dakika)" name="estimatedDurationMinutes" rules={[{ required: true }, { type: 'number', min: 1, max: 1440 }]}><InputNumber min={1} max={1440} precision={0} style={{ width: '100%' }} /></Form.Item></Col>
      <Col xs={24} md={8}><Form.Item label="Hatırlatma Günü" name="reminderLeadDays" rules={[{ required: true }, { type: 'number', min: 0, max: 365 }]}><InputNumber min={0} max={365} precision={0} style={{ width: '100%' }} /></Form.Item></Col>
      <Col span={24}><Form.Item label="Açıklama" name="description" rules={[{ max: 1000 }]}><Input.TextArea rows={3} /></Form.Item></Col>
    </Row>
    <Space><Button htmlType="submit" loading={submitting} type="primary">{initialPlan ? 'Değişiklikleri Kaydet' : 'Kaydet'}</Button><Button disabled={submitting} onClick={onCancel}>İptal</Button></Space>
  </Form>
}
export default MaintenancePlanForm
