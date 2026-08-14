import { DatePicker, Form, Input, Modal } from 'antd'
import dayjs from 'dayjs'
import type { Dayjs } from 'dayjs'
import { useEffect } from 'react'
import type { Assignment, ReturnAssignmentInput } from '../../types/assignment'

interface ReturnFormValues {
  returnedAt: Dayjs
  returnedBy: string
  returnNotes?: string
}

interface ReturnAssignmentModalProps {
  assignment: Assignment | null
  isSubmitting: boolean
  onCancel: () => void
  onSubmit: (input: ReturnAssignmentInput) => Promise<void>
}

function ReturnAssignmentModal({
  assignment,
  isSubmitting,
  onCancel,
  onSubmit,
}: ReturnAssignmentModalProps) {
  const [form] = Form.useForm<ReturnFormValues>()

  useEffect(() => {
    if (assignment) {
      form.setFieldsValue({
        returnedAt: dayjs(),
        returnedBy: '',
        returnNotes: '',
      })
    } else {
      form.resetFields()
    }
  }, [assignment, form])

  const handleFinish = async (values: ReturnFormValues) => {
    await onSubmit({
      returnedAt: values.returnedAt.toISOString(),
      returnedBy: values.returnedBy.trim(),
      returnNotes: values.returnNotes?.trim() || null,
    })
  }

  return (
    <Modal
      cancelButtonProps={{ disabled: isSubmitting }}
      cancelText="Vazgeç"
      closable={!isSubmitting}
      confirmLoading={isSubmitting}
      maskClosable={!isSubmitting}
      okText="İadeyi Tamamla"
      onCancel={onCancel}
      onOk={() => form.submit()}
      open={assignment !== null}
      title="Cihaz İadesi"
    >
      {assignment && (
        <Form<ReturnFormValues>
          form={form}
          layout="vertical"
          onFinish={(values) => void handleFinish(values)}
          requiredMark="optional"
        >
          <Form.Item label="Cihaz">
            <Input value={`${assignment.assetCode} · ${assignment.assetBrand} ${assignment.assetModel}`} disabled />
          </Form.Item>

          <Form.Item
            label="İade Tarihi"
            name="returnedAt"
            rules={[{ required: true, message: 'İade tarihini seçin.' }]}
          >
            <DatePicker
              disabledDate={(current) =>
                current.isBefore(dayjs(assignment.assignedAt), 'day') ||
                current.isAfter(dayjs(), 'day')
              }
              format="DD.MM.YYYY"
              placeholder="İade tarihini seçin"
              style={{ width: '100%' }}
            />
          </Form.Item>

          <Form.Item
            label="İade Alan"
            name="returnedBy"
            rules={[{ required: true, whitespace: true, message: 'İade alan personeli girin.' }]}
          >
            <Input placeholder="İşlemi yapan personel" />
          </Form.Item>

          <Form.Item label="İade Notu" name="returnNotes">
            <Input.TextArea
              maxLength={500}
              placeholder="İadeyle ilgili isteğe bağlı açıklama"
              rows={3}
              showCount
            />
          </Form.Item>
        </Form>
      )}
    </Modal>
  )
}

export default ReturnAssignmentModal
