import { apiRequest } from './client'
import type { Feedback, FeedbackInput, FeedbackStats } from '../types/feedback'

const listPath = '/api/feedback'

export async function listFeedback(
  page: number,
  pageSize: number,
  rating?: number,
): Promise<Feedback[]> {
  const params = new URLSearchParams({
    page: String(page),
    pageSize: String(pageSize),
  })
  if (rating !== undefined) {
    params.set('rating', String(rating))
  }
  return apiRequest<Feedback[]>(`${listPath}?${params}`)
}

export async function createFeedback(input: FeedbackInput): Promise<Feedback> {
  return apiRequest<Feedback>(listPath, {
    method: 'POST',
    body: JSON.stringify(input),
  })
}

export async function updateFeedback(
  id: string,
  input: FeedbackInput,
): Promise<Feedback> {
  return apiRequest<Feedback>(`${listPath}/${id}`, {
    method: 'PUT',
    body: JSON.stringify(input),
  })
}

export async function deleteFeedback(id: string): Promise<void> {
  await apiRequest<void>(`${listPath}/${id}`, { method: 'DELETE' })
}

export async function analyzeFeedback(id: string): Promise<Feedback> {
  return apiRequest<Feedback>(`${listPath}/${id}/analyze`, { method: 'POST' })
}

async function fetchAllFeedback(rating?: number): Promise<Feedback[]> {
  const all: Feedback[] = []
  let page = 1
  const pageSize = 100

  while (true) {
    const batch = await listFeedback(page, pageSize, rating)
    all.push(...batch)
    if (batch.length < pageSize) {
      break
    }
    page += 1
  }

  return all
}

export async function loadFeedbackStats(): Promise<FeedbackStats> {
  const items = await fetchAllFeedback()
  const distribution: FeedbackStats['distribution'] = {
    1: 0,
    2: 0,
    3: 0,
    4: 0,
    5: 0,
  }

  for (const item of items) {
    const key = item.rating as 1 | 2 | 3 | 4 | 5
    if (key >= 1 && key <= 5) {
      distribution[key] += 1
    }
  }

  const total = items.length
  const averageRating =
    total === 0
      ? 0
      : items.reduce((sum, item) => sum + item.rating, 0) / total

  return { total, averageRating, distribution }
}