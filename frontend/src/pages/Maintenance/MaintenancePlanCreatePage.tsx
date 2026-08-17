import { App as AntdApp } from 'antd'
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ContentCard, ErrorState, LoadingState, PageHeader } from '../../components'
import { assetService } from '../../services/assetService'
import { maintenanceService } from '../../services/maintenanceService'
import { userService } from '../../services/userService'
import type { Asset } from '../../types/asset'
import type { MaintenancePlanInput } from '../../types/maintenance'
import type { ItStaffMember } from '../../types/user'
import MaintenancePlanForm from './MaintenancePlanForm'

function MaintenancePlanCreatePage() {
  const navigate = useNavigate(); const { message } = AntdApp.useApp()
  const [assets, setAssets] = useState<Asset[]>([]); const [itStaff, setItStaff] = useState<ItStaffMember[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState<string | null>(null); const [submitting, setSubmitting] = useState(false)
  useEffect(() => { Promise.all([assetService.getAssets(), userService.getItStaff()]).then(([assetData, staffData]) => { setAssets(assetData); setItStaff(staffData) }).catch((reason: unknown) => setError(reason instanceof Error ? reason.message : 'Form verileri yüklenemedi.')).finally(() => setLoading(false)) }, [])
  const submit = async (input: MaintenancePlanInput) => { setSubmitting(true); try { await maintenanceService.createPlan(input); message.success('Bakım planı ve ilk görevi oluşturuldu.'); void navigate('/maintenance') } catch (reason: unknown) { message.error(reason instanceof Error ? reason.message : 'Bakım planı kaydedilemedi.') } finally { setSubmitting(false) } }
  return <section><PageHeader title="Yeni Bakım Planı" description="Periyodik bakım planını ve sorumlu IT personelini tanımlayın." /><ContentCard>{loading ? <LoadingState message="Form verileri yükleniyor..." /> : error ? <ErrorState message={error} /> : <MaintenancePlanForm assets={assets} itStaff={itStaff} onCancel={() => void navigate('/maintenance')} onSubmit={submit} submitting={submitting} />}</ContentCard></section>
}
export default MaintenancePlanCreatePage
