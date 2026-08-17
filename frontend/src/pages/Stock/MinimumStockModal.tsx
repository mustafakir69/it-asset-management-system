import { App as AntdApp, InputNumber, Modal, Space, Typography } from 'antd'
import { useEffect, useState } from 'react'
import { stockService } from '../../services/stockService'
import type { StockItem } from '../../types/stockItem'

interface MinimumStockModalProps {
  stockItem: StockItem | null
  open: boolean
  onCancel: () => void
  onSuccess: (stockItem: StockItem) => void | Promise<void>
}

function MinimumStockModal({ stockItem, open, onCancel, onSuccess }: MinimumStockModalProps) {
  const { message } = AntdApp.useApp()
  const [minimumQuantity, setMinimumQuantity] = useState<number | null>(null)
  const [isSaving, setIsSaving] = useState(false)

  useEffect(() => {
    if (open && stockItem) setMinimumQuantity(stockItem.minimumQuantity)
  }, [open, stockItem])

  const save = async () => {
    if (!stockItem || minimumQuantity === null || !Number.isInteger(minimumQuantity) || minimumQuantity < 0) {
      void message.error('Minimum stok miktarı sıfır veya pozitif bir tam sayı olmalıdır.')
      return
    }

    setIsSaving(true)
    try {
      const updated = await stockService.updateMinimumQuantity(stockItem.id, minimumQuantity)
      await onSuccess(updated)
      void message.success('Minimum stok miktarı güncellendi.')
    } catch (error: unknown) {
      void message.error(error instanceof Error ? error.message : 'Minimum stok miktarı güncellenemedi.')
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <Modal
      cancelText="Vazgeç"
      confirmLoading={isSaving}
      okButtonProps={{ disabled: minimumQuantity === null }}
      okText="Kaydet"
      onCancel={onCancel}
      onOk={() => void save()}
      open={open}
      title="Minimum Stok Düzenle"
    >
      <Space direction="vertical" size="small" style={{ width: '100%' }}>
        <Typography.Text strong>{stockItem?.itemCode} — {stockItem?.name}</Typography.Text>
        <Typography.Text type="secondary">
          Mevcut stok: {stockItem?.currentQuantity ?? 0} {stockItem?.unit ?? ''}
        </Typography.Text>
        <InputNumber
          aria-label="Minimum stok miktarı"
          min={0}
          onChange={setMinimumQuantity}
          precision={0}
          style={{ width: '100%' }}
          value={minimumQuantity}
        />
      </Space>
    </Modal>
  )
}

export default MinimumStockModal
