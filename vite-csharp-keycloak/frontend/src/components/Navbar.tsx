import { Link } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

export default function Navbar() {
  const { user, loading, login, logout } = useAuth()

  return (
    <nav>
      <div className="nav-content">
        <h1>Keycloak Demo</h1>
        <div className="nav-links">
          <Link to="/">Home</Link>
          <Link to="/protected">Protected</Link>
          {!loading && (
            <>
              {user?.authenticated ? (
                <>
                  <span>Welcome, {user.username}!</span>
                  <button onClick={logout} className="secondary">Logout</button>
                </>
              ) : (
                <button onClick={() => login()}>Login</button>
              )}
            </>
          )}
        </div>
      </div>
    </nav>
  )
}
