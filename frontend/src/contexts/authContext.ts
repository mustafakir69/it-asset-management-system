import { createContext } from 'react'
import type { AuthUser, LoginCredentials } from '../types/auth'

export interface AuthContextValue {
  user: AuthUser | null
  role: AuthUser['role'] | null
  isAuthenticated: boolean
  loading: boolean
  login: (credentials: LoginCredentials) => Promise<void>
  logout: () => void
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined)
