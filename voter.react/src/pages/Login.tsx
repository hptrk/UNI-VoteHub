import { useEffect, useState } from 'react';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

interface LoginFormValues {
  email: string;
  password: string;
}

const Login = () => {
  const { login, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [error, setError] = useState('');

  useEffect(() => {
    // redirect if already logged in
    if (isAuthenticated) {
      navigate('/');
    }
  }, [isAuthenticated, navigate]);

  const formik = useFormik<LoginFormValues>({
    initialValues: {
      email: '',
      password: '',
    },
    validationSchema: Yup.object({
      email: Yup.string()
        .email('Invalid email address')
        .required('Email is required'),
      password: Yup.string().required('Password is required'),
    }),
    onSubmit: async (values) => {
      try {
        setError('');
        await login({
          email: values.email,
          password: values.password,
        });
        navigate('/');
      } catch (err: any) {
        setError(err.detail || 'Login failed. Please check your credentials.');
      }
    },
  });
  return (
    <div className="container mt-5 pt-3">
      <div className="row justify-content-center">
        <div className="col-md-6 col-lg-5">
          <div className="text-center mb-5 fade-in">
            <div
              className="d-inline-block p-3 rounded-circle mb-3"
              style={{
                background: 'var(--gradient-primary)',
                boxShadow: '0 10px 25px -5px rgba(99, 102, 241, 0.5)',
              }}
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="40"
                height="40"
                fill="white"
                className="bi bi-check2-square"
                viewBox="0 0 16 16"
              >
                <path d="M3 14.5A1.5 1.5 0 0 1 1.5 13V3A1.5 1.5 0 0 1 3 1.5h8a.5.5 0 0 1 0 1H3a.5.5 0 0 0-.5.5v10a.5.5 0 0 0 .5.5h10a.5.5 0 0 0 .5-.5V8a.5.5 0 0 1 1 0v5a1.5 1.5 0 0 1-1.5 1.5z" />
                <path d="m8.354 10.354 7-7a.5.5 0 0 0-.708-.708L8 9.293 5.354 6.646a.5.5 0 1 0-.708.708l3 3a.5.5 0 0 0 .708 0" />
              </svg>
            </div>
            <h2
              className="mt-4 mb-2"
              style={{
                fontFamily: 'Plus Jakarta Sans, sans-serif',
                fontWeight: 800,
              }}
            >
              Welcome Back
            </h2>
            <p className="text-muted" style={{ fontSize: '1.05rem' }}>
              Sign in to continue to VoteHub
            </p>
          </div>

          <div
            className="card shadow border-0 slide-in"
            style={{ borderRadius: '16px', overflow: 'hidden' }}
          >
            <div className="card-body p-4 p-lg-5">
              {error && (
                <div
                  className="alert alert-danger d-flex align-items-center rounded-3 border-0 shadow-sm mb-4"
                  role="alert"
                >
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    width="18"
                    height="18"
                    fill="currentColor"
                    className="bi bi-exclamation-triangle-fill flex-shrink-0 me-2"
                    viewBox="0 0 16 16"
                  >
                    <path d="M8.982 1.566a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767L8.982 1.566zM8 5c.535 0 .954.462.9.995l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 5.995A.905.905 0 0 1 8 5zm.002 6a1 1 0 1 1 0 2 1 1 0 0 1 0-2z" />
                  </svg>
                  <div className="fw-medium">{error}</div>
                </div>
              )}

              <form onSubmit={formik.handleSubmit}>
                <div className="mb-4">
                  <label htmlFor="email" className="form-label fw-medium mb-2">
                    Email Address
                  </label>
                  <div className="input-group">
                    <span className="input-group-text bg-light border-end-0">
                      <svg
                        xmlns="http://www.w3.org/2000/svg"
                        width="16"
                        height="16"
                        fill="currentColor"
                        className="bi bi-envelope"
                        viewBox="0 0 16 16"
                      >
                        <path d="M0 4a2 2 0 0 1 2-2h12a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2zm2-1a1 1 0 0 0-1 1v.217l7 4.2 7-4.2V4a1 1 0 0 0-1-1zm13 2.383-4.708 2.825L15 11.105zm-.034 6.876-5.64-3.471L8 9.583l-1.326-.795-5.64 3.47A1 1 0 0 0 2 13h12a1 1 0 0 0 .966-.741M1 11.105l4.708-2.897L1 5.383z" />
                      </svg>
                    </span>
                    <input
                      type="email"
                      className={`form-control form-control-lg ${
                        formik.touched.email && formik.errors.email
                          ? 'is-invalid'
                          : ''
                      }`}
                      id="email"
                      name="email"
                      placeholder="name@example.com"
                      onChange={formik.handleChange}
                      onBlur={formik.handleBlur}
                      value={formik.values.email}
                    />
                    {formik.touched.email && formik.errors.email && (
                      <div className="invalid-feedback">
                        {formik.errors.email as string}
                      </div>
                    )}
                  </div>
                </div>

                <div className="mb-4">
                  {' '}
                  <div className="mb-2">
                    <label
                      htmlFor="password"
                      className="form-label fw-medium mb-0"
                    >
                      Password
                    </label>
                  </div>
                  <div className="input-group">
                    <span className="input-group-text bg-light border-end-0">
                      <svg
                        xmlns="http://www.w3.org/2000/svg"
                        width="16"
                        height="16"
                        fill="currentColor"
                        className="bi bi-lock"
                        viewBox="0 0 16 16"
                      >
                        <path d="M8 1a2 2 0 0 1 2 2v4H6V3a2 2 0 0 1 2-2m3 6V3a3 3 0 0 0-6 0v4a2 2 0 0 0-2 2v5a2 2 0 0 0 2 2h6a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2M5 8h6a1 1 0 0 1 1 1v5a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1V9a1 1 0 0 1 1-1" />
                      </svg>
                    </span>
                    <input
                      type="password"
                      className={`form-control form-control-lg ${
                        formik.touched.password && formik.errors.password
                          ? 'is-invalid'
                          : ''
                      }`}
                      id="password"
                      name="password"
                      placeholder="Enter your password"
                      onChange={formik.handleChange}
                      onBlur={formik.handleBlur}
                      value={formik.values.password}
                    />
                    {formik.touched.password && formik.errors.password && (
                      <div className="invalid-feedback">
                        {formik.errors.password as string}
                      </div>
                    )}
                  </div>
                </div>

                <div className="d-grid mt-4">
                  <button
                    type="submit"
                    className="btn btn-primary btn-lg py-3"
                    disabled={formik.isSubmitting}
                  >
                    {formik.isSubmitting ? (
                      <>
                        <span
                          className="spinner-border spinner-border-sm me-2"
                          role="status"
                          aria-hidden="true"
                        ></span>
                        Signing in...
                      </>
                    ) : (
                      'Sign In'
                    )}
                  </button>
                </div>
              </form>
            </div>
          </div>

          <div
            className="text-center mt-4 slide-in"
            style={{ animationDelay: '0.2s' }}
          >
            <p className="mb-0 fs-5">
              Don't have an account?{' '}
              <a href="/register" className="fw-semibold text-primary">
                Register now
              </a>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Login;
