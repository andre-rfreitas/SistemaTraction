import { render, screen } from '@testing-library/react'
import { Button } from './button'

test('em loading fica desabilitado e mostra spinner', () => {
  render(<Button isLoading>Salvar</Button>)
  const btn = screen.getByRole('button', { name: /salvar/i })
  expect(btn).toBeDisabled()
  expect(btn.querySelector('svg')).toBeInTheDocument()
})
