import { useState } from 'react'
import type { AppConfigDto } from '../types'
import { useUpsertAppConfig } from '../hooks/useUpsertAppConfig'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'

interface Props {
  config: AppConfigDto
}

export function AppConfigRow({ config }: Props) {
  const [value, setValue] = useState(config.value)
  const upsert = useUpsertAppConfig()

  const isDirty = value !== config.value

  function handleSave() {
    upsert.mutate(
      { key: config.key, value },
      { onSuccess: () => {} }
    )
  }

  return (
    <div className="border rounded-lg p-4 bg-white space-y-2">
      <div className="flex items-start justify-between gap-2">
        <div className="min-w-0">
          <p className="font-mono text-sm font-semibold text-neutral-800 truncate">
            {config.key}
          </p>
          {config.description && (
            <p className="text-xs text-neutral-500 mt-0.5">{config.description}</p>
          )}
        </div>
      </div>
      <div className="flex gap-2">
        <Input
          value={value}
          onChange={(e) => setValue(e.target.value)}
          className="font-mono text-sm"
        />
        <Button
          size="sm"
          disabled={!isDirty || upsert.isPending}
          onClick={handleSave}
        >
          {upsert.isPending ? 'Salvando...' : 'Salvar'}
        </Button>
      </div>
      {upsert.isError && (
        <p className="text-xs text-red-500">{(upsert.error as Error).message}</p>
      )}
      {upsert.isSuccess && !isDirty && (
        <p className="text-xs text-green-600">Salvo ✓</p>
      )}
    </div>
  )
}
