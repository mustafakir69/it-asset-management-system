import { Tag } from 'antd'

export type StatusType =
  | 'Stokta'
  | 'Zimmetli'
  | 'Bakımda'
  | 'Kayıp'
  | 'Hurda'
  | 'Elden çıkarıldı'
  | 'Aktif'
  | 'İade Edildi'
  | 'Pasif'
  | 'Yaklaşıyor'
  | 'Gecikmiş'
  | 'Tamamlandı'

export interface StatusTagProps {
  status: StatusType
}

const statusColors: Record<StatusType, string> = {
  Stokta: 'green',
  Zimmetli: 'blue',
  Bakımda: 'orange',
  Kayıp: 'red',
  Hurda: 'default',
  'Elden çıkarıldı': 'purple',
  Aktif: 'green',
  'İade Edildi': 'cyan',
  Pasif: 'default',
  Yaklaşıyor: 'gold',
  Gecikmiş: 'red',
  Tamamlandı: 'cyan',
}

function StatusTag({ status }: StatusTagProps) {
  return <Tag color={statusColors[status]}>{status}</Tag>
}

export default StatusTag
