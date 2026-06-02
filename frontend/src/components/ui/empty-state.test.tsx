import { render, screen } from '@testing-library/react'
import { Inbox } from 'lucide-react'
import { EmptyState } from './empty-state'

test('renderiza titulo, descricao e acao', () => {
  render(
    <EmptyState
      icon={Inbox}
      title="Sem lançamentos"
      description="Nada por aqui ainda."
      action={<button>Adicionar</button>}
    />,
  )
  expect(screen.getByText('Sem lançamentos')).toBeInTheDocument()
  expect(screen.getByText('Nada por aqui ainda.')).toBeInTheDocument()
  expect(screen.getByRole('button', { name: 'Adicionar' })).toBeInTheDocument()
})
