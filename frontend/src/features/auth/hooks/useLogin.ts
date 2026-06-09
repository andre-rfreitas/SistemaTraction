import { useMutation } from '@tanstack/react-query'
import { api } from '@/lib/api'
import { useAuth } from '@/lib/auth'

export function useLogin() {
  const { setAuthenticated } = useAuth()
  return useMutation({
    mutationFn: (password: string) => api.post('/auth/login', { password }),
    onSuccess: () => setAuthenticated(true),
  })
}
