import type { FeedbackStats } from '../types/feedback'

type DashboardProps = {
  stats: FeedbackStats | null
  loading: boolean
  error: string | null
}

function formatAverage(value: number): string {
  if (value === 0) {
    return '—'
  }
  return value.toFixed(1)
}

export function Dashboard({ stats, loading, error }: DashboardProps) {
  if (loading) {
    return (
      <section className="dashboard" aria-busy="true">
        <p className="status status--loading">Loading dashboard…</p>
      </section>
    )
  }

  if (error) {
    return (
      <section className="dashboard">
        <p className="status status--error" role="alert">
          {error}
        </p>
      </section>
    )
  }

  if (!stats || stats.total === 0) {
    return (
      <section className="dashboard">
        <div className="stat-grid">
          <article className="stat-card">
            <h2>Total feedback</h2>
            <p className="stat-value">0</p>
          </article>
          <article className="stat-card">
            <h2>Average rating</h2>
            <p className="stat-value">—</p>
          </article>
        </div>
        <p className="status status--empty">No feedback yet. Add the first entry below.</p>
      </section>
    )
  }

  const maxCount = Math.max(...Object.values(stats.distribution), 1)

  return (
    <section className="dashboard">
      <div className="stat-grid">
        <article className="stat-card">
          <h2>Total feedback</h2>
          <p className="stat-value">{stats.total}</p>
        </article>
        <article className="stat-card">
          <h2>Average rating</h2>
          <p className="stat-value">
            {formatAverage(stats.averageRating)}
            <span className="stat-suffix"> / 5</span>
          </p>
        </article>
      </div>
      <article className="distribution-card">
        <h2>Rating distribution</h2>
        <ul className="distribution-list">
          {([5, 4, 3, 2, 1] as const).map((rating) => {
            const count = stats.distribution[rating]
            const width = (count / maxCount) * 100
            return (
              <li key={rating} className="distribution-row">
                <span className="distribution-label">{rating} ★</span>
                <div className="distribution-bar-track">
                  <div
                    className="distribution-bar-fill"
                    style={{ width: `${width}%` }}
                  />
                </div>
                <span className="distribution-count">{count}</span>
              </li>
            )
          })}
        </ul>
      </article>
    </section>
  )
}