import { render, screen, act } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { ThemeProvider } from './theme'
import { ThemeToggle } from '@/components/ui/theme-toggle'

beforeEach(() => {
  localStorage.clear()
  document.documentElement.classList.remove('dark')
})

test('alterna a classe dark no html e persiste no localStorage', async () => {
  const user = userEvent.setup()
  render(
    <ThemeProvider>
      <ThemeToggle />
    </ThemeProvider>,
  )
  expect(document.documentElement.classList.contains('dark')).toBe(false)
  await act(async () => {
    await user.click(screen.getByRole('button', { name: /tema/i }))
  })
  expect(document.documentElement.classList.contains('dark')).toBe(true)
  expect(localStorage.getItem('theme')).toBe('dark')
})
