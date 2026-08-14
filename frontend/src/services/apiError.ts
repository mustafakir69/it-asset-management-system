import axios from 'axios'

const isRecord = (value: unknown): value is Record<string, unknown> =>
  typeof value === 'object' && value !== null

const getValidationMessage = (value: unknown): string | undefined => {
  if (!isRecord(value)) return undefined

  const messages = Object.values(value).flatMap((entry) =>
    Array.isArray(entry)
      ? entry.filter((message): message is string => typeof message === 'string')
      : [],
  )
  return messages.length > 0 ? messages.join(' ') : undefined
}

export const getApiErrorMessage = (error: unknown, fallbackMessage: string): string => {
  if (!axios.isAxiosError(error)) {
    return error instanceof Error ? error.message : fallbackMessage
  }

  if (!error.response) {
    return 'Backend servisine ulaşılamadı. Servisin çalıştığını kontrol edip tekrar deneyin.'
  }

  const responseData: unknown = error.response.data
  if (isRecord(responseData)) {
    const validationMessage = getValidationMessage(responseData.errors)
    if (validationMessage) return validationMessage
    if (typeof responseData.detail === 'string') return responseData.detail
    if (typeof responseData.message === 'string') return responseData.message
  }

  if (error.response.status === 403) return 'Bu işlem için yetkiniz bulunmuyor.'
  if (error.response.status === 404) return 'İstenen kayıt bulunamadı.'
  if (error.response.status === 409) return 'İşlem mevcut kayıtlarla çakıştığı için tamamlanamadı.'
  return fallbackMessage
}
