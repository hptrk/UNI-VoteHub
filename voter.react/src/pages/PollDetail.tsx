import { useEffect, useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import type { PollDTO } from '../api/models/PollDTO';
import pollsService from '../api/client/pollsService';
import { useAuth } from '../contexts/AuthContext';

const PollDetail = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();
  const [poll, setPoll] = useState<PollDTO | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => {
    const fetchPoll = async () => {
      try {
        if (!id) return;

        setLoading(true);
        setError('');
        const data = await pollsService.getPollById(id);
        setPoll(data);
      } catch (err: any) {
        setError(err.detail || 'Failed to load poll details');
      } finally {
        setLoading(false);
      }
    };

    fetchPoll();
  }, [id]);

  const formik = useFormik({
    initialValues: {
      optionId: '',
    },
    validationSchema: Yup.object({
      optionId: Yup.string().required('Please select an option'),
    }),
    onSubmit: async (values) => {
      try {
        if (!id || !isAuthenticated) return;

        setError('');
        await pollsService.vote({
          pollId: parseInt(id),
          optionId: parseInt(values.optionId),
        });

        setSuccess('Your vote has been recorded successfully!');

        // refresh poll data
        const updatedPoll = await pollsService.getPollById(id);
        setPoll(updatedPoll);

        setTimeout(() => {
          setSuccess('');
        }, 3000);
      } catch (err: any) {
        setError(err.detail || 'Failed to cast your vote');
      }
    },
  });

  if (loading) {
    return (
      <div className="container mt-5 pt-3">
        <div
          className="d-flex justify-content-center align-items-center"
          style={{ minHeight: '300px' }}
        >
          <div className="text-center">
            <div
              className="spinner-border text-primary"
              role="status"
              style={{ width: '3rem', height: '3rem' }}
            >
              <span className="visually-hidden">Loading...</span>
            </div>
            <p className="mt-3 text-muted">Loading poll details...</p>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mt-5 pt-3">
        <div
          className="alert alert-danger d-flex align-items-center rounded-3 border-0 shadow-sm fade-in"
          role="alert"
          style={{ borderRadius: '12px' }}
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            width="24"
            height="24"
            fill="currentColor"
            className="bi bi-exclamation-triangle-fill flex-shrink-0 me-3"
            viewBox="0 0 16 16"
          >
            <path d="M8.982 1.566a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767L8.982 1.566zM8 5c.535 0 .954.462.9.995l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 5.995A.905.905 0 0 1 8 5zm.002 6a1 1 0 1 1 0 2 1 1 0 0 1 0-2z" />
          </svg>
          <div className="fw-medium">{error}</div>
        </div>
        <div className="text-center mt-4">
          <button
            className="btn btn-outline-primary"
            onClick={() => navigate(-1)}
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="16"
              height="16"
              fill="currentColor"
              className="bi bi-arrow-left me-2"
              viewBox="0 0 16 16"
            >
              <path
                fillRule="evenodd"
                d="M15 8a.5.5 0 0 0-.5-.5H2.707l3.147-3.146a.5.5 0 1 0-.708-.708l-4 4a.5.5 0 0 0 0 .708l4 4a.5.5 0 0 0 .708-.708L2.707 8.5H14.5A.5.5 0 0 0 15 8z"
              />
            </svg>
            Go Back
          </button>
        </div>
      </div>
    );
  }

  if (!poll) {
    return (
      <div className="container mt-5 pt-3">
        <div className="text-center py-5 fade-in">
          <svg
            xmlns="http://www.w3.org/2000/svg"
            width="64"
            height="64"
            fill="currentColor"
            className="bi bi-question-circle text-secondary mb-3"
            viewBox="0 0 16 16"
          >
            <path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z" />
            <path d="M5.255 5.786a.237.237 0 0 0 .241.247h.825c.138 0 .248-.113.266-.25.09-.656.54-1.134 1.342-1.134.686 0 1.314.343 1.314 1.168 0 .635-.374.927-.965 1.371-.673.489-1.206 1.06-1.168 1.987l.003.217a.25.25 0 0 0 .25.246h.811a.25.25 0 0 0 .25-.25v-.105c0-.718.273-.927 1.01-1.486.609-.463 1.244-.977 1.244-2.056 0-1.511-1.276-2.241-2.673-2.241-1.267 0-2.655.59-2.75 2.286zm1.557 5.763c0 .533.425.927 1.01.927.609 0 1.028-.394 1.028-.927 0-.552-.42-.94-1.029-.94-.584 0-1.009.388-1.009.94z" />
          </svg>
          <h3 className="mb-3">Poll Not Found</h3>
          <p className="text-muted mb-4">
            The poll you're looking for doesn't exist or has been removed.
          </p>
          <button className="btn btn-primary" onClick={() => navigate('/')}>
            View All Polls
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="container mt-5 pt-3">
      <div className="row justify-content-center">
        <div className="col-lg-8">
          <div className="mb-4 d-flex justify-content-between align-items-center fade-in">
            <button
              className="btn btn-outline-secondary btn-sm px-3 d-flex align-items-center"
              onClick={() => navigate(-1)}
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="16"
                height="16"
                fill="currentColor"
                className="bi bi-arrow-left me-2"
                viewBox="0 0 16 16"
              >
                <path
                  fillRule="evenodd"
                  d="M15 8a.5.5 0 0 0-.5-.5H2.707l3.147-3.146a.5.5 0 1 0-.708-.708l-4 4a.5.5 0 0 0 0 .708l4 4a.5.5 0 0 0 .708-.708L2.707 8.5H14.5A.5.5 0 0 0 15 8z"
                />
              </svg>
              Back to Polls
            </button>

            <div>
              {new Date(poll.endDate) < new Date() ? (
                <span className="badge bg-danger p-2 d-flex align-items-center">
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    width="12"
                    height="12"
                    fill="currentColor"
                    className="bi bi-lock-fill me-1"
                    viewBox="0 0 16 16"
                  >
                    <path d="M8 1a2 2 0 0 1 2 2v4H6V3a2 2 0 0 1 2-2zm3 6V3a3 3 0 0 0-6 0v4a2 2 0 0 0-2 2v5a2 2 0 0 0 2 2h6a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2z" />
                  </svg>
                  Closed
                </span>
              ) : (
                <span className="badge bg-success p-2 d-flex align-items-center">
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    width="12"
                    height="12"
                    fill="currentColor"
                    className="bi bi-unlock-fill me-1"
                    viewBox="0 0 16 16"
                  >
                    <path d="M11 1a2 2 0 0 0-2 2v4a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V9a2 2 0 0 1 2-2h5V3a3 3 0 0 1 6 0v4a.5.5 0 0 1-1 0V3a2 2 0 0 0-2-2z" />
                  </svg>
                  Open
                </span>
              )}
            </div>
          </div>

          <div
            className="card border-0 shadow-sm slide-in"
            style={{ borderRadius: '16px', overflow: 'hidden' }}
          >
            <div
              className="card-header p-4"
              style={{
                background: 'var(--gradient-primary)',
                borderBottom: 'none',
              }}
            >
              <h2
                className="mb-0 text-white"
                style={{
                  fontFamily: 'Plus Jakarta Sans, sans-serif',
                  fontWeight: 700,
                }}
              >
                {poll.question}
              </h2>
            </div>
            <div className="card-body p-4">
              {success && (
                <div
                  className="alert alert-success d-flex align-items-center rounded-3 border-0 shadow-sm mb-4 fade-in"
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

              <div className="d-flex justify-content-between mb-4 pb-3 border-bottom">
                <div className="d-flex align-items-center">
                  <div
                    className="rounded-circle p-2 bg-light me-3 d-flex align-items-center justify-content-center"
                    style={{ width: '42px', height: '42px' }}
                  >
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      width="18"
                      height="18"
                      fill="var(--primary-color)"
                      className="bi bi-calendar-event"
                      viewBox="0 0 16 16"
                    >
                      <path d="M11 6.5a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 .5.5v1a.5.5 0 0 1-.5.5h-1a.5.5 0 0 1-.5-.5v-1z" />
                      <path d="M3.5 0a.5.5 0 0 1 .5.5V1h8V.5a.5.5 0 0 1 1 0V1h1a2 2 0 0 1 2 2v11a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2V3a2 2 0 0 1 2-2h1V.5a.5.5 0 0 1 .5-.5zM1 4v10a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V4H1z" />
                    </svg>
                  </div>
                  <div>
                    <div className="text-muted small">Start Date</div>
                    <div className="fw-medium">
                      {new Date(poll.startDate).toLocaleDateString(undefined, {
                        year: 'numeric',
                        month: 'short',
                        day: 'numeric',
                      })}
                    </div>
                  </div>
                </div>

                <div className="d-flex align-items-center">
                  <div
                    className="rounded-circle p-2 bg-light me-3 d-flex align-items-center justify-content-center"
                    style={{ width: '42px', height: '42px' }}
                  >
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      width="18"
                      height="18"
                      fill="var(--primary-color)"
                      className="bi bi-calendar-x"
                      viewBox="0 0 16 16"
                    >
                      <path d="M6.146 7.146a.5.5 0 0 1 .708 0L8 8.293l1.146-1.147a.5.5 0 1 1 .708.708L8.707 9l1.147 1.146a.5.5 0 0 1-.708.708L8 9.707l-1.146 1.147a.5.5 0 0 1-.708-.708L7.293 9 6.146 7.854a.5.5 0 0 1 0-.708z" />
                      <path d="M3.5 0a.5.5 0 0 1 .5.5V1h8V.5a.5.5 0 0 1 1 0V1h1a2 2 0 0 1 2 2v11a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2V3a2 2 0 0 1 2-2h1V.5a.5.5 0 0 1 .5-.5zM1 4v10a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V4H1z" />
                    </svg>
                  </div>
                  <div>
                    <div className="text-muted small">End Date</div>
                    <div className="fw-medium">
                      {new Date(poll.endDate).toLocaleDateString(undefined, {
                        year: 'numeric',
                        month: 'short',
                        day: 'numeric',
                      })}
                    </div>
                  </div>
                </div>
              </div>

              {!isAuthenticated ? (
                <div
                  className="alert alert-warning d-flex align-items-center rounded-3 border-0 shadow-sm p-4 mt-3 fade-in"
                  role="alert"
                >
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    width="24"
                    height="24"
                    fill="currentColor"
                    className="bi bi-person-lock flex-shrink-0 me-3"
                    viewBox="0 0 16 16"
                  >
                    <path d="M11 5a3 3 0 1 1-6 0 3 3 0 0 1 6 0ZM8 7a2 2 0 1 0 0-4 2 2 0 0 0 0 4Zm0 5.996V14H3s-1 0-1-1 1-4 6-4c.564 0 1.077.038 1.544.107a4.524 4.524 0 0 0-.803.918A10.46 10.46 0 0 0 8 10c-2.29 0-3.516.68-4.168 1.332-.678.678-.83 1.418-.832 1.664h5ZM9 13a1 1 0 0 1 1-1v-1a2 2 0 1 1 4 0v1a1 1 0 0 1 1 1v2a1 1 0 0 1-1 1h-4a1 1 0 0 1-1-1v-2Zm3-3a1 1 0 0 0-1 1v1h2v-1a1 1 0 0 0-1-1Z" />
                  </svg>
                  <div>
                    <p className="mb-0 fw-medium">
                      You need to be logged in to vote
                    </p>
                    <div className="mt-2">
                      <Link to="/login" className="btn btn-primary btn-sm me-2">
                        Sign In
                      </Link>
                      <Link
                        to="/register"
                        className="btn btn-outline-primary btn-sm"
                      >
                        Register
                      </Link>
                    </div>
                  </div>
                </div>
              ) : poll.userHasVoted ? (
                <div className="mt-3">
                  <div
                    className="alert alert-success d-flex align-items-center rounded-3 border-0 shadow-sm p-4 mb-4 fade-in"
                    role="alert"
                  >
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      width="24"
                      height="24"
                      fill="currentColor"
                      className="bi bi-check2-circle flex-shrink-0 me-3"
                      viewBox="0 0 16 16"
                    >
                      <path d="M2.5 8a5.5 5.5 0 0 1 8.25-4.764.5.5 0 0 0 .5-.866A6.5 6.5 0 1 0 14.5 8a.5.5 0 0 0-1 0 5.5 5.5 0 1 1-11 0z" />
                      <path d="M15.354 3.354a.5.5 0 0 0-.708-.708L8 9.293 5.354 6.646a.5.5 0 1 0-.708.708l3 3a.5.5 0 0 0 .708 0l7-7z" />
                    </svg>
                    <div>
                      <p className="mb-0 fw-medium">You voted for:</p>
                      <p className="mb-0 fs-5 fw-bold mt-1">
                        "{poll.options.find((option) => option.userVoted)?.text}
                        "
                      </p>
                    </div>
                  </div>

                  <Link
                    to={`/polls/${poll.id}/results`}
                    className="btn btn-primary w-100 py-3"
                  >
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      width="16"
                      height="16"
                      fill="currentColor"
                      className="bi bi-bar-chart-fill me-2"
                      viewBox="0 0 16 16"
                    >
                      <path d="M1 11a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1H2a1 1 0 0 1-1-1v-3zm5-4a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v7a1 1 0 0 1-1 1H7a1 1 0 0 1-1-1V7zm5-5a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v12a1 1 0 0 1-1 1h-2a1 1 0 0 1-1-1V2z" />
                    </svg>
                    View Poll Results
                  </Link>
                </div>
              ) : (
                <form onSubmit={formik.handleSubmit} className="mt-3 fade-in">
                  <h5 className="fw-bold mb-3">Cast Your Vote</h5>
                  <div className="mb-4">
                    <div className="voting-options">
                      {poll.options.map((option) => (
                        <div
                          key={option.id}
                          className={`voting-option mb-3 position-relative ${
                            formik.values.optionId === option.id.toString()
                              ? 'active'
                              : ''
                          }`}
                        >
                          <input
                            type="radio"
                            name="optionId"
                            id={`option-${option.id}`}
                            value={option.id}
                            onChange={formik.handleChange}
                            onBlur={formik.handleBlur}
                            checked={
                              formik.values.optionId === option.id.toString()
                            }
                            className="position-absolute opacity-0"
                            style={{
                              width: '100%',
                              height: '100%',
                              cursor: 'pointer',
                            }}
                          />
                          <label
                            htmlFor={`option-${option.id}`}
                            className="d-flex align-items-center p-3 rounded-3 border"
                            style={{
                              cursor: 'pointer',
                              transition: 'all 0.2s ease',
                              borderColor:
                                formik.values.optionId === option.id.toString()
                                  ? 'var(--primary-color)'
                                  : '#dee2e6',
                              backgroundColor:
                                formik.values.optionId === option.id.toString()
                                  ? 'rgba(99, 102, 241, 0.08)'
                                  : '',
                              transform:
                                formik.values.optionId === option.id.toString()
                                  ? 'translateY(-2px)'
                                  : '',
                              boxShadow:
                                formik.values.optionId === option.id.toString()
                                  ? '0 5px 15px -5px rgba(99, 102, 241, 0.3)'
                                  : '',
                            }}
                          >
                            <span
                              className="me-3 rounded-circle d-flex align-items-center justify-content-center"
                              style={{
                                width: '24px',
                                height: '24px',
                                border:
                                  formik.values.optionId ===
                                  option.id.toString()
                                    ? '0'
                                    : '2px solid #dee2e6',
                                backgroundColor:
                                  formik.values.optionId ===
                                  option.id.toString()
                                    ? 'var(--primary-color)'
                                    : 'transparent',
                              }}
                            >
                              {formik.values.optionId ===
                                option.id.toString() && (
                                <svg
                                  xmlns="http://www.w3.org/2000/svg"
                                  width="14"
                                  height="14"
                                  fill="white"
                                  className="bi bi-check"
                                  viewBox="0 0 16 16"
                                >
                                  <path d="M10.97 4.97a.75.75 0 0 1 1.07 1.05l-3.99 4.99a.75.75 0 0 1-1.08.02L4.324 8.384a.75.75 0 1 1 1.06-1.06l2.094 2.093 3.473-4.425a.267.267 0 0 1 .02-.022z" />
                                </svg>
                              )}
                            </span>
                            <span className="fw-medium fs-5">
                              {option.text}
                            </span>
                          </label>
                        </div>
                      ))}
                    </div>
                    {formik.touched.optionId && formik.errors.optionId && (
                      <div className="text-danger mt-2 mb-3">
                        <svg
                          xmlns="http://www.w3.org/2000/svg"
                          width="16"
                          height="16"
                          fill="currentColor"
                          className="bi bi-exclamation-circle me-2"
                          viewBox="0 0 16 16"
                        >
                          <path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z" />
                          <path d="M7.002 11a1 1 0 1 1 2 0 1 1 0 0 1-2 0zM7.1 4.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 4.995z" />
                        </svg>
                        {formik.errors.optionId}
                      </div>
                    )}
                  </div>
                  <div className="mt-4">
                    <button
                      type="submit"
                      className="btn btn-primary w-100 py-3"
                      disabled={formik.isSubmitting}
                    >
                      {formik.isSubmitting ? (
                        <>
                          <span
                            className="spinner-border spinner-border-sm me-2"
                            role="status"
                            aria-hidden="true"
                          ></span>
                          Submitting Vote...
                        </>
                      ) : (
                        <>
                          <svg
                            xmlns="http://www.w3.org/2000/svg"
                            width="16"
                            height="16"
                            fill="currentColor"
                            className="bi bi-check2-square me-2"
                            viewBox="0 0 16 16"
                          >
                            <path d="M3 14.5A1.5 1.5 0 0 1 1.5 13V3A1.5 1.5 0 0 1 3 1.5h8a.5.5 0 0 1 0 1H3a.5.5 0 0 0-.5.5v10a.5.5 0 0 0 .5.5h10a.5.5 0 0 0 .5-.5V8a.5.5 0 0 1 1 0v5a1.5 1.5 0 0 1-1.5 1.5H3z" />
                            <path d="M8.354 10.354l7-7a.5.5 0 0 0-.708-.708L8 9.293 5.354 6.646a.5.5 0 1 0-.708.708l3 3a.5.5 0 0 0 .708 0z" />
                          </svg>
                          Submit Vote
                        </>
                      )}
                    </button>
                  </div>
                </form>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default PollDetail;
