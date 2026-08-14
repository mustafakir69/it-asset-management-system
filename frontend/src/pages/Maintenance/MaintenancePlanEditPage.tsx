import { App as AntdApp } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ContentCard, ErrorState, LoadingState, PageHeader } from '../../components'
import { assetService } from '../../services/assetService'
import { maintenanceService } from '../../services/maintenanceService'
import type { Asset } from '../../types/asset'
import type { MaintenancePlan, MaintenancePlanInput } from '../../types/maintenance'
import MaintenancePlanForm from './MaintenancePlanForm'

function MaintenancePlanEditPage() {
  const { id } = useParams<{ id: string }>(); const navigate = useNavigate(); const { message } = AntdApp.useApp()
  const [plan, setPlan] = useState<MaintenancePlan | null>(null); const [assets, setAssets] = useState<Asset[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState<string | null>(null); const [submitting, setSubmitting] = useState(false)
  const load = useCallback(async () => { if (!id) return; setLoading(true); try { const [found, assetData] = await Promise.all([maintenanceService.getPlanById(id), assetService.getAssets()]); if (!found) throw new Error('Bakım planı bulunamadı.'); setPlan(found); setAssets(assetData) } catch (reason: unknown) { setError(reason instanceof Error ? reason.message : 'Bakım planı yüklenemedi.') } finally { setLoading(false) } }, [id])
  useEffect(() => { void load() }, [load])
  const submit = async (input: MaintenancePlanInput) => { if (!id) return; setSubmitting(true); try { await maintenanceService.updatePlan(id, input); message.success('Bakım planı güncellendi.'); void navigate(`/maintenance/plans/${id}`) } catch (reason: unknown) { message.error(reason instanceof Error ? reason.message : 'Bakım planı güncellenemedi.') } finally { setSubmitting(false) } }
  return <section><PageHeader title="Bakım Planını Düzenle" description="Plan bilgilerini ve sorumlu personeli güncelleyin." /><ContentCard>{loading ? <LoadingState /> : error ? <ErrorState message={error} onRetry={() => void load()} /> : plan ? <MaintenancePlanForm assets={assets} initialPlan={plan} onCancel={() => void navigate(`/maintenance/plans/${id}`)} onSubmit={submit} submitting={submitting} /> : null}</ContentCard></section>
}
export default MaintenancePlanEditPage
