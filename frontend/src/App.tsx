import { useCallback, useEffect, useState } from 'react'
import {
  analyzeFeedback,
  createFeedback,
  deleteFeedback,
  listFeedback,
  loadFeedbackStats,
  updateFeedback,
} from './api/feedbackApi'
import { Dashboard } from './components/Dashboard'
import { FeedbackForm } from './components/FeedbackForm'
import { Modal } from './components/Modal'
import type { Feedback, FeedbackInput, FeedbackStats } from './types/feedback'

const PAGE_SIZE = 10

function formatDate(iso: string): string {
  return new Date(iso).toLocaleString(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  })
}

function stars(rating: number): string {
  return '★'.repeat(rating) + '☆'.repeat(5 - rating)
}

function sentimentClass(sentiment: string): string {
  const value = sentiment.toLowerCase()
  if (value === 'positive') return 'badge badge--positive'
  if (value === 'negative') return 'badge badge--negative'
  return 'badge badge--neutral'
}

type ModalMode =
  | { kind: 'closed' }
  | { kind: 'create' }
  | { kind: 'edit'; feedback: Feedback }

export default function App() {
  const [stats, setStats] = useState<FeedbackStats | null>(null)
  const [statsLoading, setStatsLoading] = useState(true)
  const [statsError, setStatsError] = useState<string | null>(null)

  const [items, setItems] = useState<Feedback[]>([])
  const [listLoading, setListLoading] = useState(true)
  const [listError, setListError] = useState<string | null>(null)

  const [page, setPage] = useState(1)
  const [ratingFilter, setRatingFilter] = useState<number | ''>('')
  const [hasNextPage, setHasNextPage] = useState(false)

  const [modal, setModal] = useState<ModalMode>({ kind: 'closed' })
  const [deletingId, setDeletingId] = useState<string | null>(null)
  const [analyzingId, setAnalyzingId] = useState<string | null>(null)
  const [analyzeErrors, setAnalyzeErrors] = useState<Record<string, string>>({})

  const refreshStats = useCallback(async () => {
    setStatsLoading(true)
    setStatsError(null)
    try {
      const data = await loadFeedbackStats()
      setStats(data)
    } catch (err) {
      setStatsError(err instanceof Error ? err.message : 'Failed to load stats')
    } finally {
      setStatsLoading(false)
    }
  }, [])

  const refreshList = useCallback(async () => {
    setListLoading(true)
    setListError(null)
    try {
      const rating = ratingFilter === '' ? undefined : ratingFilter
      const data = await listFeedback(page, PAGE_SIZE, rating)
      setItems(data)
      setHasNextPage(data.length === PAGE_SIZE)
    } catch (err) {
      setListError(err instanceof Error ? err.message : 'Failed to load feedback')
      setItems([])
      setHasNextPage(false)
    } finally {
      setListLoading(false)
    }
  }, [page, ratingFilter])

  useEffect(() => {
    void refreshStats()
  }, [refreshStats])

  useEffect(() => {
    void refreshList()
  }, [refreshList])

  async function handleSaved() {
    setModal({ kind: 'closed' })
    await Promise.all([refreshStats(), refreshList()])
  }

  async function handleCreate(input: FeedbackInput) {
    await createFeedback(input)
    setPage(1)
    await handleSaved()
  }

  async function handleUpdate(id: string, input: FeedbackInput) {
    await updateFeedback(id, input)
    await handleSaved()
  }

  async function handleAnalyze(feedback: Feedback) {
    setAnalyzingId(feedback.id)
    setAnalyzeErrors((prev) => {
      const next = { ...prev }
      delete next[feedback.id]
      return next
    })
    try {
      const updated = await analyzeFeedback(feedback.id)
      setItems((prev) =>
        prev.map((item) => (item.id === feedback.id ? updated : item)),
      )
    } catch (err) {
      setAnalyzeErrors((prev) => ({
        ...prev,
        [feedback.id]:
          err instanceof Error ? err.message : 'Analysis failed',
      }))
    } finally {
      setAnalyzingId(null)
    }
  }

  async function handleDelete(feedback: Feedback) {
    if (!window.confirm(`Delete feedback from ${feedback.customerName}?`)) {
      return
    }
    setDeletingId(feedback.id)
    try {
      await deleteFeedback(feedback.id)
      await refreshStats()
      if (items.length === 1 && page > 1) {
        setPage((p) => p - 1)
      } else {
        await refreshList()
      }
    } catch (err) {
      window.alert(err instanceof Error ? err.message : 'Delete failed')
    } finally {
      setDeletingId(null)
    }
  }

  return (
    <div className="app">
      <header className="app-header">
        <div>
          <h1>FeedbackAI</h1>
          <p className="app-subtitle">
            Customer feedback dashboard with AI-powered comment analysis
          </p>
        </div>
        <button
          type="button"
          className="btn btn--primary"
          onClick={() => setModal({ kind: 'create' })}
        >
          Add feedback
        </button>
      </header>

      <Dashboard stats={stats} loading={statsLoading} error={statsError} />

      <section className="list-section">
        <div className="list-toolbar">
          <label className="filter-label">
            Filter by rating
            <select
              value={ratingFilter === '' ? '' : String(ratingFilter)}
              onChange={(e) => {
                const v = e.target.value
                setRatingFilter(v === '' ? '' : Number(v))
                setPage(1)
              }}
            >
              <option value="">All ratings</option>
              {[5, 4, 3, 2, 1].map((r) => (
                <option key={r} value={r}>
                  {r} stars
                </option>
              ))}
            </select>
          </label>
        </div>

        {listLoading && (
          <p className="status status--loading">Loading feedback…</p>
        )}

        {!listLoading && listError && (
          <p className="status status--error" role="alert">
            {listError}
          </p>
        )}

        {!listLoading && !listError && items.length === 0 && (
          <p className="status status--empty">
            {ratingFilter === ''
              ? 'No feedback to show.'
              : `No feedback with ${ratingFilter} stars.`}
          </p>
        )}

        {!listLoading && !listError && items.length > 0 && (
          <div className="table-wrap">
            <table className="feedback-table">
              <thead>
                <tr>
                  <th>Customer</th>
                  <th>Rating</th>
                  <th>Comment</th>
                  <th>AI analysis</th>
                  <th>Created</th>
                  <th aria-label="Actions" />
                </tr>
              </thead>
              <tbody>
                {items.map((row) => (
                  <tr key={row.id}>
                    <td data-label="Customer">
                      <span className="customer-name">{row.customerName}</span>
                      <span className="customer-email">{row.customerEmail}</span>
                    </td>
                    <td data-label="Rating">
                      <span title={`${row.rating} of 5`}>
                        {stars(row.rating)}
                      </span>
                    </td>
                    <td data-label="Comment" className="comment-cell">
                      {row.comment}
                    </td>
                    <td data-label="AI analysis" className="analysis-cell">
                      {row.analysis ? (
                        <div className="analysis-block">
                          <div className="analysis-meta">
                            <span className={sentimentClass(row.analysis.sentiment)}>
                              {row.analysis.sentiment}
                            </span>
                            <span className="badge badge--category">
                              {row.analysis.category}
                            </span>
                          </div>
                          <p className="analysis-summary">{row.analysis.summary}</p>
                        </div>
                      ) : (
                        <span className="analysis-placeholder">Not analyzed</span>
                      )}
                      {analyzeErrors[row.id] && (
                        <p className="analysis-error" role="alert">
                          {analyzeErrors[row.id]}
                        </p>
                      )}
                    </td>
                    <td data-label="Created">{formatDate(row.createdAtUtc)}</td>
                    <td className="actions-cell">
                      <button
                        type="button"
                        className="btn btn--ghost btn--sm"
                        disabled={analyzingId === row.id}
                        onClick={() => void handleAnalyze(row)}
                      >
                        {analyzingId === row.id ? '…' : 'Analyze'}
                      </button>
                      <button
                        type="button"
                        className="btn btn--ghost btn--sm"
                        onClick={() =>
                          setModal({ kind: 'edit', feedback: row })
                        }
                      >
                        Edit
                      </button>
                      <button
                        type="button"
                        className="btn btn--danger btn--sm"
                        disabled={deletingId === row.id}
                        onClick={() => void handleDelete(row)}
                      >
                        {deletingId === row.id ? '…' : 'Delete'}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        <nav className="pagination" aria-label="Pagination">
          <button
            type="button"
            className="btn btn--ghost"
            disabled={page <= 1 || listLoading}
            onClick={() => setPage((p) => Math.max(1, p - 1))}
          >
            Previous
          </button>
          <span className="pagination-page">Page {page}</span>
          <button
            type="button"
            className="btn btn--ghost"
            disabled={!hasNextPage || listLoading}
            onClick={() => setPage((p) => p + 1)}
          >
            Next
          </button>
        </nav>
      </section>

      {modal.kind === 'create' && (
        <Modal title="Add feedback" onClose={() => setModal({ kind: 'closed' })}>
          <FeedbackForm
            submitLabel="Create"
            onSubmit={handleCreate}
            onCancel={() => setModal({ kind: 'closed' })}
          />
        </Modal>
      )}

      {modal.kind === 'edit' && (
        <Modal title="Edit feedback" onClose={() => setModal({ kind: 'closed' })}>
          <FeedbackForm
            key={modal.feedback.id}
            submitLabel="Save changes"
            initial={{
              customerName: modal.feedback.customerName,
              customerEmail: modal.feedback.customerEmail,
              rating: modal.feedback.rating,
              comment: modal.feedback.comment,
            }}
            onSubmit={(input) => handleUpdate(modal.feedback.id, input)}
            onCancel={() => setModal({ kind: 'closed' })}
          />
        </Modal>
      )}
    </div>
  )
}