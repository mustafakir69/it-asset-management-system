const tokenStorageKey = 'takip-programi.auth-token'

export const authUnauthorizedEvent = 'takip-programi:unauthorized'

export const getStoredToken = (): string | null => localStorage.getItem(tokenStorageKey)

export const storeToken = (token: string): void => {
  localStorage.setItem(tokenStorageKey, token)
}

export const clearStoredToken = (): void => {
  localStorage.removeItem(tokenStorageKey)
}
