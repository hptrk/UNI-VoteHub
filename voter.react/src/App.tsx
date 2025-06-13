import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import Home from './pages/Home';
import Login from './pages/Login';
import Register from './pages/Register';
import PollDetail from './pages/PollDetail';
import PollResults from './pages/PollResults';
import ClosedPolls from './pages/ClosedPolls';
import Navbar from './components/Navbar';
import './App.css';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap/dist/js/bootstrap.bundle.min.js';

function App() {
  return (
    <Router>
      <AuthProvider>
        <div className="d-flex flex-column min-vh-100">
          <Navbar />
          <main className="flex-grow-1 py-4">
            <Routes>
              <Route path="/" element={<Home />} />
              <Route path="/login" element={<Login />} />
              <Route path="/register" element={<Register />} />
              <Route path="/polls/closed" element={<ClosedPolls />} />
              <Route path="/polls/:id" element={<PollDetail />} />
              <Route path="/polls/:id/results" element={<PollResults />} />
            </Routes>
          </main>
          <footer
            className="mt-auto py-5"
            style={{ backgroundColor: 'var(--gray-900)' }}
          >
            <div className="container">
              <div className="row">
                <div className="col-lg-4 mb-4 mb-lg-0">
                  <div className="d-flex align-items-center mb-3">
                    <div
                      className="me-2 d-flex align-items-center justify-content-center"
                      style={{
                        background: 'var(--gradient-primary)',
                        borderRadius: '10px',
                        width: '32px',
                        height: '32px',
                      }}
                    >
                      <svg
                        xmlns="http://www.w3.org/2000/svg"
                        width="18"
                        height="18"
                        fill="white"
                        className="bi bi-check2-square"
                        viewBox="0 0 16 16"
                      >
                        <path d="M3 14.5A1.5 1.5 0 0 1 1.5 13V3A1.5 1.5 0 0 1 3 1.5h8a.5.5 0 0 1 0 1H3a.5.5 0 0 0-.5.5v10a.5.5 0 0 0 .5.5h10a.5.5 0 0 0 .5-.5V8a.5.5 0 0 1 1 0v5a1.5 1.5 0 0 1-1.5 1.5z" />
                        <path d="m8.354 10.354 7-7a.5.5 0 0 0-.708-.708L8 9.293 5.354 6.646a.5.5 0 1 0-.708.708l3 3a.5.5 0 0 0 .708 0" />
                      </svg>
                    </div>
                    <span
                      style={{
                        fontFamily: 'Plus Jakarta Sans, sans-serif',
                        fontWeight: 800,
                        color: 'white',
                        fontSize: '1.3rem',
                        letterSpacing: '-0.5px',
                      }}
                    >
                      VoteHub
                    </span>
                  </div>
                  <p
                    className="text-white-50 mb-4"
                    style={{ fontSize: '0.9rem' }}
                  >
                    A modern polling platform designed to gather anonymous
                    feedback and make collaborative decisions.
                  </p>
                </div>

                <div className="col-6 col-lg-2 mb-4 mb-lg-0">
                  <h6 className="text-white mb-3">Quick Links</h6>
                  <ul className="list-unstyled mb-0">
                    <li className="mb-2">
                      <a
                        href="/"
                        className="text-white-50 text-decoration-none"
                      >
                        Active Polls
                      </a>
                    </li>
                    <li className="mb-2">
                      <a
                        href="/polls/closed"
                        className="text-white-50 text-decoration-none"
                      >
                        Closed Polls
                      </a>
                    </li>
                  </ul>
                </div>

                <div className="col-6 col-lg-2 mb-4 mb-lg-0">
                  <h6 className="text-white mb-3">Account</h6>
                  <ul className="list-unstyled mb-0">
                    <li className="mb-2">
                      <a
                        href="/login"
                        className="text-white-50 text-decoration-none"
                      >
                        Sign In
                      </a>
                    </li>
                    <li className="mb-2">
                      <a
                        href="/register"
                        className="text-white-50 text-decoration-none"
                      >
                        Create Account
                      </a>
                    </li>
                  </ul>
                </div>

                <div className="col-lg-4">
                  <h6 className="text-white mb-3">About VoteHub</h6>
                  <p className="text-white-50" style={{ fontSize: '0.9rem' }}>
                    VoteHub is a university assignment for the ELTE Modern Web
                    Technologies course. The backend is developed with .NET,
                    while the frontend uses React for a responsive user
                    experience.
                  </p>
                </div>
              </div>

              <hr
                className="mt-4 mb-4"
                style={{ borderColor: 'rgba(255,255,255,0.1)' }}
              />

              <div className="row align-items-center">
                <div className="col-md-6 text-center text-md-start">
                  <p
                    className="text-white-50 mb-0"
                    style={{ fontSize: '0.9rem' }}
                  >
                    &copy; {new Date().getFullYear()} VoteHub. All rights
                    reserved.
                  </p>
                </div>
                <div className="col-md-6 text-center text-md-end mt-3 mt-md-0">
                  {/* Privacy Policy and Terms of Service links removed */}
                </div>
              </div>
            </div>
          </footer>
        </div>
      </AuthProvider>
    </Router>
  );
}

export default App;
