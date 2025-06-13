import { useEffect, useState } from 'react';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

interface RegisterFormValues {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
}

const Register = () => {
  const { register, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => {
    // redirect if already logged in
    if (isAuthenticated) {
      navigate('/');
    }
  }, [isAuthenticated, navigate]);

  const formik = useFormik<RegisterFormValues>({
    initialValues: {
      username: '',
      email: '',
      password: '',
      confirmPassword: '',
    },
    validationSchema: Yup.object({
      username: Yup.string()
        .min(3, 'Username must be at least 3 characters')
        .max(50, 'Username must be less than 50 characters')
        .required('Username is required'),
      email: Yup.string()
        .email('Invalid email address')
        .required('Email is required'),
      password: Yup.string()
        .min(6, 'Password must be at least 6 characters')
        .required('Password is required'),
      confirmPassword: Yup.string()
        .oneOf([Yup.ref('password')], 'Passwords must match')
        .required('Please confirm your password'),
    }),
    onSubmit: async (values) => {
      try {
        setError('');
        setSuccess('');

        await register({
          username: values.username,
          email: values.email,
          password: values.password,
          confirmPassword: values.confirmPassword,
        });

        setSuccess('Registration successful! You can now log in.');
        setTimeout(() => {
          navigate('/login');
        }, 2000);
      } catch (err: any) {
        setError(err.detail || 'Registration failed. Please try again.');
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
                background: 'var(--gradient-secondary)',
                boxShadow: '0 10px 25px -5px rgba(79, 70, 229, 0.5)',
              }}
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="40"
                height="40"
                fill="white"
                className="bi bi-person-plus"
                viewBox="0 0 16 16"
              >
                <path d="M6 8a3 3 0 1 0 0-6 3 3 0 0 0 0 6m2-3a2 2 0 1 1-4 0 2 2 0 0 1 4 0m4 8c0 1-1 1-1 1H1s-1 0-1-1 1-4 6-4 6 3 6 4m-1-.004c-.001-.246-.154-.986-.832-1.664C9.516 10.68 8.289 10 6 10c-2.29 0-3.516.68-4.168 1.332-.678.678-.83 1.418-.832 1.664z" />
                <path
                  fillRule="evenodd"
                  d="M13.5 5a.5.5 0 0 1 .5.5V7h1.5a.5.5 0 0 1 0 1H14v1.5a.5.5 0 0 1-1 0V8h-1.5a.5.5 0 0 1 0-1H13V5.5a.5.5 0 0 1 .5-.5"
                />
              </svg>
            </div>
            <h2
              className="mt-4 mb-2"
              style={{
                fontFamily: 'Plus Jakarta Sans, sans-serif',
                fontWeight: 800,
              }}
            >
              Create Account
            </h2>
            <p className="text-muted" style={{ fontSize: '1.05rem' }}>
              Join VoteHub to start voting on polls
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
              {success && (
                <div
                  className="alert alert-success d-flex align-items-center rounded-3 border-0 shadow-sm mb-4"
                  role="alert"
                >
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    width="18"
                    height="18"
                    fill="currentColor"
                    className="bi bi-check-circle-fill flex-shrink-0 me-2"
                    viewBox="0 0 16 16"
                  >
                    <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0m-3.97-3.03a.75.75 0 0 0-1.08.022L7.477 9.417 5.384 7.323a.75.75 0 0 0-1.06 1.06L6.97 11.03a.75.75 0 0 0 1.079-.02l3.992-4.99a.75.75 0 0 0-.01-1.05z" />
                  </svg>
                  <div className="fw-medium">{success}</div>
                </div>
              )}

              <form onSubmit={formik.handleSubmit}>
                <div className="mb-4">
                  <label
                    htmlFor="username"
                    className="form-label fw-medium mb-2"
                  >
                    Username
                  </label>
                  <div className="input-group">
                    <span className="input-group-text bg-light border-end-0">
                      <svg
                        xmlns="http://www.w3.org/2000/svg"
                        width="16"
                        height="16"
                        fill="currentColor"
                        className="bi bi-person"
                        viewBox="0 0 16 16"
                      >
                        <path d="M8 8a3 3 0 1 0 0-6 3 3 0 0 0 0 6m2-3a2 2 0 1 1-4 0 2 2 0 0 1 4 0m4 8c0 1-1 1-1 1H3s-1 0-1-1 1-4 6-4 6 3 6 4m-1-.004c-.001-.246-.154-.986-.832-1.664C11.516 10.68 10.289 10 8 10c-2.29 0-3.516.68-4.168 1.332-.678.678-.83 1.418-.832 1.664z" />
                      </svg>
                    </span>
                    <input
                      type="text"
                      className={`form-control form-control-lg ${
                        formik.touched.username && formik.errors.username
                          ? 'is-invalid'
                          : ''
                      }`}
                      id="username"
                      name="username"
                      placeholder="Choose a username"
                      onChange={formik.handleChange}
                      onBlur={formik.handleBlur}
                      value={formik.values.username}
                    />
                    {formik.touched.username && formik.errors.username && (
                      <div className="invalid-feedback">
                        {formik.errors.username as string}
                      </div>
                    )}
                  </div>
                </div>

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
                  <label
                    htmlFor="password"
                    className="form-label fw-medium mb-2"
                  >
                    Password
                  </label>
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
                      placeholder="Create a password"
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

                <div className="mb-4">
                  <label
                    htmlFor="confirmPassword"
                    className="form-label fw-medium mb-2"
                  >
                    Confirm Password
                  </label>
                  <div className="input-group">
                    <span className="input-group-text bg-light border-end-0">
                      <svg
                        xmlns="http://www.w3.org/2000/svg"
                        width="16"
                        height="16"
                        fill="currentColor"
                        className="bi bi-shield-lock"
                        viewBox="0 0 16 16"
                      >
                        <path d="M5.338 1.59a61 61 0 0 0-2.837.856.48.48 0 0 0-.328.39c-.554 4.157.726 7.19 2.253 9.188a10.7 10.7 0 0 0 2.287 2.233c.346.244.652.42.893.533q.126.064.207.1a.73.73 0 0 0 .103.041c.046.01.073.021.117.021.044 0 .07-.01.117-.02a.74.74 0 0 0 .102-.04l.207-.1c.241-.115.547-.29.893-.533a10.7 10.7 0 0 0 2.287-2.233c1.527-1.997 2.807-5.031 2.253-9.188a.48.48 0 0 0-.328-.39c-.651-.213-1.75-.56-2.837-.855C9.552 1.29 8.531 1.067 8 1.067c-.53 0-1.552.223-2.662.524zM5.072.56C6.157.265 7.31 0 8 0s1.843.265 2.928.56c1.11.3 2.229.655 2.887.87a1.54 1.54 0 0 1 1.044 1.262c.596 4.477-.787 7.795-2.465 9.99a11.8 11.8 0 0 1-2.517 2.453 7 7 0 0 1-1.048.625c-.28.132-.581.24-.829.24s-.548-.108-.829-.24a7 7 0 0 1-1.048-.625 11.8 11.8 0 0 1-2.517-2.453C1.928 10.487.545 7.169 1.141 2.692A1.54 1.54 0 0 1 2.185 1.43 63 63 0 0 1 5.072.56z" />
                        <path d="M9.5 6.5a1.5 1.5 0 0 1-1 1.415l.385 1.99a.5.5 0 0 1-.491.595h-.788a.5.5 0 0 1-.49-.595l.384-1.99a1.5 1.5 0 1 1 2-1.415z" />
                      </svg>
                    </span>
                    <input
                      type="password"
                      className={`form-control form-control-lg ${
                        formik.touched.confirmPassword &&
                        formik.errors.confirmPassword
                          ? 'is-invalid'
                          : ''
                      }`}
                      id="confirmPassword"
                      name="confirmPassword"
                      placeholder="Confirm your password"
                      onChange={formik.handleChange}
                      onBlur={formik.handleBlur}
                      value={formik.values.confirmPassword}
                    />
                    {formik.touched.confirmPassword &&
                      formik.errors.confirmPassword && (
                        <div className="invalid-feedback">
                          {formik.errors.confirmPassword as string}
                        </div>
                      )}
                  </div>
                </div>

                <div className="d-grid mt-4">
                  <button
                    type="submit"
                    className="btn btn-secondary btn-lg py-3"
                    disabled={formik.isSubmitting}
                    style={{
                      background: 'var(--gradient-secondary)',
                      border: 'none',
                      transition: 'all 0.3s ease',
                    }}
                  >
                    {formik.isSubmitting ? (
                      <>
                        <span
                          className="spinner-border spinner-border-sm me-2"
                          role="status"
                          aria-hidden="true"
                        ></span>
                        Creating Account...
                      </>
                    ) : (
                      'Create Account'
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
              Already have an account?{' '}
              <a href="/login" className="fw-semibold text-primary">
                Sign in here
              </a>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Register;
