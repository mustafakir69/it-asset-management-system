import { EditOutlined, EyeOutlined, MoreOutlined, PlusOutlined, PoweroffOutlined } from '@ant-design/icons'
import { App as AntdApp, Button, Dropdown, Space, Table, Tooltip, Typography } from 'antd'
import type { MenuProps, TableColumnsType, TablePaginationConfig } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ContentCard, EmptyState, ErrorState, LoadingState, PageHeader, StatusTag } from '../../components'
import { maintenanceService } from '../../services/maintenanceService'
import type { MaintenancePlan } from '../../types/maintenance'
import { formatDate } from '../../utils'
import './MaintenancePage.css'

function MaintenancePlansPage() {
  const navigate = useNavigate(); const { message, modal } = AntdApp.useApp()
  const [plans, setPlans] = useState<MaintenancePlan[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState<string | null>(null); const [pagination, setPagination] = useState({ current: 1, pageSize: 10 })
  const load = useCallback(async () => { setLoading(true); setError(null); try { setPlans(await maintenanceService.getPlans()) } catch (reason: unknown) { setError(reason instanceof Error ? reason.message : 'Bakım planları yüklenemedi.') } finally { setLoading(false) } }, [])
  useEffect(() => { void load() }, [load])
  const changeStatus = (plan: MaintenancePlan) => modal.confirm({ title: plan.isActive ? 'Bakım planını pasife al' : 'Bakım planını aktifleştir', content: `“${plan.name}” planının durumunu değiştirmek istediğinize emin misiniz?`, okText: plan.isActive ? 'Pasife Al' : 'Aktifleştir', cancelText: 'Vazgeç', onOk: async () => { try { await maintenanceService.updatePlanStatus(plan.id, !plan.isActive); message.success('Bakım planı durumu güncellendi.'); await load() } catch (reason: unknown) { message.error(reason instanceof Error ? reason.message : 'Durum güncellenemedi.') } } })
  const columns: TableColumnsType<MaintenancePlan> = [
    { title: 'Cihaz', key: 'asset', width: 200, sorter: (a, b) => a.assetCode.localeCompare(b.assetCode, 'tr-TR'), render: (_value, plan) => <Space direction="vertical" size={0}><Typography.Text strong>{plan.assetCode}</Typography.Text><Typography.Text type="secondary">{plan.assetName}</Typography.Text></Space> },
    { title: 'Plan Adı', dataIndex: 'name', key: 'name', width: 175, ellipsis: true, sorter: (a, b) => a.name.localeCompare(b.name, 'tr-TR') },
    { title: 'Sorumlu IT Personeli', dataIndex: 'responsibleUserName', key: 'responsibleUserName', width: 150, ellipsis: true },
    { title: 'Bakım Sıklığı', dataIndex: 'frequencyDays', key: 'frequencyDays', align: 'center', width: 115, sorter: (a, b) => a.frequencyDays - b.frequencyDays, render: (value: number) => `${value} gün` },
    { title: 'Başlangıç Tarihi', dataIndex: 'startDate', key: 'startDate', align: 'center', width: 125, sorter: (a, b) => a.startDate.localeCompare(b.startDate), render: (value: string) => formatDate(value) },
    { title: 'Aktif/Pasif', dataIndex: 'isActive', key: 'isActive', align: 'center', width: 100, render: (value: boolean) => <StatusTag status={value ? 'Aktif' : 'Pasif'} /> },
    { title: 'İşlemler', key: 'actions', align: 'center', width: 72, render: (_value, plan) => { const items: MenuProps['items'] = [{ key: 'view', icon: <EyeOutlined />, label: 'Görüntüle', onClick: () => void navigate(`/maintenance/plans/${plan.id}`) }, { key: 'edit', icon: <EditOutlined />, label: 'Düzenle', onClick: () => void navigate(`/maintenance/plans/${plan.id}/edit`) }, { key: 'status', icon: <PoweroffOutlined />, label: plan.isActive ? 'Pasife Al' : 'Aktifleştir', onClick: () => changeStatus(plan) }]; return <Dropdown menu={{ items }} placement="bottomRight" trigger={['click']}><Tooltip title="İşlemleri aç"><Button aria-label="Bakım planı işlemlerini aç" icon={<MoreOutlined />} size="small" /></Tooltip></Dropdown> } },
  ]
  const page = (next: TablePaginationConfig) => setPagination({ current: next.current ?? 1, pageSize: next.pageSize ?? 10 })
  return <section className="maintenance-page"><PageHeader title="Bakım Planları" description="Periyodik bakım planlarını ve sorumlu IT personelini yönetin." actions={<Button icon={<PlusOutlined />} onClick={() => void navigate('/maintenance/plans/new')} type="primary">Yeni Bakım Planı</Button>} />{loading ? <ContentCard><LoadingState /></ContentCard> : error ? <ContentCard><ErrorState message={error} onRetry={() => void load()} /></ContentCard> : <ContentCard><Table<MaintenancePlan> className="app-data-table" columns={columns} dataSource={plans} locale={{ emptyText: <EmptyState /> }} onChange={page} pagination={{ ...pagination, pageSizeOptions: ['10', '20', '50'], showSizeChanger: true }} rowKey="id" scroll={{ x: 937 }} size="small" tableLayout="fixed" /></ContentCard>}</section>
}
export default MaintenancePlansPage
