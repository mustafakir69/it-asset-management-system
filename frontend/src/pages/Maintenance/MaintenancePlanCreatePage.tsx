import { App as AntdApp } from 'antd'
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ContentCard, ErrorState, LoadingState, PageHeader } from '../../components'
import { assetService } from '../../services/assetService'
import { maintenanceService } from '../../services/maintenanceService'
import type { Asset } from '../../types/asset'
import type { MaintenancePlanInput } from '../../types/maintenance'
import MaintenancePlanForm from './MaintenancePlanForm'

function MaintenancePlanCreatePage() {
  const navigate = useNavigate(); const { message } = AntdApp.useApp()
  const [assets, setAssets] = useState<Asset[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState<string | null>(null); const [submitting, setSubmitting] = useState(false)
  useEffect(() => { assetService.getAssets().then(setAssets).catch((reason: unknown) => setError(reason instanceof Error ? reason.message : 'Cihazlar yüklenemedi.')).finally(() => setLoading(false)) }, [])
  const submit = async (input: MaintenancePlanInput) => { setSubmitting(true); try { await maintenanceService.createPlan(input); message.success('Bakım planı ve ilk görevi oluşturuldu.'); void navigate('/maintenance/plans') } catch (reason: unknown) { message.error(reason instanceof Error ? reason.message : 'Bakım planı kaydedilemedi.') } finally { setSubmitting(false) } }
  return <section><PageHeader title="Yeni Bakım Planı" description="Periyodik bakım planını ve sorumlu IT personelini tanımlayın." /><ContentCard>{loading ? <LoadingState message="Cihazlar yükleniyor..." /> : error ? <ErrorState message={error} /> : <MaintenancePlanForm assets={assets} onCancel={() => void navigate('/maintenance/plans')} onSubmit={submit} submitting={submitting} />}</ContentCard></section>
}
export default MaintenancePlanCreatePage
