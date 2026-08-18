import { SearchOutlined } from '@ant-design/icons'
import { AutoComplete, Input, Spin, Typography } from 'antd'
import type { AutoCompleteProps } from 'antd'
import type { ReactNode } from 'react'
import { useEffect, useMemo, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { globalSearchService } from '../../services/globalSearchService'
import type { GlobalSearchResult } from '../../types/globalSearch'

function GlobalSearch() {
  const navigate = useNavigate()
  const [query, setQuery] = useState('')
  const [results, setResults] = useState<GlobalSearchResult[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const requestNumber = useRef(0)

  useEffect(() => {
    const term = query.trim()
    const currentRequest = ++requestNumber.current
    if (term.length < 2) {
      setResults([])
      setError(null)
      setLoading(false)
      return
    }

    setLoading(true)
    const timer = window.setTimeout(() => {
      void globalSearchService.search(term)
        .then((items) => {
          if (requestNumber.current !== currentRequest) return
          setResults(items)
          setError(null)
        })
        .catch((reason: unknown) => {
          if (requestNumber.current !== currentRequest) return
          setResults([])
          setError(reason instanceof Error ? reason.message : 'Arama yapılamadı.')
        })
        .finally(() => {
          if (requestNumber.current === currentRequest) setLoading(false)
        })
    }, 300)

    return () => window.clearTimeout(timer)
  }, [query])

  const resultMap = useMemo(() => new Map(
    results.map((result, index) => [`result-${index}`, result]),
  ), [results])

  const options = useMemo<AutoCompleteProps['options']>(() => {
    const categories = new Map<string, Array<{ value: string; label: ReactNode }>>()
    results.forEach((result, index) => {
      const items = categories.get(result.category) ?? []
      items.push({
        value: `result-${index}`,
        label: (
          <div className="global-search-result">
            <Typography.Text strong>{result.title}</Typography.Text>
            <Typography.Text ellipsis type="secondary">{result.description}</Typography.Text>
          </div>
        ),
      })
      categories.set(result.category, items)
    })
    return Array.from(categories, ([label, categoryOptions]) => ({ label, options: categoryOptions }))
  }, [results])

  const notFoundContent = query.trim().length < 2
    ? 'En az 2 karakter girin.'
    : loading
      ? <Spin size="small" />
      : error ?? 'Sonuç bulunamadı.'

  return (
    <AutoComplete
      className="header-global-search"
      notFoundContent={notFoundContent}
      onChange={setQuery}
      onSelect={(value) => {
        const result = resultMap.get(value)
        if (!result) return
        setQuery('')
        setResults([])
        void navigate(result.route)
      }}
      open={query.trim().length >= 2}
      options={options}
      value={query}
    >
      <Input
        allowClear
        aria-label="Sistemde ara"
        placeholder="Sistemde ara..."
        prefix={<SearchOutlined />}
        suffix={loading ? <Spin size="small" /> : undefined}
      />
    </AutoComplete>
  )
}

export default GlobalSearch
