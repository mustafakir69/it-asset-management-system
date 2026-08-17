import { App as AntdApp, DatePicker, Form, Input, InputNumber, Modal, Select, Typography } from 'antd'
import dayjs from 'dayjs'
import type { Dayjs } from 'dayjs'
import { useEffect, useState } from 'react'
import { useAuth } from '../../contexts/useAuth'
import { assignmentService } from '../../services/assignmentService'
import { stockService } from '../../services/stockService'
import type { Employee } from '../../types/assignment'
import type { StockItem, StockTransactionInput, StockTransactionType } from '../../types/stockItem'

interface StockTransactionFormValues {
  transactionType: StockTransactionType
  quantity: number
  transactionDate: Dayjs
  recipientEmployeeId?: string
  note?: string
}

interface StockTransactionModalProps {
  open: boolean
  stockItem: StockItem | null
  onCancel: () => void
  onSuccess: () => Promise<void>
}

function StockTransactionModal({
  open,
  stockItem,
  onCancel,
  onSuccess,
}: StockTransactionModalProps) {
  const [form] = Form.useForm<StockTransactionFormValues>()
  const { message } = AntdApp.useApp()
  const { user } = useAuth()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [employees, setEmployees] = useState<Employee[]>([])
  const transactionType = Form.useWatch('transactionType', form)

  useEffect(() => {
    if (open) {
      form.setFieldsValue({ transactionType: 'Giriş', transactionDate: dayjs() })
      void assignmentService.getEmployees().then(setEmployees).catch(() => {
        message.error('Teslim alan çalışan listesi yüklenemedi.')
      })
    } else {
      form.resetFields()
    }
  }, [form, message, open])

  const handleSubmit = async (values: StockTransactionFormValues) => {
    if (!stockItem) {
      return
    }

    const input: StockTransactionInput = {
      transactionType: values.transactionType,
      quantity: values.quantity,
      transactionDate: values.transactionDate.startOf('day').toISOString(),
      recipientEmployeeId: values.transactionType === 'Çıkış' ? values.recipientEmployeeId : undefined,
      note: values.note?.trim() || undefined,
    }

    setIsSubmitting(true)

    try {
      await stockService.createStockTransaction(stockItem.id, input)
      message.success('Stok hareketi başarıyla kaydedildi.')
      await onSuccess()
      onCancel()
    } catch (error: unknown) {
      message.error(error instanceof Error ? error.message : 'Stok hareketi kaydedilemedi.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <Modal
      cancelButtonProps={{ disabled: isSubmitting }}
      cancelText="İptal"
      confirmLoading={isSubmitting}
      okText="Hareketi Kaydet"
      onCancel={onCancel}
      onOk={() => void form.submit()}
      open={open}
      title={stockItem ? `${stockItem.itemCode} - Stok Hareketi` : 'Stok Hareketi'}
    >
      <Form<StockTransactionFormValues>
        form={form}
        layout="vertical"
        onFinish={(values) => void handleSubmit(values)}
        preserve={false}
        requiredMark="optional"
      >
        <Form.Item
          label="İşlem Tipi"
          name="transactionType"
          rules={[{ required: true, message: 'İşlem tipini seçin.' }]}
        >
          <Select<StockTransactionType>
            options={[
              { label: 'Giriş', value: 'Giriş' },
              { label: 'Çıkış', value: 'Çıkış' },
            ]}
          />
        </Form.Item>
        <Form.Item
          label="Miktar"
          name="quantity"
          rules={[
            { required: true, message: 'Miktarı girin.' },
            { type: 'number', min: 1, message: 'Miktar sıfırdan büyük olmalıdır.' },
          ]}
        >
          <InputNumber min={1} precision={0} style={{ width: '100%' }} />
        </Form.Item>
        <Form.Item
          label="İşlem Tarihi"
          name="transactionDate"
          rules={[{ required: true, message: 'İşlem tarihini seçin.' }]}
        >
          <DatePicker format="DD.MM.YYYY" style={{ width: '100%' }} />
        </Form.Item>
        <Typography.Paragraph type="secondary">
          İşlemi yapan: <Typography.Text strong>{user?.username ?? 'Oturum kullanıcısı'}</Typography.Text>
        </Typography.Paragraph>
        {transactionType === 'Çıkış' && (
          <Form.Item label="Teslim Alan Çalışan" name="recipientEmployeeId">
            <Select
              allowClear
              optionFilterProp="label"
              options={employees.map((employee) => ({
                value: employee.id,
                label: `${employee.employeeNo} · ${employee.fullName} · ${employee.department}`,
              }))}
              placeholder="Genel tüketim veya çalışan seçin"
              showSearch
            />
          </Form.Item>
        )}
        <Form.Item label="Not" name="note">
          <Input.TextArea maxLength={500} rows={3} showCount />
        </Form.Item>
      </Form>
    </Modal>
  )
}

export default StockTransactionModal
