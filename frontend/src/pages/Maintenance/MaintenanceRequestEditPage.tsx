import { App as AntdApp } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ContentCard, ErrorState, LoadingState, PageHeader } from '../../components'
import { assetService } from '../../services/assetService'
import { maintenanceService } from '../../services/maintenanceService'
import type { Asset } from '../../types/asset'
import type { MaintenanceRequest, MaintenanceRequestInput } from '../../types/maintenance'
import MaintenanceRequestForm from './MaintenanceRequestForm'

function MaintenanceRequestEditPage() {
  const { id } = useParams<{ id: string }>(); const navigate = useNavigate(); const { message } = AntdApp.useApp(); const [request, setRequest] = useState<MaintenanceRequest | null>(null); const [assets, setAssets] = useState<Asset[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState<string | null>(null); const [submitting, setSubmitting] = useState(false)
  const load = useCallback(async () => { if (!id) return; setLoading(true); try { const [found, assetData] = await Promise.all([maintenanceService.getRequestById(id), assetService.getAssets()]); if (!found) throw new Error('Bakım talebi bulunamadı.'); if (found.status === 'Tamamlandı' || found.status === 'İptal Edildi') throw new Error('Tamamlanmış veya iptal edilmiş talep düzenlenemez.'); setRequest(found); setAssets(assetData) } catch (reason: unknown) { setError(reason instanceof Error ? reason.message : 'Bakım talebi yüklenemedi.') } finally { setLoading(false) } }, [id])
  useEffect(() => { void load() }, [load])
  const submit = async (input: MaintenanceRequestInput) => { if (!id) return; setSubmitting(true); try { await maintenanceService.updateRequest(id, input); message.success('Bakım talebi güncellendi.'); void navigate(`/maintenance/requests/${id}`) } catch (reason: unknown) { message.error(reason instanceof Error ? reason.message : 'Bakım talebi güncellenemedi.') } finally { setSubmitting(false) } }
  return <section><PageHeader title="Bakım Talebini Düzenle" description="Açık bakım talebinin temel bilgilerini güncelleyin." /><ContentCard>{loading ? <LoadingState /> : error ? <ErrorState message={error} onRetry={() => void load()} /> : request ? <MaintenanceRequestForm assets={assets} initialRequest={request} onCancel={() => void navigate(`/maintenance/requests/${id}`)} onSubmit={submit} submitting={submitting} /> : null}</ContentCard></section>
}
export default MaintenanceRequestEditPage
