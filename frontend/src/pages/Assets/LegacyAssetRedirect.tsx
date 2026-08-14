import { Navigate, useParams } from 'react-router-dom'

interface LegacyAssetRedirectProps { edit?: boolean }

function LegacyAssetRedirect({ edit = false }: LegacyAssetRedirectProps) {
  const { deviceId } = useParams<{ deviceId: string }>()
  return deviceId ? <Navigate replace to={`/assets/${deviceId}${edit ? '/edit' : ''}`} /> : <Navigate replace to="/assets" />
}

export default LegacyAssetRedirect
