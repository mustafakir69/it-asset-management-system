import type {
  CriticalStockItem,
  DashboardSummary,
  DeviceMovement,
  ExpiringWarranty,
  MaintenanceTaskSummary,
} from '../types/dashboard'

export const dashboardSummaries: DashboardSummary[] = [
  { key: 'totalDevices', title: 'Toplam Cihaz', value: 324 },
  { key: 'inStockDevices', title: 'Stokta Cihaz', value: 74 },
  { key: 'assignedDevices', title: 'Zimmetli Cihaz', value: 218 },
  { key: 'maintenanceDevices', title: 'Bakımda Cihaz', value: 12 },
  { key: 'expiringWarranties', title: 'Yaklaşan Garanti', value: 18 },
  { key: 'upcomingLicenseRenewals', title: 'Yaklaşan Lisans Yenileme', value: 7 },
  { key: 'criticalStockItems', title: 'Kritik Stok', value: 6 },
  { key: 'overdueMaintenanceTasks', title: 'Geciken Bakım', value: 3 },
]

export const recentDeviceMovements: DeviceMovement[] = [
  {
    id: 'movement-1',
    assetCode: 'DNT-2026-0148',
    deviceName: 'Dizüstü Bilgisayar',
    description: 'Operasyon ekibine zimmetlendi.',
    occurredAt: '2026-08-13T09:20:00+03:00',
    status: 'Zimmetli',
  },
  {
    id: 'movement-2',
    assetCode: 'MNT-2026-0062',
    deviceName: '27 inç Monitör',
    description: 'İade alınarak stoğa aktarıldı.',
    occurredAt: '2026-08-12T16:45:00+03:00',
    status: 'Stokta',
  },
  {
    id: 'movement-3',
    assetCode: 'TLF-2025-0039',
    deviceName: 'Kurumsal Telefon',
    description: 'Ekran kontrolü için bakıma gönderildi.',
    occurredAt: '2026-08-12T11:10:00+03:00',
    status: 'Bakımda',
  },
  {
    id: 'movement-4',
    assetCode: 'YAZ-2024-0017',
    deviceName: 'Lazer Yazıcı',
    description: 'Periyodik bakım işlemi tamamlandı.',
    occurredAt: '2026-08-11T14:30:00+03:00',
    status: 'Tamamlandı',
  },
]

export const expiringWarranties: ExpiringWarranty[] = [
  {
    id: 'warranty-1',
    assetCode: 'DNT-2024-0084',
    deviceName: 'Dizüstü Bilgisayar',
    expiresAt: '2026-08-21',
    remainingDays: 8,
    status: 'Yaklaşıyor',
  },
  {
    id: 'warranty-2',
    assetCode: 'SWC-2023-0012',
    deviceName: 'Ağ Anahtarı',
    expiresAt: '2026-08-28',
    remainingDays: 15,
    status: 'Yaklaşıyor',
  },
  {
    id: 'warranty-3',
    assetCode: 'SRV-2023-0005',
    deviceName: 'Uygulama Sunucusu',
    expiresAt: '2026-09-05',
    remainingDays: 23,
    status: 'Yaklaşıyor',
  },
]

export const criticalStockItems: CriticalStockItem[] = [
  {
    id: 'stock-1',
    productName: 'USB-C Çoklayıcı',
    currentQuantity: 3,
    minimumQuantity: 10,
    unit: 'adet',
  },
  {
    id: 'stock-2',
    productName: 'Kablosuz Klavye',
    currentQuantity: 4,
    minimumQuantity: 8,
    unit: 'adet',
  },
  {
    id: 'stock-3',
    productName: 'Cat6 Ağ Kablosu (3 m)',
    currentQuantity: 6,
    minimumQuantity: 15,
    unit: 'adet',
  },
  {
    id: 'stock-4',
    productName: 'Siyah Toner',
    currentQuantity: 2,
    minimumQuantity: 6,
    unit: 'adet',
  },
]

export const maintenanceTasks: MaintenanceTaskSummary[] = [
  {
    id: 'maintenance-1',
    assetCode: 'SRV-2022-0003',
    deviceName: 'Dosya Sunucusu',
    taskName: 'Disk ve yedekleme kontrolü',
    dueDate: '2026-08-10',
    status: 'Gecikmiş',
  },
  {
    id: 'maintenance-2',
    assetCode: 'UPS-2023-0011',
    deviceName: 'Kesintisiz Güç Kaynağı',
    taskName: 'Akü kapasite testi',
    dueDate: '2026-08-12',
    status: 'Gecikmiş',
  },
  {
    id: 'maintenance-3',
    assetCode: 'YAZ-2024-0021',
    deviceName: 'Çok Fonksiyonlu Yazıcı',
    taskName: 'Periyodik temizlik ve sayaç kontrolü',
    dueDate: '2026-08-16',
    status: 'Yaklaşıyor',
  },
  {
    id: 'maintenance-4',
    assetCode: 'SWC-2024-0009',
    deviceName: 'Ağ Anahtarı',
    taskName: 'Port ve yapılandırma kontrolü',
    dueDate: '2026-08-19',
    status: 'Yaklaşıyor',
  },
]
