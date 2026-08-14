import { PageHeader } from '../components'

interface PlaceholderPageProps {
  title: string
}

function PlaceholderPage({ title }: PlaceholderPageProps) {
  return (
    <section>
      <PageHeader
        title={title}
        description="Bu sayfanın içeriği sonraki görevlerde hazırlanacaktır."
      />
    </section>
  )
}

export default PlaceholderPage
