import { Statistic } from 'antd'
import type { ReactNode } from 'react'
import ContentCard from '../ContentCard/ContentCard'
import './ActionStatisticCard.css'

export interface ActionStatisticCardProps {
  active?: boolean
  ariaLabel?: string
  color?: string
  icon?: ReactNode
  onClick?: () => void
  title: ReactNode
  value: number
}

function ActionStatisticCard({
  active,
  ariaLabel,
  color,
  icon,
  onClick,
  title,
  value,
}: ActionStatisticCardProps) {
  const statistic = (
    <Statistic
      prefix={icon ? <span style={{ color }}>{icon}</span> : undefined}
      title={title}
      value={value}
      valueStyle={color ? { color } : undefined}
    />
  )

  return (
    <ContentCard className={`action-statistic-card${active ? ' action-statistic-card--active' : ''}`}>
      {onClick ? (
        <button
          aria-label={ariaLabel}
          aria-pressed={active === undefined ? undefined : active}
          className="action-statistic-card__button"
          onClick={onClick}
          type="button"
        >
          {statistic}
        </button>
      ) : (
        statistic
      )}
    </ContentCard>
  )
}

export default ActionStatisticCard
