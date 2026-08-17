import { App as AntdApp } from 'antd'
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ContentCard, ErrorState, LoadingState, PageHeader } from '../../components'
import { assignmentService } from '../../services/assignmentService'
import { maintenanceService } from '../../services/maintenanceService'
import type { Assignment } from '../../types/assignment'
import type { MaintenanceRequestInput } from '../../types/maintenance'
import MaintenanceRequestForm from './MaintenanceRequestForm'

function MaintenanceRequestCreatePage() {
  const navigate = useNavigate(); const { message } = AntdApp.useApp(); const [assignments, setAssignments] = useState<Assignment[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState<string | null>(null); const [submitting, setSubmitting] = useState(false)
  useEffect(() => { assignmentService.getMyAssignments().then(setAssignments).catch((reason: unknown) => setError(reason instanceof Error ? reason.message : 'Zimmetli cihazlarınız yüklenemedi.')).finally(() => setLoading(false)) }, [])
  const submit = async (input: MaintenanceRequestInput) => { setSubmitting(true); try { await maintenanceService.createRequest(input); message.success('Teknik destek talebi oluşturuldu.'); void navigate('/support-requests') } catch (reason: unknown) { message.error(reason instanceof Error ? reason.message : 'Teknik destek talebi kaydedilemedi.') } finally { setSubmitting(false) } }
  const assets = assignments.map((assignment) => ({ id: assignment.assetId, assetCode: assignment.assetCode, assetName: assignment.assetName }))
  return <section><PageHeader title="Yeni Destek Talebi" description="Size zimmetli bir cihaz için teknik destek talebi oluşturun." /><ContentCard>{loading ? <LoadingState /> : error ? <ErrorState message={error} /> : <MaintenanceRequestForm assets={assets} onCancel={() => void navigate('/support-requests')} onSubmit={submit} submitting={submitting} />}</ContentCard></section>
}
export default MaintenanceRequestCreatePage
