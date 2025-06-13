import { useState, useEffect } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

const Navbar = () => {
  const { isAuthenticated, logout, user } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [scrolled, setScrolled] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  // scroll effect for navbar
  useEffect(() => {
    const handleScroll = () => {
      if (window.scrollY > 50) {
        setScrolled(true);
      } else {
        setScrolled(false);
      }
    };

    window.addEventListener('scroll', handleScroll);
    return () => {
      window.removeEventListener('scroll', handleScroll);
    };
  }, []);

  useEffect(() => {
    setMobileMenuOpen(false);
  }, [location]);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <nav
      className={`navbar navbar-expand-lg ${
        scrolled ? 'shadow navbar-scrolled' : ''
      }`}
      style={{
        background: scrolled ? 'rgba(255, 255, 255, 0.98)' : 'var(--card-bg)',
        backdropFilter: scrolled ? 'blur(10px)' : 'none',
        transition: 'all 0.4s cubic-bezier(0.165, 0.84, 0.44, 1)',
        position: 'sticky',
        top: 0,
        zIndex: 1000,
        padding: scrolled ? '0.75rem 0' : '1.25rem 0',
      }}
    >
      <div className="container">
        <Link
          className="navbar-brand d-flex align-items-center"
          to="/"
          style={{ opacity: 1 }}
        >
          <div
            className="me-2 d-flex align-items-center justify-content-center"
            style={{
              background: 'var(--gradient-primary)',
              borderRadius: '12px',
              width: '36px',
              height: '36px',
            }}
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="20"
              height="20"
              fill="white"
              className="bi bi-check2-square"
              viewBox="0 0 16 16"
            >
              <path d="M3 14.5A1.5 1.5 0 0 1 1.5 13V3A1.5 1.5 0 0 1 3 1.5h8a.5.5 0 0 1 0 1H3a.5.5 0 0 0-.5.5v10a.5.5 0 0 0 .5.5h10a.5.5 0 0 0 .5-.5V8a.5.5 0 0 1 1 0v5a1.5 1.5 0 0 1-1.5 1.5z" />
              <path d="m8.354 10.354 7-7a.5.5 0 0 0-.708-.708L8 9.293 5.354 6.646a.5.5 0 1 0-.708.708l3 3a.5.5 0 0 0 .708 0" />
            </svg>
          </div>
          <div>
            <span
              style={{
                fontFamily: 'Plus Jakarta Sans, sans-serif',
                fontWeight: 800,
                background: 'var(--gradient-primary)',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent',
                backgroundClip: 'text',
                fontSize: '1.5rem',
                letterSpacing: '-0.5px',
              }}
            >
              VoteHub
            </span>
          </div>
        </Link>{' '}
        <button
          className="navbar-toggler border-0"
          type="button"
          data-bs-toggle="collapse"
          data-bs-target="#navbarNav"
          aria-controls="navbarNav"
          aria-expanded={mobileMenuOpen}
          aria-label="Toggle navigation"
          onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
        >
          <span className="navbar-toggler-icon"></span>
        </button>
        <div
          className={`collapse navbar-collapse ${mobileMenuOpen ? 'show' : ''}`}
          id="navbarNav"
        >
          <ul className="navbar-nav mx-auto">
            <li className="nav-item mx-1">
              <Link
                className={`nav-link px-3 py-2 ${
                  location.pathname === '/' ? 'active fw-bold' : ''
                }`}
                to="/"
                style={{
                  borderRadius: '8px',
                  transition: 'all 0.3s ease',
                  backgroundColor:
                    location.pathname === '/'
                      ? 'var(--gray-100)'
                      : 'transparent',
                  color:
                    location.pathname === '/'
                      ? 'var(--primary-color)'
                      : 'var(--gray-700)',
                  position: 'relative',
                }}
              >
                <span className="d-flex align-items-center">
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    width="18"
                    height="18"
                    fill="currentColor"
                    className="bi bi-lightning-charge me-2"
                    viewBox="0 0 16 16"
                  >
                    <path d="M11.251.068a.5.5 0 0 1 .227.58L9.677 6.5H13a.5.5 0 0 1 .364.843l-8 8.5a.5.5 0 0 1-.842-.49L6.323 9.5H3a.5.5 0 0 1-.364-.843l8-8.5a.5.5 0 0 1 .615-.09z" />
                  </svg>
                  Active Polls
                </span>
                {location.pathname === '/' && (
                  <span className="active-indicator"></span>
                )}
              </Link>
            </li>
            <li className="nav-item mx-1">
              <Link
                className={`nav-link px-3 py-2 ${
                  location.pathname === '/polls/closed' ? 'active fw-bold' : ''
                }`}
                to="/polls/closed"
                style={{
                  borderRadius: '8px',
                  backgroundColor:
                    location.pathname === '/polls/closed'
                      ? 'var(--gray-100)'
                      : 'transparent',
                  color:
                    location.pathname === '/polls/closed'
                      ? 'var(--primary-color)'
                      : 'var(--gray-700)',
                  transition: 'all 0.3s ease',
                  position: 'relative',
                }}
              >
                <span className="d-flex align-items-center">
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    width="18"
                    height="18"
                    fill="currentColor"
                    className="bi bi-archive me-2"
                    viewBox="0 0 16 16"
                  >
                    <path d="M0 2a1 1 0 0 1 1-1h14a1 1 0 0 1 1 1v2a1 1 0 0 1-1 1v7.5a2.5 2.5 0 0 1-2.5 2.5h-9A2.5 2.5 0 0 1 1 12.5V5a1 1 0 0 1-1-1zm2 3v7.5A1.5 1.5 0 0 0 3.5 14h9a1.5 1.5 0 0 0 1.5-1.5V5zm13-3H1v2h14zM5 7.5a.5.5 0 0 1 .5-.5h5a.5.5 0 0 1 0 1h-5a.5.5 0 0 1-.5-.5" />
                  </svg>
                  Closed Polls
                </span>
                {location.pathname === '/polls/closed' && (
                  <span className="active-indicator"></span>
                )}
              </Link>
            </li>
          </ul>

          <ul className="navbar-nav">
            {isAuthenticated ? (
              <li className="nav-item dropdown">
                <a
                  className="nav-link dropdown-toggle d-flex align-items-center"
                  href="#"
                  role="button"
                  data-bs-toggle="dropdown"
                  aria-expanded="false"
                  style={{
                    backgroundColor: 'var(--gray-100)',
                    borderRadius: '30px',
                    padding: '0.5rem 1rem',
                    fontWeight: 500,
                  }}
                >
                  <div
                    className="me-2 d-flex align-items-center justify-content-center"
                    style={{
                      background: 'var(--gradient-primary)',
                      borderRadius: '50%',
                      width: '28px',
                      height: '28px',
                    }}
                  >
                    <span
                      style={{
                        color: 'white',
                        fontWeight: 600,
                        fontSize: '0.8rem',
                      }}
                    >
                      {user?.email?.charAt(0).toUpperCase() || 'U'}
                    </span>
                  </div>
                  {user?.email?.split('@')[0] || 'My Account'}
                </a>
                <ul
                  className="dropdown-menu dropdown-menu-end shadow-sm border-0"
                  style={{
                    borderRadius: '12px',
                    overflow: 'hidden',
                    marginTop: '10px',
                  }}
                >
                  <li>
                    <div
                      className="dropdown-item-text px-4 py-2"
                      style={{ color: 'var(--gray-500)', fontSize: '0.85rem' }}
                    >
                      Signed in as <strong>{user?.email}</strong>
                    </div>
                  </li>
                  <li>
                    <hr className="dropdown-divider" />
                  </li>
                  <li>
                    <button
                      className="dropdown-item d-flex align-items-center px-4 py-2"
                      onClick={handleLogout}
                    >
                      <svg
                        xmlns="http://www.w3.org/2000/svg"
                        width="16"
                        height="16"
                        fill="currentColor"
                        className="bi bi-box-arrow-right me-2"
                        viewBox="0 0 16 16"
                      >
                        <path
                          fillRule="evenodd"
                          d="M10 12.5a.5.5 0 0 1-.5.5h-8a.5.5 0 0 1-.5-.5v-9a.5.5 0 0 1 .5-.5h8a.5.5 0 0 1 .5.5v2a.5.5 0 0 0 1 0v-2A1.5 1.5 0 0 0 9.5 2h-8A1.5 1.5 0 0 0 0 3.5v9A1.5 1.5 0 0 0 1.5 14h8a1.5 1.5 0 0 0 1.5-1.5v-2a.5.5 0 0 0-1 0z"
                        />
                        <path
                          fillRule="evenodd"
                          d="M15.854 8.354a.5.5 0 0 0 0-.708l-3-3a.5.5 0 0 0-.708.708L14.293 7.5H5.5a.5.5 0 0 0 0 1h8.793l-2.147 2.146a.5.5 0 0 0 .708.708z"
                        />
                      </svg>
                      Sign Out
                    </button>
                  </li>
                </ul>
              </li>
            ) : (
              <>
                <li className="nav-item me-2">
                  <Link
                    to="/login"
                    className="btn btn-outline-primary px-4"
                    style={{
                      borderRadius: '8px',
                      fontWeight: 600,
                    }}
                  >
                    Sign In
                  </Link>
                </li>
                <li className="nav-item">
                  <Link
                    to="/register"
                    className="btn btn-primary px-4"
                    style={{
                      borderRadius: '8px',
                      fontWeight: 600,
                    }}
                  >
                    Register
                  </Link>
                </li>
              </>
            )}
          </ul>
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
