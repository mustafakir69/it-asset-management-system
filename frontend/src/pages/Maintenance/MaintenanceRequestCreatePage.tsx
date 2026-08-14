import { App as AntdApp } from 'antd'
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ContentCard, ErrorState, LoadingState, PageHeader } from '../../components'
import { assetService } from '../../services/assetService'
import { maintenanceService } from '../../services/maintenanceService'
import type { Asset } from '../../types/asset'
import type { MaintenanceRequestInput } from '../../types/maintenance'
import MaintenanceRequestForm from './MaintenanceRequestForm'

function MaintenanceRequestCreatePage() {
  const navigate = useNavigate(); const { message } = AntdApp.useApp(); const [assets, setAssets] = useState<Asset[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState<string | null>(null); const [submitting, setSubmitting] = useState(false)
  useEffect(() => { assetService.getAssets().then(setAssets).catch((reason: unknown) => setError(reason instanceof Error ? reason.message : 'Cihazlar yüklenemedi.')).finally(() => setLoading(false)) }, [])
  const submit = async (input: MaintenanceRequestInput) => { setSubmitting(true); try { await maintenanceService.createRequest(input); message.success('Bakım talebi oluşturuldu.'); void navigate('/maintenance/requests') } catch (reason: unknown) { message.error(reason instanceof Error ? reason.message : 'Bakım talebi kaydedilemedi.') } finally { setSubmitting(false) } }
  return <section><PageHeader title="Yeni Bakım Talebi" description="Manuel bakım, arıza veya destek talebi oluşturun." /><ContentCard>{loading ? <LoadingState /> : error ? <ErrorState message={error} /> : <MaintenanceRequestForm assets={assets} onCancel={() => void navigate('/maintenance/requests')} onSubmit={submit} submitting={submitting} />}</ContentCard></section>
}
export default MaintenanceRequestCreatePage
