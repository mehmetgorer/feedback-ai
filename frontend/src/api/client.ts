const apiBase = (import.meta.env.VITE_API_BASE_URL as string | undefined) ?? ''

export class ApiError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

function buildUrl(path: string): string {
  const base = apiBase.replace(/\/$/, '')
  const normalized = path.startsWith('/') ? path : `/${path}`
  return `${base}${normalized}`
}

export async function apiRequest<T>(
  path: string,
  init?: RequestInit,
): Promise<T> {
  const response = await fetch(buildUrl(path), {
    ...init,
    headers: {
      Accept: 'application/json',
      ...(init?.body ? { 'Content-Type': 'application/json' } : {}),
      ...init?.headers,
    },
  })

  if (!response.ok) {
    const text = await response.text()
    let message = response.statusText || 'Request failed'
    try {
      const problem = JSON.parse(text) as {
        title?: string
        detail?: string
        errors?: Record<string, string[]>
      }
      if (problem.errors) {
        const fromErrors = Object.values(problem.errors).flat().join(' ')
        if (fromErrors) {
          message = fromErrors
        }
      } else if (problem.detail) {
        message = problem.detail
      } else if (problem.title) {
        message = problem.title
      }
    } catch {
      if (text) {
        message = text
      }
    }
    throw new ApiError(response.status, message)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}