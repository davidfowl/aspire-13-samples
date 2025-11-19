import { useEffect, useState } from 'react'
import { useAuth } from '../hooks/useAuth'

interface ProfileData {
  message: string
  username: string
  email: string
  timestamp: string
}

export default function Protected() {
  const { user, loading, login } = useAuth()
  const [profileData, setProfileData] = useState<ProfileData | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loadingData, setLoadingData] = useState(false)

  useEffect(() => {
    if (!loading && user?.authenticated) {
      fetchProtectedData()
    }
  }, [loading, user])

  const fetchProtectedData = async () => {
    setLoadingData(true)
    setError(null)
    try {
      const response = await fetch('/api/data/profile')
      if (!response.ok) {
        throw new Error('Failed to fetch protected data')
      }
      const data = await response.json()
      setProfileData(data)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred')
    } finally {
      setLoadingData(false)
    }
  }

  if (loading) {
    return <div className="loading">Loading...</div>
  }

  if (!user?.authenticated) {
    return (
      <div className="card">
        <h2>Protected Page</h2>
        <p>This page requires authentication. Please log in to continue.</p>
        <button onClick={() => login('/protected')}>Login</button>
      </div>
    )
  }

  return (
    <div className="card">
      <h2>Protected Page</h2>
      <p>Welcome! You have successfully authenticated and can access this protected content.</p>

      {loadingData && <div className="loading">Loading profile data...</div>}

      {error && <div className="error">{error}</div>}

      {profileData && (
        <div className="user-info">
          <h3>Protected Data from BFF</h3>
          <dl>
            <dt>Message:</dt>
            <dd>{profileData.message}</dd>
            <dt>Username:</dt>
            <dd>{profileData.username}</dd>
            <dt>Email:</dt>
            <dd>{profileData.email}</dd>
            <dt>Timestamp:</dt>
            <dd>{new Date(profileData.timestamp).toLocaleString()}</dd>
          </dl>
        </div>
      )}

      <button onClick={fetchProtectedData} style={{ marginTop: '1rem' }}>
        Refresh Data
      </button>
    </div>
  )
}
