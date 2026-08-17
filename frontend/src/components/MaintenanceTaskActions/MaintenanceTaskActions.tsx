import { CalendarOutlined, CheckOutlined, CloseOutlined, EyeOutlined, MoreOutlined } from '@ant-design/icons'
import { App as AntdApp, Button, DatePicker, Dropdown, Form, Input, Modal, Space, Tooltip } from 'antd'
import type { MenuProps } from 'antd'
import type { Dayjs } from 'dayjs'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { maintenanceService } from '../../services/maintenanceService'
import type { MaintenanceTask } from '../../types/maintenance'

type ActionType = 'complete' | 'cancel' | 'reschedule'
interface ActionValues { date?: Dayjs; result?: string; workNotes?: string; cancellationReason?: string }
export interface MaintenanceTaskActionsProps { task: MaintenanceTask; onSuccess?: () => void; mode?: 'menu' | 'buttons' }

function MaintenanceTaskActions({ task, onSuccess, mode = 'menu' }: MaintenanceTaskActionsProps) {
  const navigate = useNavigate()
  const { message } = AntdApp.useApp()
  const [form] = Form.useForm<ActionValues>()
  const [action, setAction] = useState<ActionType | null>(null)
  const [submitting, setSubmitting] = useState(false)
  const canChange = task.status === 'Planlandı'

  const open = (next: ActionType) => { form.resetFields(); setAction(next) }
  const submit = async () => {
    const values = await form.validateFields()
    setSubmitting(true)
    try {
      if (action === 'complete') await maintenanceService.completeTask(task.id, { completedDate: values.date!.format('YYYY-MM-DD'), result: values.result!.trim(), workNotes: values.workNotes!.trim() })
      if (action === 'cancel') await maintenanceService.cancelTask(task.id, values.cancellationReason!.trim())
      if (action === 'reschedule') await maintenanceService.rescheduleTask(task.id, { plannedDate: values.date!.format('YYYY-MM-DD'), workNotes: values.workNotes!.trim() })
      message.success(action === 'complete' ? 'Bakım görevi tamamlandı ve sonraki görev kontrol edildi.' : action === 'cancel' ? 'Bakım görevi iptal edildi.' : 'Bakım görevi yeniden planlandı.')
      setAction(null)
      onSuccess?.()
    } catch (error: unknown) {
      if (error instanceof Error) message.error(error.message)
    } finally { setSubmitting(false) }
  }

  const items: MenuProps['items'] = [
    { key: 'view', icon: <EyeOutlined />, label: 'Görüntüle', onClick: () => void navigate(`/maintenance/tasks/${task.id}`) },
    ...(canChange ? [
      { key: 'complete', icon: <CheckOutlined />, label: 'Tamamla', onClick: () => open('complete') },
      { key: 'cancel', icon: <CloseOutlined />, danger: true, label: 'İptal Et', onClick: () => open('cancel') },
      { key: 'reschedule', icon: <CalendarOutlined />, label: 'Yeniden Planla', onClick: () => open('reschedule') },
    ] : []),
  ]

  const modalTitle = action === 'complete' ? 'Bakım Görevini Tamamla' : action === 'cancel' ? 'Bakım Görevini İptal Et' : 'Bakım Görevini Yeniden Planla'
  return <>
    {mode === 'menu' ? <Dropdown menu={{ items }} placement="bottomRight" trigger={['click']}><Tooltip title="İşlemleri aç"><Button aria-label="Bakım görevi işlemlerini aç" icon={<MoreOutlined />} size="small" /></Tooltip></Dropdown> : <Space wrap><Button icon={<EyeOutlined />} onClick={() => void navigate(`/maintenance/tasks/${task.id}`)}>Görüntüle</Button>{canChange && <><Button icon={<CheckOutlined />} onClick={() => open('complete')} type="primary">Tamamla</Button><Button danger icon={<CloseOutlined />} onClick={() => open('cancel')}>İptal Et</Button><Button icon={<CalendarOutlined />} onClick={() => open('reschedule')}>Yeniden Planla</Button></>}</Space>}
    <Modal destroyOnHidden confirmLoading={submitting} okText={action === 'cancel' ? 'İptal Et' : 'Kaydet'} onCancel={() => setAction(null)} onOk={() => void submit()} open={action !== null} title={modalTitle}>
      <Form form={form} layout="vertical" preserve={false}>
        {(action === 'complete' || action === 'reschedule') && <Form.Item label={action === 'complete' ? 'Gerçekleşen Tarih' : 'Yeni Planlanan Tarih'} name="date" rules={[{ required: true, message: 'Tarih zorunludur.' }]}><DatePicker format="DD.MM.YYYY" style={{ width: '100%' }} /></Form.Item>}
        {action === 'complete' && <Form.Item label="Sonuç" name="result" rules={[{ required: true, whitespace: true, message: 'Sonuç zorunludur.' }]}><Input.TextArea rows={2} /></Form.Item>}
        {(action === 'complete' || action === 'reschedule') && <Form.Item label="İşlem Notu" name="workNotes" rules={[{ required: true, whitespace: true, message: 'İşlem notu zorunludur.' }]}><Input.TextArea rows={3} /></Form.Item>}
        {action === 'cancel' && <Form.Item label="İptal Nedeni" name="cancellationReason" rules={[{ required: true, whitespace: true, message: 'İptal nedeni zorunludur.' }]}><Input.TextArea rows={3} /></Form.Item>}
      </Form>
    </Modal>
  </>
}

export default MaintenanceTaskActions
