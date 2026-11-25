export type FeedbackAnalysis = {
  sentiment: 'Positive' | 'Neutral' | 'Negative' | string
  category: string
  summary: string
  analyzedAtUtc: string
}

export type Feedback = {
  id: string
  customerName: string
  customerEmail: string
  rating: number
  comment: string
  createdAtUtc: string
  updatedAtUtc: string | null
  analysis?: FeedbackAnalysis | null
}

export type FeedbackInput = {
  customerName: string
  customerEmail: string
  rating: number
  comment: string
}

export type FeedbackStats = {
  total: number
  averageRating: number
  distribution: Record<1 | 2 | 3 | 4 | 5, number>
}