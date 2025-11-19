import { createContext, useContext, useEffect, useState, ReactNode } from 'react'

interface User {
  authenticated: boolean
  username?: string
  email?: string
  firstName?: string
  lastName?: string
}

interface AuthContextType {
  user: User | null
  loading: boolean
  login: (returnUrl?: string) => void
  logout: () => void
  refreshUser: () => Promise<void>
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [loading, setLoading] = useState(true)

  const refreshUser = async () => {
    try {
      const response = await fetch('/api/auth/user')
      const data = await response.json()
      setUser(data)
    } catch (error) {
      console.error('Failed to fetch user info:', error)
      setUser({ authenticated: false })
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    refreshUser()
  }, [])

  const login = (returnUrl?: string) => {
    const url = returnUrl
      ? `/api/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`
      : '/api/auth/login'
    window.location.href = url
  }

  const logout = () => {
    // For OIDC logout, we need the browser to follow redirects to Keycloak and back
    // Using a form POST allows the browser to naturally handle the redirect chain
    const form = document.createElement('form')
    form.method = 'POST'
    form.action = '/api/auth/logout'
    document.body.appendChild(form)
    form.submit()
  }

  return (
    <AuthContext.Provider value={{ user, loading, login, logout, refreshUser }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}
