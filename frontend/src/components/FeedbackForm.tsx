import { useState, type FormEvent } from 'react'
import type { FeedbackInput } from '../types/feedback'

type FeedbackFormProps = {
  initial?: FeedbackInput
  submitLabel: string
  onSubmit: (input: FeedbackInput) => Promise<void>
  onCancel?: () => void
}

const empty: FeedbackInput = {
  customerName: '',
  customerEmail: '',
  rating: 5,
  comment: '',
}

export function FeedbackForm({
  initial,
  submitLabel,
  onSubmit,
  onCancel,
}: FeedbackFormProps) {
  const [values, setValues] = useState<FeedbackInput>(initial ?? empty)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSaving(true)
    setError(null)
    try {
      await onSubmit(values)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Save failed')
    } finally {
      setSaving(false)
    }
  }

  return (
    <form className="feedback-form" onSubmit={handleSubmit}>
      {error && (
        <p className="status status--error" role="alert">
          {error}
        </p>
      )}
      <label>
        Customer name
        <input
          type="text"
          required
          maxLength={200}
          value={values.customerName}
          onChange={(e) =>
            setValues((v) => ({ ...v, customerName: e.target.value }))
          }
        />
      </label>
      <label>
        Email
        <input
          type="email"
          required
          maxLength={320}
          value={values.customerEmail}
          onChange={(e) =>
            setValues((v) => ({ ...v, customerEmail: e.target.value }))
          }
        />
      </label>
      <label>
        Rating
        <select
          value={values.rating}
          onChange={(e) =>
            setValues((v) => ({ ...v, rating: Number(e.target.value) }))
          }
        >
          {[5, 4, 3, 2, 1].map((r) => (
            <option key={r} value={r}>
              {r} {r === 1 ? 'star' : 'stars'}
            </option>
          ))}
        </select>
      </label>
      <label>
        Comment
        <textarea
          required
          minLength={10}
          maxLength={4000}
          rows={4}
          value={values.comment}
          onChange={(e) =>
            setValues((v) => ({ ...v, comment: e.target.value }))
          }
        />
      </label>
      <div className="form-actions">
        {onCancel && (
          <button type="button" className="btn btn--ghost" onClick={onCancel}>
            Cancel
          </button>
        )}
        <button type="submit" className="btn btn--primary" disabled={saving}>
          {saving ? 'Saving…' : submitLabel}
        </button>
      </div>
    </form>
  )
}