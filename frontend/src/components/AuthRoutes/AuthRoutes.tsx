import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../../contexts/useAuth'
import type { UserRole } from '../../types/auth'
import LoadingState from '../LoadingState/LoadingState'

interface RouteGuardProps {
  children: ReactNode
}

export function ProtectedRoute({ children }: RouteGuardProps) {
  const { isAuthenticated, loading } = useAuth()
  const location = useLocation()

  if (loading) return <LoadingState message="Oturum doğrulanıyor..." />
  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }

  return children
}

export function PublicOnlyRoute({ children }: RouteGuardProps) {
  const { isAuthenticated, loading } = useAuth()

  if (loading) return <LoadingState message="Oturum doğrulanıyor..." />
  if (isAuthenticated) return <Navigate to="/dashboard" replace />

  return children
}

interface RoleRouteProps extends RouteGuardProps {
  allowedRoles: readonly UserRole[]
}

export function RoleRoute({ allowedRoles, children }: RoleRouteProps) {
  const { loading, role } = useAuth()

  if (loading) return <LoadingState message="Yetki bilgisi doğrulanıyor..." />
  if (!role || !allowedRoles.includes(role)) return <Navigate to="/unauthorized" replace />

  return children
}
