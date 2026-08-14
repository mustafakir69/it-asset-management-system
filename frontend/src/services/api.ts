import axios from 'axios'
import { authUnauthorizedEvent, clearStoredToken, getStoredToken } from './authStorage'

export const apiBaseUrl =
  import.meta.env.VITE_API_BASE_URL?.trim() || 'http://localhost:5080'

export const apiClient = axios.create({
  baseURL: apiBaseUrl,
  headers: {
    Accept: 'application/json',
  },
  timeout: 10_000,
})

apiClient.interceptors.request.use((config) => {
  const token = getStoredToken()
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

apiClient.interceptors.response.use(
  (response) => response,
  (error: unknown) => {
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      const isLoginRequest = error.config?.url?.includes('/api/auth/login') ?? false
      if (!isLoginRequest) {
        clearStoredToken()
        window.dispatchEvent(new Event(authUnauthorizedEvent))
        if (window.location.pathname !== '/login') window.location.assign('/login')
      }
    }

    return Promise.reject(error)
  },
)
