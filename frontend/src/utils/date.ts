export type DateInput = Date | string | number | null | undefined

const defaultDateOptions: Intl.DateTimeFormatOptions = {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
}

export function formatDate(
  value: DateInput,
  options: Intl.DateTimeFormatOptions = defaultDateOptions,
): string {
  if (value === null || value === undefined || value === '') {
    return '-'
  }

  const date = value instanceof Date ? value : new Date(value)

  if (Number.isNaN(date.getTime())) {
    return '-'
  }

  return new Intl.DateTimeFormat('tr-TR', options).format(date)
}
