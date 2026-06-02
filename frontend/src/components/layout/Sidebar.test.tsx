import { render, screen } from '@testing-library/react'
import { Sidebar } from './Sidebar'

test('marca o item ativo com aria-current', () => {
  render(<Sidebar active="financial" onSelect={() => {}} />)
  const ativo = screen.getByRole('button', { name: /financeiro/i })
  expect(ativo).toHaveAttribute('aria-current', 'page')
})
