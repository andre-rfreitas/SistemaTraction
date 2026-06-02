import * as React from 'react'
import { cn } from '@/lib/utils'

type Tone = 'default' | 'success' | 'danger'

interface StatCardProps {
  label: string
  value: React.ReactNode
  hint?: string
  tone?: Tone
  className?: string
}

const toneClass: Record<Tone, string> = {
  default: 'text-foreground',
  success: 'text-success',
  danger: 'text-danger',
}

export function StatCard({ label, value, hint, tone = 'default', className }: StatCardProps) {
  return (
    <div className={cn('rounded-lg border border-border bg-card p-4 shadow-sm', className)}>
      <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">{label}</p>
      <p className={cn('mt-1.5 text-2xl font-semibold tabular-nums', toneClass[tone])}>{value}</p>
      {hint && <p className="mt-1 text-xs text-muted-foreground">{hint}</p>}
    </div>
  )
}
