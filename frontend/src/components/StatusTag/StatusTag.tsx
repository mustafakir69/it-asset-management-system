import { Tag } from 'antd'

export type StatusType =
  | 'Boşta'
  | 'Zimmetli'
  | 'Bakımda'
  | 'Kayıp'
  | 'Hurda'
  | 'Elden Çıkarıldı'
  | 'Aktif'
  | 'İade Edildi'
  | 'Pasif'
  | 'Yaklaşıyor'
  | 'Gecikmiş'
  | 'Tamamlandı'
  | 'Kritik'
  | 'Planlandı'
  | 'Gecikti'
  | 'İptal Edildi'
  | 'Açık'
  | 'Atandı'
  | 'İşlemde'
  | 'Düşük'
  | 'Normal'
  | 'Yüksek'

export interface StatusTagProps {
  status: StatusType
}

const statusColors: Record<StatusType, string> = {
  Boşta: 'green',
  Zimmetli: 'blue',
  Bakımda: 'orange',
  Kayıp: 'red',
  Hurda: 'default',
  'Elden Çıkarıldı': 'purple',
  Aktif: 'green',
  'İade Edildi': 'cyan',
  Pasif: 'default',
  Yaklaşıyor: 'gold',
  Gecikmiş: 'red',
  Tamamlandı: 'cyan',
  Kritik: 'red',
  Planlandı: 'blue',
  Gecikti: 'red',
  'İptal Edildi': 'default',
  Açık: 'blue',
  Atandı: 'cyan',
  İşlemde: 'orange',
  Düşük: 'default',
  Normal: 'blue',
  Yüksek: 'orange',
}

function StatusTag({ status }: StatusTagProps) {
  return <Tag color={statusColors[status]}>{status}</Tag>
}

export default StatusTag
