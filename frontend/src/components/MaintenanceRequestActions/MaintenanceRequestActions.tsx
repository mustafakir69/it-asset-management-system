import { CheckOutlined, CloseOutlined, EyeOutlined, MoreOutlined, PlayCircleOutlined, TeamOutlined } from '@ant-design/icons'
import { App as AntdApp, Button, DatePicker, Dropdown, Form, Input, Modal, Select, Space, Tooltip } from 'antd'
import type { MenuProps } from 'antd'
import type { Dayjs } from 'dayjs'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../contexts/useAuth'
import { maintenanceService } from '../../services/maintenanceService'
import { userService } from '../../services/userService'
import type { MaintenanceRequest } from '../../types/maintenance'
import type { ItStaffMember } from '../../types/user'

type Action = 'assign' | 'complete' | 'cancel'
interface Values { assignedToUserId?: string; completedAt?: Dayjs; result?: string; workNotes?: string; cancellationReason?: string }
export interface MaintenanceRequestActionsProps { request: MaintenanceRequest; onSuccess?: () => void; mode?: 'menu' | 'buttons' }

function MaintenanceRequestActions({ request, onSuccess, mode = 'menu' }: MaintenanceRequestActionsProps) {
  const navigate = useNavigate(); const { user } = useAuth(); const { message } = AntdApp.useApp()
  const [form] = Form.useForm<Values>(); const [action, setAction] = useState<Action | null>(null); const [submitting, setSubmitting] = useState(false); const [staff, setStaff] = useState<ItStaffMember[]>([])
  const canManage = user?.role === 'Admin' || user?.role === 'IT'
  const open = async (next: Action) => { form.resetFields(); if (next === 'assign') { try { setStaff(await userService.getItStaff()) } catch (error: unknown) { message.error(error instanceof Error ? error.message : 'IT personeli alınamadı.'); return } } setAction(next) }
  const submit = async () => {
    const values = await form.validateFields(); setSubmitting(true)
    try {
      if (action === 'assign') await maintenanceService.assignRequest(request.id, values.assignedToUserId!)
      if (action === 'complete') await maintenanceService.completeRequest(request.id, { completedAt: values.completedAt!.toISOString(), result: values.result!.trim(), workNotes: values.workNotes!.trim() })
      if (action === 'cancel') await maintenanceService.cancelRequest(request.id, values.cancellationReason!.trim())
      message.success(action === 'assign' ? 'Talep IT personeline atandı.' : action === 'complete' ? 'Talep tamamlandı.' : 'Talep iptal edildi.')
      setAction(null); onSuccess?.()
    } catch (error: unknown) { message.error(error instanceof Error ? error.message : 'İşlem tamamlanamadı.') }
    finally { setSubmitting(false) }
  }
  const start = async () => { try { await maintenanceService.startRequest(request.id); message.success('Talep işleme alındı.'); onSuccess?.() } catch (error: unknown) { message.error(error instanceof Error ? error.message : 'Talep işleme alınamadı.') } }
  const items: MenuProps['items'] = [
    { key: 'view', icon: <EyeOutlined />, label: 'Görüntüle', onClick: () => void navigate(`/support-requests/${request.id}`) },
    ...(canManage && !['Tamamlandı', 'İptal Edildi'].includes(request.status) ? [
      { key: 'assign', icon: <TeamOutlined />, label: 'IT Personeline Ata', onClick: () => void open('assign') },
      ...(request.status === 'Atandı' ? [{ key: 'start', icon: <PlayCircleOutlined />, label: 'İşleme Al', onClick: () => void start() }] : []),
      ...(request.status === 'İşlemde' ? [{ key: 'complete', icon: <CheckOutlined />, label: 'Tamamla', onClick: () => void open('complete') }] : []),
      { key: 'cancel', icon: <CloseOutlined />, danger: true, label: 'İptal Et', onClick: () => void open('cancel') },
    ] : []),
  ]
  return <>
    {mode === 'menu' ? <Dropdown menu={{ items }} trigger={['click']}><Tooltip title="İşlemleri aç"><Button icon={<MoreOutlined />} size="small" /></Tooltip></Dropdown> : <Space><Button icon={<EyeOutlined />} onClick={() => void navigate(`/support-requests/${request.id}`)}>Görüntüle</Button>{canManage && request.status === 'Atandı' && <Button onClick={() => void start()} type="primary">İşleme Al</Button>}</Space>}
    <Modal destroyOnHidden confirmLoading={submitting} onCancel={() => setAction(null)} onOk={() => void submit()} open={action !== null} title={action === 'assign' ? 'IT Personeline Ata' : action === 'complete' ? 'Destek Talebini Tamamla' : 'Destek Talebini İptal Et'}>
      <Form form={form} layout="vertical" preserve={false}>
        {action === 'assign' && <Form.Item label="IT Personeli" name="assignedToUserId" rules={[{ required: true, message: 'IT personeli seçin.' }]}><Select options={staff.map((item) => ({ value: item.userId, label: `${item.fullName} · ${item.email}` }))} /></Form.Item>}
        {action === 'complete' && <><Form.Item label="Tamamlanma Tarihi" name="completedAt" rules={[{ required: true, message: 'Tarih zorunludur.' }]}><DatePicker showTime style={{ width: '100%' }} /></Form.Item><Form.Item label="Çözüm" name="result" rules={[{ required: true, whitespace: true, message: 'Çözüm zorunludur.' }]}><Input.TextArea rows={3} /></Form.Item><Form.Item label="Çalışma Notları" name="workNotes" rules={[{ required: true, whitespace: true, message: 'Çalışma notları zorunludur.' }]}><Input.TextArea rows={3} /></Form.Item></>}
        {action === 'cancel' && <Form.Item label="İptal Nedeni" name="cancellationReason" rules={[{ required: true, whitespace: true, message: 'İptal nedeni zorunludur.' }]}><Input.TextArea rows={3} /></Form.Item>}
      </Form>
    </Modal>
  </>
}
export default MaintenanceRequestActions
