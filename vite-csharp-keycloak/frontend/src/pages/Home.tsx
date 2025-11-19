import { useAuth } from '../hooks/useAuth'

export default function Home() {
  const { user, loading } = useAuth()

  if (loading) {
    return <div className="loading">Loading...</div>
  }

  return (
    <div className="card">
      <h2>Welcome to the Keycloak Demo</h2>
      <p>
        This is a public page that anyone can access. It demonstrates the BFF (Backend for Frontend)
        pattern with Keycloak as the identity provider.
      </p>

      {user?.authenticated ? (
        <div className="user-info">
          <h3>You are logged in</h3>
          <dl>
            <dt>Username:</dt>
            <dd>{user.username}</dd>
            <dt>Email:</dt>
            <dd>{user.email}</dd>
            {user.firstName && (
              <>
                <dt>Name:</dt>
                <dd>{user.firstName} {user.lastName}</dd>
              </>
            )}
          </dl>
        </div>
      ) : (
        <div className="user-info">
          <p>You are not logged in. Click the Login button to authenticate with Keycloak.</p>
          <p><strong>Demo credentials:</strong> username: demo, password: demo</p>
        </div>
      )}
    </div>
  )
}
