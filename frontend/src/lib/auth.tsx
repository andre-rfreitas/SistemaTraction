import { createContext, useContext, useEffect, useState } from 'react'
import { api } from './api'

interface AuthState {
  isAuthenticated: boolean
  isLoading: boolean
  setAuthenticated: (value: boolean) => void
}

const AuthContext = createContext<AuthState | null>(null)

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [isAuthenticated, setAuthenticated] = useState(false)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const handleUnauthorized = () => setAuthenticated(false)
    window.addEventListener('auth:logout', handleUnauthorized)
    return () => window.removeEventListener('auth:logout', handleUnauthorized)
  }, [])

  useEffect(() => {
    api
      .get('/auth/me')
      .then(() => setAuthenticated(true))
      .catch(() => setAuthenticated(false))
      .finally(() => setIsLoading(false))
  }, [])

  return (
    <AuthContext.Provider value={{ isAuthenticated, isLoading, setAuthenticated }}>
      {children}
    </AuthContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth deve ser usado dentro de AuthProvider')
  return ctx
}
