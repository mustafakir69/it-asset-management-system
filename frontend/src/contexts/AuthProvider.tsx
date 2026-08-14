import { useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { authService } from '../services/authService'
import { authUnauthorizedEvent } from '../services/authStorage'
import type { AuthUser } from '../types/auth'
import { AuthContext } from './authContext'
import type { AuthContextValue } from './authContext'

interface AuthProviderProps {
  children: ReactNode
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<AuthUser | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const handleUnauthorized = () => setUser(null)
    window.addEventListener(authUnauthorizedEvent, handleUnauthorized)

    const verifySession = async () => {
      if (!authService.getToken()) {
        setLoading(false)
        return
      }

      try {
        setUser(await authService.getMe())
      } catch {
        authService.logout()
        setUser(null)
      } finally {
        setLoading(false)
      }
    }

    void verifySession()
    return () => window.removeEventListener(authUnauthorizedEvent, handleUnauthorized)
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      role: user?.role ?? null,
      isAuthenticated: user !== null,
      loading,
      login: async (credentials) => {
        const response = await authService.login(credentials)
        setUser(response.user)
      },
      logout: () => {
        authService.logout()
        setUser(null)
      },
    }),
    [loading, user],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
