import { render, screen } from '@testing-library/react'
import { Badge } from './badge'

test('aplica classe da variante success', () => {
  render(<Badge variant="success">Receita</Badge>)
  expect(screen.getByText('Receita').className).toMatch(/success/)
})
